namespace SyntaxCircus.RevenueCat.Maui.Tests.Infrastructure;

/// <summary>Builds minimally-valid vendor SDK DTOs (all required members set) for test fixtures.</summary>
internal static class TestFactories
{
    public static CustomerInfoDto CreateCustomerInfo() => new()
    {
        ActiveSubscriptions = [],
        AllPurchasedIdentifiers = [],
        NonConsumablePurchases = [],
        FirstSeen = null,
        LatestExpirationDate = null,
        ManagementURL = string.Empty,
        Entitlements = [],
    };

    public static StoreTransactionDto CreateStoreTransaction(string transactionIdentifier) => new()
    {
        ProductIdentifier = string.Empty,
        PurchaseDate = DateTime.UtcNow,
        TransactionIdentifier = transactionIdentifier,
        Quantity = 1,
    };
}
