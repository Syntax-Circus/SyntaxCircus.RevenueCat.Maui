namespace SyntaxCircus.RevenueCat.Maui;

/// <summary>
/// Pure selection logic for picking the RevenueCat-reported store transaction id that corresponds
/// to a just-completed purchase, out of a customer's non-subscription transaction history. Kept
/// platform-neutral (no vendor SDK types) so it compiles and is unit-testable on every TFM; the
/// Android-only code that fetches the candidates lives elsewhere.
/// </summary>
internal static class StoreTransactionIdSelector
{
    /// <summary>Default tolerance between a transaction's purchase date and the target purchase date.</summary>
    internal static readonly TimeSpan DefaultTolerance = TimeSpan.FromMinutes(10);

    /// <summary>
    /// One vendor SDK non-subscription transaction, reduced to the fields the selector needs.
    /// <paramref name="ProductId"/> and <paramref name="ProductIdentifier"/> mirror the two
    /// distinct product-identifying fields RevenueCat's native transaction model exposes.
    /// </summary>
    internal readonly record struct Candidate(
        string? ProductId,
        string? ProductIdentifier,
        string? StoreTransactionId,
        DateTime? PurchaseDate);

    /// <summary>
    /// Returns the <see cref="Candidate.StoreTransactionId"/> of the candidate matching one of
    /// <paramref name="productIds"/> whose <see cref="Candidate.PurchaseDate"/> is closest to
    /// <paramref name="purchasedAt"/>, provided that distance is within <paramref name="tolerance"/>
    /// (<see cref="DefaultTolerance"/> when not given). Candidates with no store transaction id, no
    /// purchase date, or a non-matching product are ignored. Returns null when nothing qualifies.
    /// </summary>
    internal static string? Select(
        IEnumerable<Candidate> candidates,
        IReadOnlyCollection<string> productIds,
        DateTime purchasedAt,
        TimeSpan? tolerance = null)
    {
        ArgumentNullException.ThrowIfNull(candidates);
        ArgumentNullException.ThrowIfNull(productIds);

        var effectiveTolerance = tolerance ?? DefaultTolerance;

        string? best = null;
        var bestDelta = TimeSpan.MaxValue;

        foreach (var candidate in candidates)
        {
            if (string.IsNullOrWhiteSpace(candidate.StoreTransactionId))
            {
                continue;
            }

            if (!MatchesAnyProduct(candidate, productIds))
            {
                continue;
            }

            if (candidate.PurchaseDate is not { } purchaseDate)
            {
                continue;
            }

            var delta = (purchaseDate - purchasedAt).Duration();
            if (delta > effectiveTolerance)
            {
                continue;
            }

            if (delta < bestDelta)
            {
                bestDelta = delta;
                best = candidate.StoreTransactionId;
            }
        }

        return best;
    }

    private static bool MatchesAnyProduct(Candidate candidate, IReadOnlyCollection<string> productIds)
    {
        foreach (var productId in productIds)
        {
            if (string.IsNullOrWhiteSpace(productId))
            {
                continue;
            }

            if (string.Equals(candidate.ProductId, productId, StringComparison.OrdinalIgnoreCase)
                || string.Equals(candidate.ProductIdentifier, productId, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
