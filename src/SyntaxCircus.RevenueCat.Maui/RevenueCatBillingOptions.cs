namespace SyntaxCircus.RevenueCat.Maui;

/// <summary>
/// The <b>public (publishable)</b> per-platform RevenueCat API keys, safe to ship inside the app
/// binary. Distinct from the server-side secret key used by <c>SyntaxCircus.RevenueCat</c>.
/// </summary>
public sealed class RevenueCatBillingOptions
{
    public const string SectionName = "RevenueCat";

    /// <summary>RevenueCat public API key for the Android platform.</summary>
    public string AndroidApiKey { get; set; } = string.Empty;

    /// <summary>RevenueCat public API key for the iOS platform.</summary>
    public string IosApiKey { get; set; } = string.Empty;
}
