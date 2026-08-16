namespace SyntaxCircus.RevenueCat.Maui;

/// <summary>A RevenueCat offering package, flattened to the fields a purchase UI needs.</summary>
public sealed record RevenueCatProduct(
    string PackageIdentifier,
    string Sku,
    string PriceLocalized,
    decimal Price,
    string Currency,
    string OfferingIdentifier);
