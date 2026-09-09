namespace SyntaxCircus.RevenueCat.Maui;

/// <summary>
/// The outcome of a store purchase or restore attempt. Recording the transaction against your
/// own backend is the caller's responsibility — this only reports what happened at the store.
/// </summary>
public sealed record RevenueCatPurchaseResult(
    bool Success,
    string? TransactionId = null,
    string? AppUserId = null,
    bool WasCancelled = false,
    string? ErrorMessage = null,
    PurchaseErrorStatus? ErrorStatus = null)
{
    public RevenueCatPurchaseOutcome Outcome => Success
        ? RevenueCatPurchaseOutcome.Succeeded
        : WasCancelled
            ? RevenueCatPurchaseOutcome.Cancelled
            : ErrorStatus switch
            {
                PurchaseErrorStatus.PaymentPendingError => RevenueCatPurchaseOutcome.Pending,
                PurchaseErrorStatus.ProductAlreadyPurchasedError => RevenueCatPurchaseOutcome.AlreadyOwned,
                _ => RevenueCatPurchaseOutcome.Failed,
            };

    public string? StoreErrorCode => ErrorStatus?.ToString();
}

public enum RevenueCatPurchaseOutcome
{
    Succeeded,
    Cancelled,
    Pending,
    AlreadyOwned,
    Failed,
}
