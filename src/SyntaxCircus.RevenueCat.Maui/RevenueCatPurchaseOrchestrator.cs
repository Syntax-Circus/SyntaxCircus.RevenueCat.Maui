namespace SyntaxCircus.RevenueCat.Maui;

/// <summary>
/// Drives the store-side half of a purchase: find the package, trigger the platform purchase
/// sheet, and interpret the result. Recording the transaction against your own backend is the
/// caller's job — this only owns the vendor SDK interaction.
/// </summary>
public static partial class RevenueCatPurchaseOrchestrator
{
    /// <summary>
    /// Finds a package in the current offering by SKU or package identifier and purchases it.
    /// A user-cancelled purchase comes back as <see cref="RevenueCatPurchaseResult.WasCancelled"/>,
    /// not as a thrown exception.
    /// </summary>
    public static async Task<RevenueCatPurchaseResult> PurchaseAsync(
        IRevenueCatBilling billing,
        string productIdentifier,
        ILogger logger,
        CancellationToken ct = default)
        => await PurchaseAsync(billing, productIdentifier, logger, packageResolver: null, ct: ct).ConfigureAwait(false);

    /// <summary>
    /// Finds a package in the current offering using a caller-provided resolver and purchases it.
    /// A user-cancelled purchase comes back as <see cref="RevenueCatPurchaseResult.WasCancelled"/>,
    /// not as a thrown exception.
    /// </summary>
    public static async Task<RevenueCatPurchaseResult> PurchaseAsync(
        IRevenueCatBilling billing,
        string productIdentifier,
        ILogger logger,
        Func<IReadOnlyList<PackageDto>, string, PackageDto?>? packageResolver,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(billing);
        ArgumentNullException.ThrowIfNull(productIdentifier);
        ArgumentNullException.ThrowIfNull(logger);
        var offeringsResult = await billing.GetOfferings(forceRefresh: false, cancellationToken: ct).ConfigureAwait(false);
        if (!offeringsResult.IsSuccess)
        {
            LogPurchaseFailed(logger, offeringsResult.Error);
            return new RevenueCatPurchaseResult(Success: false, ErrorMessage: $"Store error: {offeringsResult.Error}", ErrorStatus: offeringsResult.Error);
        }

        var current = offeringsResult.Value?.GetCurrent();
        var package = current is null
            ? null
            : packageResolver?.Invoke(current.AvailablePackages, productIdentifier)
                ?? current.AvailablePackages.FirstOrDefault(p => PackageMatchesProductIdentifier(p, productIdentifier));

        if (package is null)
        {
            LogProductNotFound(logger, productIdentifier);
            return new RevenueCatPurchaseResult(Success: false, ErrorMessage: $"Product '{productIdentifier}' not found.");
        }

        var storeResult = await billing.PurchaseProduct(package, ct).ConfigureAwait(false);

        if (!storeResult.IsSuccess)
        {
            if (storeResult.Error == PurchaseErrorStatus.PurchaseCancelledError)
            {
                return new RevenueCatPurchaseResult(Success: false, WasCancelled: true, ErrorMessage: "Purchase cancelled.", ErrorStatus: storeResult.Error);
            }

            LogPurchaseFailed(logger, storeResult.Error);
            return new RevenueCatPurchaseResult(Success: false, ErrorMessage: $"Store error: {storeResult.Error}", ErrorStatus: storeResult.Error);
        }

        var revenueCatTransactionId = await ResolveRevenueCatTransactionIdAsync(storeResult.Transaction, package, logger, ct).ConfigureAwait(false);

        return new RevenueCatPurchaseResult(
            Success: true,
            TransactionId: storeResult.Transaction?.TransactionIdentifier,
            RevenueCatTransactionId: revenueCatTransactionId,
            AppUserId: billing.GetAppUserId());
    }

    /// <summary>
    /// Resolves the identifier RevenueCat's webhooks/REST API will report for this purchase.
    /// On iOS (and any other non-Android TFM) the store's own transaction identifier already
    /// matches, so this is just <see cref="StoreTransactionDto.TransactionIdentifier"/>. On
    /// Android the store identifier the app receives is the Play Billing purchase token, which
    /// RevenueCat never reports back — the matching id is the Play order id, found only via the
    /// native SDK's <c>NonSubscriptionTransactions</c>. Never throws: any failure or timeout
    /// (capped around 5s) is logged at warning and resolves to null rather than failing the purchase.
    /// </summary>
    private static Task<string?> ResolveRevenueCatTransactionIdAsync(
        StoreTransactionDto? transaction,
        PackageDto purchasedPackage,
        ILogger logger,
        CancellationToken ct)
    {
        if (transaction is null)
        {
            return Task.FromResult<string?>(null);
        }

#if ANDROID
        return ResolveAndroidRevenueCatTransactionIdAsync(transaction, purchasedPackage, logger, ct);
    }

