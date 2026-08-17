namespace SyntaxCircus.RevenueCat.Maui;

/// <summary>
/// Thin wrapper around the vendor SDK's subscription-management URL lookup, kept behind this
/// package's abstraction like every other billing operation.
/// </summary>
public static class RevenueCatManagementUrl
{
    /// <summary>
    /// Returns the platform-specific (App Store / Play Store) or RevenueCat customer-portal deep
    /// link for the current app user to manage their subscription, or <see langword="null"/> if
    /// the SDK has none to report. Unlike the identity-sync helpers, failures are not swallowed
    /// here: this is typically used to populate a "Manage Subscription" link, and silently
    /// returning a missing/broken URL is worse for that use case than letting the caller see and
    /// handle the failure.
    /// </summary>
    public static async Task<string?> GetAsync(IRevenueCatBilling billing, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(billing);

        return await billing.GetManagementSubscriptionUrl(ct).ConfigureAwait(false);
    }
}
