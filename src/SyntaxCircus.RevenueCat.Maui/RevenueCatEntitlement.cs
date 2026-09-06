namespace SyntaxCircus.RevenueCat.Maui;

/// <summary>A RevenueCat entitlement, flattened to the fields an "is this user entitled?" check needs.</summary>
public sealed record RevenueCatEntitlement(
    string Identifier,
    bool IsActive,
    DateTime? ExpirationDate,
    string ProductIdentifier,
    bool WillRenew);
