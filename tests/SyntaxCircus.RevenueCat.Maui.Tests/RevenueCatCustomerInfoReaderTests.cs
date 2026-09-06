namespace SyntaxCircus.RevenueCat.Maui.Tests;

public class RevenueCatCustomerInfoReaderTests
{
    [Fact]
    public async Task GetAsync_NullBilling_ThrowsArgumentNullException()
        => await Should.ThrowAsync<ArgumentNullException>(() =>
            RevenueCatCustomerInfoReader.GetAsync(null!, TestContext.Current.CancellationToken));

    [Fact]
    public async Task GetAsync_ValidCall_MapsCustomerInfo()
    {
        var billing = Substitute.For<IRevenueCatBilling>();
        var customerInfo = TestFactories.CreateCustomerInfo(
            activeSubscriptions: ["sku_monthly"],
            entitlements: [TestFactories.CreateEntitlement("pro", isActive: true)]);
        billing.GetCustomerInfo(Arg.Any<CancellationToken>()).Returns(new CustomerInfoResultDto { Value = customerInfo });

        var result = await RevenueCatCustomerInfoReader.GetAsync(billing, TestContext.Current.CancellationToken);

        result.ActiveSubscriptions.ShouldBe(["sku_monthly"]);
        result.Entitlements.Count.ShouldBe(1);
        result.Entitlements[0].Identifier.ShouldBe("pro");
        result.Entitlements[0].IsActive.ShouldBeTrue();
        result.Entitlements[0].ProductIdentifier.ShouldBe("sku_monthly");
    }

    [Fact]
    public async Task GetAsync_BillingReturnsError_Throws()
    {
        var billing = Substitute.For<IRevenueCatBilling>();
        billing.GetCustomerInfo(Arg.Any<CancellationToken>())
            .Returns(new CustomerInfoResultDto { ErrorException = new InvalidOperationException("boom") });

        await Should.ThrowAsync<InvalidOperationException>(() =>
            RevenueCatCustomerInfoReader.GetAsync(billing, TestContext.Current.CancellationToken));
    }

    [Fact]
    public void IsEntitled_NullCustomerInfo_ThrowsArgumentNullException()
        => Should.Throw<ArgumentNullException>(() =>
            RevenueCatCustomerInfoReader.IsEntitled(null!, "pro"));

    [Fact]
    public void IsEntitled_ActiveMatchingEntitlement_ReturnsTrue()
    {
        var customerInfo = new RevenueCatCustomerInfo(
            ActiveSubscriptions: [],
            AllPurchasedIdentifiers: [],
            Entitlements: [new RevenueCatEntitlement("pro", IsActive: true, ExpirationDate: null, ProductIdentifier: "sku_monthly", WillRenew: true)],
            LatestExpirationDate: null);

        RevenueCatCustomerInfoReader.IsEntitled(customerInfo, "PRO").ShouldBeTrue();
    }

    [Fact]
    public void IsEntitled_InactiveEntitlement_ReturnsFalse()
    {
        var customerInfo = new RevenueCatCustomerInfo(
            ActiveSubscriptions: [],
            AllPurchasedIdentifiers: [],
            Entitlements: [new RevenueCatEntitlement("pro", IsActive: false, ExpirationDate: null, ProductIdentifier: "sku_monthly", WillRenew: false)],
            LatestExpirationDate: null);

        RevenueCatCustomerInfoReader.IsEntitled(customerInfo, "pro").ShouldBeFalse();
    }

    [Fact]
    public void IsEntitled_NoMatchingEntitlement_ReturnsFalse()
    {
        var customerInfo = new RevenueCatCustomerInfo(
            ActiveSubscriptions: [],
            AllPurchasedIdentifiers: [],
            Entitlements: [],
            LatestExpirationDate: null);

        RevenueCatCustomerInfoReader.IsEntitled(customerInfo, "pro").ShouldBeFalse();
    }
}
