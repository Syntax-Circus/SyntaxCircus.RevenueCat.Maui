namespace SyntaxCircus.RevenueCat.Maui;

/// <summary>
/// A snapshot of the current app user's subscription state, flattened from the vendor SDK's
/// customer-info read.
/// </summary>
public sealed record RevenueCatCustomerInfo(
    IReadOnlyList<string> ActiveSubscriptions,
    IReadOnlyList<string> AllPurchasedIdentifiers,
    IReadOnlyList<RevenueCatEntitlement> Entitlements,
    DateTime? LatestExpirationDate);
