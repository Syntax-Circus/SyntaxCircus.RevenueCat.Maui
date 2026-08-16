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

        var platform = GetCompileTimePlatform();
        if (platform is null)
        {
            return false;
        }

        return TryInitialize(billing, options, platform.Value);
    }

    /// <summary>
    /// Initializes <paramref name="billing"/> for an explicit platform. This avoids compile-time
    /// platform symbol checks and is useful in tests or other non-mobile hosts.
    /// </summary>
    public static bool TryInitialize(IRevenueCatBilling billing, RevenueCatBillingOptions options, RevenueCatPlatform platform)
    {
        ArgumentNullException.ThrowIfNull(billing);
        ArgumentNullException.ThrowIfNull(options);

        var apiKey = ResolvePlatformApiKey(options, platform);
        if (string.IsNullOrEmpty(apiKey))
        {
            return false;
        }

        billing.Initialize(apiKey);
        return true;
    }

    /// <summary>
    /// Initializes <paramref name="billing"/> using a custom API-key resolver. The resolver can
    /// decide which key to use without depending on compile-time platform symbols.
    /// </summary>
    public static bool TryInitialize(
        IRevenueCatBilling billing,
        RevenueCatBillingOptions options,
        Func<RevenueCatBillingOptions, string?> apiKeyResolver)
    {
        ArgumentNullException.ThrowIfNull(billing);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(apiKeyResolver);

        var apiKey = apiKeyResolver(options);
        if (string.IsNullOrEmpty(apiKey))
        {
            return false;
        }

        billing.Initialize(apiKey);
        return true;
    }

    private static RevenueCatPlatform? GetCompileTimePlatform()
    {
#if ANDROID
        return RevenueCatPlatform.Android;
#elif IOS
        return RevenueCatPlatform.Ios;
#else
        return null;
#endif
    }

    private static string? ResolvePlatformApiKey(RevenueCatBillingOptions options, RevenueCatPlatform platform)
        => platform switch
        {
            RevenueCatPlatform.Android => options.AndroidApiKey,
            RevenueCatPlatform.Ios => options.IosApiKey,
            _ => null,
        };
}

/// <summary>Explicit platform selector for RevenueCat initialization.</summary>
public enum RevenueCatPlatform
{
    Android,
    Ios,
}
