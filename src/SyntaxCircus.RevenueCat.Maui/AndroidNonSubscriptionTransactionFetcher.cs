#if ANDROID
using Com.Revenuecat.Purchases;
using Com.Revenuecat.Purchases.Interfaces;

namespace SyntaxCircus.RevenueCat.Maui;

/// <summary>
/// Fetches the current customer's non-subscription transactions straight from the native
/// RevenueCat Android SDK (<see cref="Purchases.SharedInstance"/>), bypassing Kebechet's
/// <c>IRevenueCatBilling</c>/<c>CustomerInfoDto</c> wrapper — that DTO doesn't carry
/// <c>NonSubscriptionTransactions</c>, which is the only place the Google Play order id
/// (<c>StoreTransactionId</c>) is exposed.
/// </summary>
internal static class AndroidNonSubscriptionTransactionFetcher
{
    internal static Task<IReadOnlyList<StoreTransactionIdSelector.Candidate>> GetCandidatesAsync(CancellationToken ct)
    {
        var listener = new ReceiveCustomerInfoListener(ct);
        Purchases.SharedInstance.GetCustomerInfo(listener);
        return listener.Task;
    }

    private sealed class ReceiveCustomerInfoListener : Java.Lang.Object, IReceiveCustomerInfoCallback
    {
        private readonly TaskCompletionSource<IReadOnlyList<StoreTransactionIdSelector.Candidate>> _tcs = new();

        public ReceiveCustomerInfoListener(CancellationToken ct)
        {
            ct.Register(() => _tcs.TrySetCanceled(ct));
        }

        public Task<IReadOnlyList<StoreTransactionIdSelector.Candidate>> Task => _tcs.Task;

        public void OnReceived(CustomerInfo customerInfo)
        {
            try
            {
                // Transaction.ProductId is deprecated by the native SDK in favor of ProductIdentifier;
                // only the latter is used for matching.
                var candidates = customerInfo.NonSubscriptionTransactions
                    .Select(t => new StoreTransactionIdSelector.Candidate(
                        ProductId: null,
                        ProductIdentifier: t.ProductIdentifier,
                        StoreTransactionId: t.StoreTransactionId,
                        PurchaseDate: ToDateTime(t.PurchaseDate)))
                    .ToList();

                _tcs.TrySetResult(candidates);
            }
            catch (Exception ex)
            {
                _tcs.TrySetException(ex);
            }
            finally
            {
                customerInfo.Dispose();
            }
        }

        public void OnError(PurchasesError error)
        {
            _tcs.TrySetException(new InvalidOperationException($"RevenueCat GetCustomerInfo failed: {error?.Message}"));
        }

        private static DateTime? ToDateTime(Java.Util.Date? date)
            => date is null ? null : DateTimeOffset.FromUnixTimeMilliseconds(date.Time).UtcDateTime;
    }
}
#endif
