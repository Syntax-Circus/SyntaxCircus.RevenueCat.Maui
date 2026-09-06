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
    /// <c>Initialize</c> if no key is configured for the current platform. When
    /// <paramref name="appUserId"/> is supplied, initializes with that user id directly instead of
    /// creating an anonymous user — prefer this when the app's user id is already known at startup.
    /// </summary>
    public static bool TryInitialize(IRevenueCatBilling billing, RevenueCatBillingOptions options, string? appUserId = null)
    {
        ArgumentNullException.ThrowIfNull(billing);
        ArgumentNullException.ThrowIfNull(options);

        var platform = GetCompileTimePlatform();
        if (platform is null)
        {
            return false;
        }

        return TryInitialize(billing, options, platform.Value, appUserId);
    }

    /// <summary>
    /// Initializes <paramref name="billing"/> for an explicit platform. This avoids compile-time
    /// platform symbol checks and is useful in tests or other non-mobile hosts. When
    /// <paramref name="appUserId"/> is supplied, initializes with that user id directly instead of
    /// creating an anonymous user.
    /// </summary>
    public static bool TryInitialize(IRevenueCatBilling billing, RevenueCatBillingOptions options, RevenueCatPlatform platform, string? appUserId = null)
    {
        ArgumentNullException.ThrowIfNull(billing);
        ArgumentNullException.ThrowIfNull(options);

        var apiKey = ResolvePlatformApiKey(options, platform);
        if (string.IsNullOrEmpty(apiKey))
        {
            return false;
        }

        InitializeBilling(billing, apiKey, appUserId);
        return true;
    }

    /// <summary>
    /// Initializes <paramref name="billing"/> using a custom API-key resolver. The resolver can
    /// decide which key to use without depending on compile-time platform symbols. When
    /// <paramref name="appUserId"/> is supplied, initializes with that user id directly instead of
    /// creating an anonymous user.
    /// </summary>
    public static bool TryInitialize(
        IRevenueCatBilling billing,
        RevenueCatBillingOptions options,
        Func<RevenueCatBillingOptions, string?> apiKeyResolver,
        string? appUserId = null)
    {
        ArgumentNullException.ThrowIfNull(billing);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(apiKeyResolver);

        var apiKey = apiKeyResolver(options);
        if (string.IsNullOrEmpty(apiKey))
        {
            return false;
        }

        InitializeBilling(billing, apiKey, appUserId);
        return true;
    }

    private static void InitializeBilling(IRevenueCatBilling billing, string apiKey, string? appUserId)
    {
        if (string.IsNullOrWhiteSpace(appUserId))
        {
            billing.Initialize(apiKey);
        }
        else
        {
            billing.Initialize(apiKey, appUserId);
        }
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
