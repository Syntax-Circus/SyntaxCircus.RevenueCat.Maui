namespace SyntaxCircus.RevenueCat.Maui.Tests;

public class RevenueCatProductTests
{
    [Fact]
    public void Constructor_SetsAllProperties()
    {
        var product = new RevenueCatProduct("pkg_1", "sku_1", "$4.99", 4.99m, "USD", "default");

        product.PackageIdentifier.ShouldBe("pkg_1");
        product.Sku.ShouldBe("sku_1");
        product.PriceLocalized.ShouldBe("$4.99");
        product.Price.ShouldBe(4.99m);
        product.Currency.ShouldBe("USD");
        product.OfferingIdentifier.ShouldBe("default");
    }
}
