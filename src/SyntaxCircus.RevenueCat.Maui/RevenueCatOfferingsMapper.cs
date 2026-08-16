namespace SyntaxCircus.RevenueCat.Maui;

/// <summary>Maps the SDK's current offering into plain <see cref="RevenueCatProduct"/> DTOs.</summary>
public static class RevenueCatOfferingsMapper
{
    /// <summary>
    /// Returns the packages in RevenueCat's current offering, or an empty list if the SDK isn't
    /// initialized yet or no current offering is configured.
    /// </summary>
    public static async Task<IReadOnlyList<RevenueCatProduct>> GetCurrentProductsAsync(
        IRevenueCatBilling billing,
        bool forceRefresh = false,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(billing);

        if (!billing.IsInitialized())
        {
            return [];
        }

        var offerings = await billing.GetOfferings(forceRefresh: forceRefresh, cancellationToken: ct).ConfigureAwait(false);
        var current = offerings.GetCurrent();
        if (current is null)
        {
            return [];
        }

        return current.AvailablePackages
            .Select(pkg => new RevenueCatProduct(
                PackageIdentifier: pkg.Identifier,
                Sku: pkg.Product.Sku,
                PriceLocalized: pkg.Product.Pricing.PriceLocalized,
                Price: pkg.Product.Pricing.Price,
                Currency: pkg.Product.Pricing.CurrencyCode,
                OfferingIdentifier: pkg.OfferingIdentifier))
            .ToList();
    }
}
