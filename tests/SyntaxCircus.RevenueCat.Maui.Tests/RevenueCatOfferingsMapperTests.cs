namespace SyntaxCircus.RevenueCat.Maui.Tests;

public class RevenueCatOfferingsMapperTests
{
    private static PackageDto CreatePackage(string identifier, string sku, string offeringIdentifier = "default")
        => new()
        {
            Identifier = identifier,
            OfferingIdentifier = offeringIdentifier,
            Product = new ProductDto
            {
                Sku = sku,
                Pricing = new PricingDto
                {
                    Price = 4.99m,
                    CurrencyCode = "USD",
                    PriceLocalized = "$4.99",
                },
            },
        };

    [Fact]
    public async Task GetCurrentProductsAsync_NullBilling_ThrowsArgumentNullException()
        => await Should.ThrowAsync<ArgumentNullException>(() =>
            RevenueCatOfferingsMapper.GetCurrentProductsAsync(null!, ct: TestContext.Current.CancellationToken));

    [Fact]
    public async Task GetCurrentProductsAsync_NotInitialized_ReturnsEmptyWithoutFetchingOfferings()
    {
        var billing = Substitute.For<IRevenueCatBilling>();
        billing.IsInitialized().Returns(false);

        var result = await RevenueCatOfferingsMapper.GetCurrentProductsAsync(billing, ct: TestContext.Current.CancellationToken);

        result.ShouldBeEmpty();
        await billing.DidNotReceiveWithAnyArgs().GetOfferings(default, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task GetCurrentProductsAsync_NoCurrentOffering_ReturnsEmpty()
    {
        var billing = Substitute.For<IRevenueCatBilling>();
        billing.IsInitialized().Returns(true);
        billing.GetOfferings(Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns([new OfferingDto { Identifier = "default", IsCurrent = false }]);

        var result = await RevenueCatOfferingsMapper.GetCurrentProductsAsync(billing, ct: TestContext.Current.CancellationToken);

        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetCurrentProductsAsync_CurrentOfferingWithPackages_MapsToProducts()
    {
        var billing = Substitute.For<IRevenueCatBilling>();
        billing.IsInitialized().Returns(true);
        var offering = new OfferingDto
        {
            Identifier = "default",
            IsCurrent = true,
            AvailablePackages = [CreatePackage("pkg_monthly", "sku_monthly")],
        };
        billing.GetOfferings(Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns([offering]);

        var result = await RevenueCatOfferingsMapper.GetCurrentProductsAsync(billing, ct: TestContext.Current.CancellationToken);

        result.Count.ShouldBe(1);
        result[0].PackageIdentifier.ShouldBe("pkg_monthly");
        result[0].Sku.ShouldBe("sku_monthly");
        result[0].Price.ShouldBe(4.99m);
        result[0].Currency.ShouldBe("USD");
        result[0].PriceLocalized.ShouldBe("$4.99");
        result[0].OfferingIdentifier.ShouldBe("default");
    }

    [Fact]
    public async Task GetCurrentProductsAsync_ForceRefreshTrue_PassesThroughToBilling()
    {
        var billing = Substitute.For<IRevenueCatBilling>();
        billing.IsInitialized().Returns(true);
        billing.GetOfferings(Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns([]);

        await RevenueCatOfferingsMapper.GetCurrentProductsAsync(billing, forceRefresh: true, ct: TestContext.Current.CancellationToken);

        await billing.Received(1).GetOfferings(true, Arg.Any<CancellationToken>());
    }
}
