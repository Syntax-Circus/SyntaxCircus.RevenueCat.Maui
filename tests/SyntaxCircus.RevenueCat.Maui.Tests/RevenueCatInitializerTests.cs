namespace SyntaxCircus.RevenueCat.Maui.Tests;

public class RevenueCatInitializerTests
{
    [Fact]
    public void TryInitialize_NullBilling_ThrowsArgumentNullException()
        => Should.Throw<ArgumentNullException>(() =>
            RevenueCatInitializer.TryInitialize(null!, new RevenueCatBillingOptions()));

    [Fact]
    public void TryInitialize_NullOptions_ThrowsArgumentNullException()
        => Should.Throw<ArgumentNullException>(() =>
            RevenueCatInitializer.TryInitialize(Substitute.For<IRevenueCatBilling>(), null!));

    // Built under plain net10.0 (a unit-test host TFM, not android/ios), so ResolvePlatformApiKey's
    // #if ANDROID / #elif IOS branches compile to the #else branch here — it always resolves an
    // empty key regardless of what's configured, and TryInitialize always returns false. The real
    // per-platform key resolution is exercised only on an actual android/ios build, which this
    // suite can't host — documented gap, not a bug in this test.
    [Fact]
    public void TryInitialize_NonPlatformTestHost_NeverResolvesAKey_ReturnsFalse()
    {
        var billing = Substitute.For<IRevenueCatBilling>();
        var options = new RevenueCatBillingOptions { AndroidApiKey = "android_key", IosApiKey = "ios_key" };

        var result = RevenueCatInitializer.TryInitialize(billing, options);

        result.ShouldBeFalse();
        billing.DidNotReceiveWithAnyArgs().Initialize(default!);
    }

    [Fact]
    public void TryInitialize_NoKeysConfigured_ReturnsFalse()
    {
        var billing = Substitute.For<IRevenueCatBilling>();

        var result = RevenueCatInitializer.TryInitialize(billing, new RevenueCatBillingOptions());

        result.ShouldBeFalse();
    }
}
