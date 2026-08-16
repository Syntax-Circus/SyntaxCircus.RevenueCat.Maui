namespace SyntaxCircus.RevenueCat.Maui;

/// <summary>
/// Resolves the right platform API key and initializes the RevenueCat SDK. Call once, early in
/// the app lifecycle (e.g. <c>Application.OnStart</c>).
/// </summary>
public static class RevenueCatInitializer
{
    /// <summary>
    /// Initializes <paramref name="billing"/> with the platform-appropriate key from
    /// <paramref name="options"/>. Returns <see langword="false"/> without calling
    /// <c>Initialize</c> if no key is configured for the current platform.
    /// </summary>
    public static bool TryInitialize(IRevenueCatBilling billing, RevenueCatBillingOptions options)
    {
        ArgumentNullException.ThrowIfNull(billing);
        ArgumentNullException.ThrowIfNull(options);

        var apiKey = ResolvePlatformApiKey(options);
        if (string.IsNullOrEmpty(apiKey))
        {
            return false;
        }

        billing.Initialize(apiKey);
        return true;
    }

    private static string ResolvePlatformApiKey(RevenueCatBillingOptions options)
    {
#if ANDROID
        return options.AndroidApiKey;
#elif IOS
        return options.IosApiKey;
#else
        return string.Empty;
#endif
    }
}
