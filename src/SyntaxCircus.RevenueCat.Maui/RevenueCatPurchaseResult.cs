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
    string? ErrorMessage = null);
