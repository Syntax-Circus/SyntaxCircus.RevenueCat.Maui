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
    {
        ArgumentNullException.ThrowIfNull(billing);
        ArgumentNullException.ThrowIfNull(productIdentifier);
        ArgumentNullException.ThrowIfNull(logger);

        var offerings = await billing.GetOfferings(forceRefresh: false, cancellationToken: ct).ConfigureAwait(false);
        var current = offerings.GetCurrent();
        var package = current?.AvailablePackages.FirstOrDefault(p =>
            string.Equals(p.Product.Sku, productIdentifier, StringComparison.OrdinalIgnoreCase)
            || string.Equals(p.Identifier, productIdentifier, StringComparison.OrdinalIgnoreCase));

        if (package is null)
        {
            LogProductNotFound(logger, productIdentifier);
            return new RevenueCatPurchaseResult(Success: false, ErrorMessage: $"Product '{productIdentifier}' not found.");
        }

        var storeResult = await billing.PurchaseProduct(package, ct).ConfigureAwait(false);

        if (!storeResult.IsSuccess)
        {
            if (storeResult.ErrorStatus == PurchaseErrorStatus.PurchaseCancelledError)
            {
                return new RevenueCatPurchaseResult(Success: false, WasCancelled: true, ErrorMessage: "Purchase cancelled.");
            }

            LogPurchaseFailed(logger, storeResult.ErrorStatus);
            return new RevenueCatPurchaseResult(Success: false, ErrorMessage: $"Store error: {storeResult.ErrorStatus}");
        }

        return new RevenueCatPurchaseResult(
            Success: true,
            TransactionId: storeResult.Transaction?.TransactionIdentifier,
            AppUserId: billing.GetAppUserId());
    }

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
                await billing.Login(userId, ct).ConfigureAwait(false);
            }

            await billing.RestoreTransactions(ct).ConfigureAwait(false);
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
}
