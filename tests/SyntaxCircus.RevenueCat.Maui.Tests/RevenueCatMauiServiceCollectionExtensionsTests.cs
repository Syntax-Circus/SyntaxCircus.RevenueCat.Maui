namespace SyntaxCircus.RevenueCat.Maui.Tests;

public class RevenueCatMauiServiceCollectionExtensionsTests
{
    private static IConfiguration BuildConfiguration(Dictionary<string, string?> values)
        => new ConfigurationBuilder().AddInMemoryCollection(values).Build();

    [Fact]
    public void AddRevenueCatMaui_NullServices_ThrowsArgumentNullException()
        => Should.Throw<ArgumentNullException>(() =>
            RevenueCatMauiServiceCollectionExtensions.AddRevenueCatMaui(null!, BuildConfiguration([])));

    [Fact]
    public void AddRevenueCatMaui_NullConfiguration_ThrowsArgumentNullException()
    {
        var services = new ServiceCollection();

        Should.Throw<ArgumentNullException>(() => services.AddRevenueCatMaui(null!));
    }

    [Fact]
    public void AddRevenueCatMaui_BindsOptionsFromConfiguration()
    {
        var services = new ServiceCollection();
        services.AddRevenueCatMaui(BuildConfiguration(new Dictionary<string, string?>
        {
            ["RevenueCat:AndroidApiKey"] = "android_key",
            ["RevenueCat:IosApiKey"] = "ios_key",
        }));

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<RevenueCatBillingOptions>>().Value;

        options.AndroidApiKey.ShouldBe("android_key");
        options.IosApiKey.ShouldBe("ios_key");
    }

    // Doesn't resolve IRevenueCatBilling itself — the vendor's real implementation is
    // platform-backed and this test host is plain net10.0, not android/ios.
    [Fact]
    public void AddRevenueCatMaui_RegistersRevenueCatBillingService()
    {
        var services = new ServiceCollection();

        services.AddRevenueCatMaui(BuildConfiguration([]));

        services.ShouldContain(descriptor => descriptor.ServiceType == typeof(IRevenueCatBilling));
    }
}