    private static async Task<string?> ResolveAndroidRevenueCatTransactionIdAsync(
        StoreTransactionDto transaction,
        PackageDto purchasedPackage,
        ILogger logger,
        CancellationToken ct)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(AndroidResolveTimeout);

            var productIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (!string.IsNullOrWhiteSpace(transaction.ProductIdentifier))
            {
                productIds.Add(transaction.ProductIdentifier);
            }

            if (!string.IsNullOrWhiteSpace(purchasedPackage.Product.Sku))
            {
                productIds.Add(purchasedPackage.Product.Sku);
            }

            var candidates = await AndroidNonSubscriptionTransactionFetcher.GetCandidatesAsync(cts.Token).ConfigureAwait(false);
            return StoreTransactionIdSelector.Select(candidates, productIds, transaction.PurchaseDate);
        }
        catch (OperationCanceledException)
        {
            LogAndroidStoreTransactionIdResolutionTimedOut(logger);
            return null;
        }
        catch (Exception ex)
        {
            LogAndroidStoreTransactionIdResolutionFailed(logger, ex);
            return null;
        }
    }
#else
        return Task.FromResult<string?>(transaction.TransactionIdentifier);
    }
#endif

#if ANDROID
    private static readonly TimeSpan AndroidResolveTimeout = TimeSpan.FromSeconds(5);
#endif

    private static bool PackageMatchesProductIdentifier(PackageDto package, string productIdentifier)
    {
        return MatchesIdentifier(package.Identifier, productIdentifier)
            || MatchesIdentifier(package.Product.Sku, productIdentifier);
    }

    private static bool MatchesIdentifier(string? left, string right)
    {
        if (string.IsNullOrWhiteSpace(left) || string.IsNullOrWhiteSpace(right))
        {
            return false;
        }

        var trimmedLeft = left.Trim();
        var trimmedRight = right.Trim();

        return string.Equals(trimmedLeft, trimmedRight, StringComparison.OrdinalIgnoreCase)
            || string.Equals(NormalizeIdentifier(trimmedLeft), NormalizeIdentifier(trimmedRight), StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeIdentifier(string value)
        => new string(value.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();

    /// <summary>
    /// Re-syncs identity (if <paramref name="userId"/> is known) and restores prior store
    /// transactions. Store/network failures are caught and reported via
    /// <see cref="RevenueCatPurchaseResult.ErrorMessage"/> rather than thrown, since this is
    /// typically called from a "Restore Purchases" button the user can just retry.
    /// </summary>
    public static async Task<RevenueCatPurchaseResult> RestoreAsync(
        IRevenueCatBilling billing,
        string? userId,
        ILogger logger,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(billing);
        ArgumentNullException.ThrowIfNull(logger);

        try
        {
            if (!string.IsNullOrWhiteSpace(userId))
            {
                var loginResult = await billing.Login(userId, ct).ConfigureAwait(false);
                if (!loginResult.IsSuccess)
                {
                    LogRestoreFailed(logger, loginResult.ErrorException ?? new InvalidOperationException($"Login failed: {loginResult.Error}"));
                    return new RevenueCatPurchaseResult(Success: false, ErrorMessage: "Purchase restoration failed. Please try again.", ErrorStatus: loginResult.Error);
                }
            }

            var restoreResult = await billing.RestoreTransactions(ct).ConfigureAwait(false);
            if (!restoreResult.IsSuccess)
            {
                LogRestoreFailed(logger, restoreResult.ErrorException ?? new InvalidOperationException($"RestoreTransactions failed: {restoreResult.Error}"));
                return new RevenueCatPurchaseResult(Success: false, ErrorMessage: "Purchase restoration failed. Please try again.", ErrorStatus: restoreResult.Error);
            }

            return new RevenueCatPurchaseResult(Success: true, AppUserId: billing.GetAppUserId());
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            LogRestoreFailed(logger, ex);
            return new RevenueCatPurchaseResult(Success: false, ErrorMessage: "Purchase restoration failed. Please try again.");
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "RevenueCat product '{ProductIdentifier}' not found in current offering.")]
    private static partial void LogProductNotFound(ILogger logger, string productIdentifier);

    [LoggerMessage(Level = LogLevel.Error, Message = "RevenueCat purchase failed with status {ErrorStatus}.")]
    private static partial void LogPurchaseFailed(ILogger logger, PurchaseErrorStatus? errorStatus);

    [LoggerMessage(Level = LogLevel.Error, Message = "RevenueCat RestoreTransactions failed.")]
    private static partial void LogRestoreFailed(ILogger logger, Exception exception);

#if ANDROID
    [LoggerMessage(Level = LogLevel.Warning, Message = "Resolving the RevenueCat/Play order id for this purchase timed out; falling back to null.")]
    private static partial void LogAndroidStoreTransactionIdResolutionTimedOut(ILogger logger);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Failed to resolve the RevenueCat/Play order id for this purchase; falling back to null.")]
    private static partial void LogAndroidStoreTransactionIdResolutionFailed(ILogger logger, Exception exception);
#endif
}
