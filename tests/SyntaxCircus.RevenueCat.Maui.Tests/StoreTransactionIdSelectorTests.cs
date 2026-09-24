namespace SyntaxCircus.RevenueCat.Maui.Tests;

public class StoreTransactionIdSelectorTests
{
    private static readonly DateTime PurchasedAt = new(2026, 9, 1, 12, 0, 0, DateTimeKind.Utc);

    private static StoreTransactionIdSelector.Candidate Candidate(
        string? productId = "sku_monthly",
        string? productIdentifier = "sku_monthly",
        string? storeTransactionId = "GPA.1234-5678",
        DateTime? purchaseDate = null)
        => new(productId, productIdentifier, storeTransactionId, purchaseDate ?? PurchasedAt);

    [Fact]
    public void Select_ExactProductAndClosestDate_ReturnsClosestCandidate()
    {
        var farther = Candidate(storeTransactionId: "GPA.far", purchaseDate: PurchasedAt.AddMinutes(-5));
        var closer = Candidate(storeTransactionId: "GPA.close", purchaseDate: PurchasedAt.AddSeconds(-30));

        var result = StoreTransactionIdSelector.Select([farther, closer], ["sku_monthly"], PurchasedAt);

        result.ShouldBe("GPA.close");
    }

    [Fact]
    public void Select_IgnoresCandidatesForOtherProducts()
    {
        var otherProduct = Candidate(productId: "sku_other", productIdentifier: "sku_other", storeTransactionId: "GPA.other", purchaseDate: PurchasedAt);
        var matching = Candidate(storeTransactionId: "GPA.match", purchaseDate: PurchasedAt.AddMinutes(-2));

        var result = StoreTransactionIdSelector.Select([otherProduct, matching], ["sku_monthly"], PurchasedAt);

        result.ShouldBe("GPA.match");
    }

    [Fact]
    public void Select_MatchesOnProductIdentifierEvenWhenProductIdDiffers()
    {
        var candidate = new StoreTransactionIdSelector.Candidate(
            ProductId: "sku_monthly_base",
            ProductIdentifier: "sku_monthly",
            StoreTransactionId: "GPA.match",
            PurchaseDate: PurchasedAt);

        var result = StoreTransactionIdSelector.Select([candidate], ["sku_monthly"], PurchasedAt);

        result.ShouldBe("GPA.match");
    }

    [Fact]
    public void Select_OutsideTolerance_ReturnsNull()
    {
        var tooFar = Candidate(purchaseDate: PurchasedAt.AddMinutes(-11));

        var result = StoreTransactionIdSelector.Select([tooFar], ["sku_monthly"], PurchasedAt);

        result.ShouldBeNull();
    }

    [Fact]
    public void Select_WithinCustomTolerance_ReturnsCandidate()
    {
        var candidate = Candidate(purchaseDate: PurchasedAt.AddMinutes(-2));

        var result = StoreTransactionIdSelector.Select([candidate], ["sku_monthly"], PurchasedAt, TimeSpan.FromMinutes(1).Add(TimeSpan.FromMinutes(1.5)));

        result.ShouldBe("GPA.1234-5678");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Select_EmptyOrNullStoreTransactionId_IsSkipped(string? storeTransactionId)
    {
        var candidate = Candidate(storeTransactionId: storeTransactionId);

        var result = StoreTransactionIdSelector.Select([candidate], ["sku_monthly"], PurchasedAt);

        result.ShouldBeNull();
    }

    [Fact]
    public void Select_NoPurchaseDate_IsSkipped()
    {
        var candidate = new StoreTransactionIdSelector.Candidate("sku_monthly", "sku_monthly", "GPA.match", null);

        var result = StoreTransactionIdSelector.Select([candidate], ["sku_monthly"], PurchasedAt);

        result.ShouldBeNull();
    }

    [Fact]
    public void Select_NoCandidatesMatch_ReturnsNull()
        => StoreTransactionIdSelector.Select([], ["sku_monthly"], PurchasedAt).ShouldBeNull();

    [Fact]
    public void Select_NullCandidates_ThrowsArgumentNullException()
        => Should.Throw<ArgumentNullException>(() => StoreTransactionIdSelector.Select(null!, ["sku_monthly"], PurchasedAt));

    [Fact]
    public void Select_NullProductIds_ThrowsArgumentNullException()
        => Should.Throw<ArgumentNullException>(() => StoreTransactionIdSelector.Select([Candidate()], null!, PurchasedAt));
}
