namespace SyntaxCircus.RevenueCat.Maui;

/// <summary>Checks whether the current device/user is allowed to make payments through the store.</summary>
public static class RevenueCatPaymentEligibility
{
    /// <summary>
    /// Returns <see langword="true"/> if the store reports this device/user can make payments
    /// (e.g. not blocked by parental controls or a region restriction). Useful for gating a
    /// purchase button before showing it.
    /// </summary>
    public static async Task<bool> CanMakePaymentsAsync(IRevenueCatBilling billing, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(billing);

        var result = await billing.CanMakePayments(ct).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            throw result.ErrorException
                ?? new InvalidOperationException($"Failed to determine RevenueCat payment eligibility: {result.Error}.");
        }

        return result.Value;
    }
}
