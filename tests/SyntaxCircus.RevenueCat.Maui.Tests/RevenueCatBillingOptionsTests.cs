namespace SyntaxCircus.RevenueCat.Maui.Tests;

public class RevenueCatBillingOptionsTests
{
    [Fact]
    public void Defaults_AreEmptyStrings()
    {
        var options = new RevenueCatBillingOptions();

        options.AndroidApiKey.ShouldBe(string.Empty);
        options.IosApiKey.ShouldBe(string.Empty);
    }

    [Fact]
    public void SectionName_IsRevenueCat()
        => RevenueCatBillingOptions.SectionName.ShouldBe("RevenueCat");
}
