namespace SyntaxCircus.RevenueCat.Maui.Tests;

public class RevenueCatManagementUrlTests
{
    [Fact]
    public async Task GetAsync_NullBilling_ThrowsArgumentNullException()
        => await Should.ThrowAsync<ArgumentNullException>(() =>
            RevenueCatManagementUrl.GetAsync(null!, TestContext.Current.CancellationToken));

    [Fact]
    public async Task GetAsync_ValidCall_ReturnsUrlFromBilling()
    {
        var billing = Substitute.For<IRevenueCatBilling>();
        billing.GetManagementSubscriptionUrl(Arg.Any<CancellationToken>()).Returns("https://apps.apple.com/account/subscriptions");

        var result = await RevenueCatManagementUrl.GetAsync(billing, TestContext.Current.CancellationToken);

        result.ShouldBe("https://apps.apple.com/account/subscriptions");
        await billing.Received(1).GetManagementSubscriptionUrl(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAsync_BillingThrows_Propagates()
    {
        var billing = Substitute.For<IRevenueCatBilling>();
        billing.GetManagementSubscriptionUrl(Arg.Any<CancellationToken>()).ThrowsAsync(new InvalidOperationException("boom"));

        await Should.ThrowAsync<InvalidOperationException>(() =>
            RevenueCatManagementUrl.GetAsync(billing, TestContext.Current.CancellationToken));
    }
}
