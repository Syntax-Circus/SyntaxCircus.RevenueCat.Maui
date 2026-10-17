namespace SyntaxCircus.RevenueCat.Maui;

/// <summary>
/// Reads the current app user's subscription state directly from the vendor SDK, for immediate
/// client-side UI feedback (e.g. showing a "Pro" badge without waiting on a network round trip).
/// </summary>
/// <remarks>
/// This is a convenience for perceived responsiveness only, not a source of truth: client-reported
/// entitlement info can be stale (the device hasn't synced yet) or spoofed (a jailbroken/rooted
/// device can lie to the SDK). Always verify server-side — via your backend's RevenueCat webhook
/// handling and subscriber verification — before actually granting access to paid functionality.
/// </remarks>
public static class RevenueCatCustomerInfoReader
{
    /// <summary>
    /// Reads the current app user's customer info (active subscriptions and entitlements) using
    /// <see cref="CustomerInfoFetchPolicy.CachedOrFetched"/>, which may return a stale on-device cache.
    /// </summary>
    public static Task<RevenueCatCustomerInfo> GetAsync(IRevenueCatBilling billing, CancellationToken ct = default)
        => GetAsync(billing, CustomerInfoFetchPolicy.CachedOrFetched, ct);

    /// <summary>
    /// Reads the current app user's customer info (active subscriptions and entitlements), choosing how
    /// the vendor SDK's on-device cache is used via <paramref name="fetchPolicy"/> (e.g.
    /// <see cref="CustomerInfoFetchPolicy.FetchCurrent"/> to read a change made outside the app).
    /// </summary>
    public static async Task<RevenueCatCustomerInfo> GetAsync(
        IRevenueCatBilling billing,
        CustomerInfoFetchPolicy fetchPolicy,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(billing);

        var result = await billing.GetCustomerInfo(fetchPolicy, ct).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            throw result.ErrorException
                ?? new InvalidOperationException($"Failed to get RevenueCat customer info: {result.Error}.");
        }

        return Map(result.Value!);
    }

    /// <summary>
    /// Returns <see langword="true"/> if <paramref name="customerInfo"/> has an active entitlement
    /// matching <paramref name="entitlementIdentifier"/> (case-insensitive).
    /// </summary>
    public static bool IsEntitled(RevenueCatCustomerInfo customerInfo, string entitlementIdentifier)
    {
        ArgumentNullException.ThrowIfNull(customerInfo);
        ArgumentNullException.ThrowIfNull(entitlementIdentifier);

        return customerInfo.Entitlements.Any(e =>
            e.IsActive && string.Equals(e.Identifier, entitlementIdentifier, StringComparison.OrdinalIgnoreCase));
    }

    private static RevenueCatCustomerInfo Map(CustomerInfoDto customerInfo) => new(
        ActiveSubscriptions: customerInfo.ActiveSubscriptions,
        AllPurchasedIdentifiers: customerInfo.AllPurchasedIdentifiers,
        Entitlements: customerInfo.Entitlements
            .Select(e => new RevenueCatEntitlement(
                Identifier: e.Identifier,
                IsActive: e.IsActive,
                ExpirationDate: e.ExpirationDate,
                ProductIdentifier: e.ProductIdentifier,
                WillRenew: e.WillRenew))
            .ToList(),
        LatestExpirationDate: customerInfo.LatestExpirationDate);
}
