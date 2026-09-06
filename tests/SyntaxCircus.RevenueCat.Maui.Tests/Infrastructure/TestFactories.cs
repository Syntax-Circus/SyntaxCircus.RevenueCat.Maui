namespace SyntaxCircus.RevenueCat.Maui.Tests.Infrastructure;

/// <summary>Builds minimally-valid vendor SDK DTOs (all required members set) for test fixtures.</summary>
internal static class TestFactories
{
    public static CustomerInfoDto CreateCustomerInfo(
        IReadOnlyList<string>? activeSubscriptions = null,
        IReadOnlyList<EntitlementInfoDto>? entitlements = null) => new()
    {
        ActiveSubscriptions = activeSubscriptions?.ToList() ?? [],
        AllPurchasedIdentifiers = [],
        NonConsumablePurchases = [],
        FirstSeen = null,
        LatestExpirationDate = null,
        ManagementUrl = string.Empty,
        Entitlements = entitlements?.ToList() ?? [],
    };

    public static StoreTransactionDto CreateStoreTransaction(string transactionIdentifier) => new()
    {
        ProductIdentifier = string.Empty,
        PurchaseDate = DateTime.UtcNow,
        TransactionIdentifier = transactionIdentifier,
        Quantity = 1,
    };

    public static EntitlementInfoDto CreateEntitlement(string identifier, bool isActive, string productIdentifier = "sku_monthly") => new()
    {
        BillingIssueDetectedAt = null,
        ExpirationDate = null,
        Identifier = identifier,
        IsActive = isActive,
        IsSandbox = false,
        LatestPurchaseDate = null,
        OriginalPurchaseDate = null,
        OwnershipType = OwnershipType.Purchased,
        PeriodType = PeriodType.Normal,
        ProductIdentifier = productIdentifier,
        ProductPlanIdentifier = string.Empty,
        Store = StoreType.AppStore,
        UnsubscribeDetectedAt = null,
        WillRenew = isActive,
    };
}
