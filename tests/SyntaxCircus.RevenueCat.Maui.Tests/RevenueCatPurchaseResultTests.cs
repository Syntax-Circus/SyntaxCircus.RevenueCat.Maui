namespace SyntaxCircus.RevenueCat.Maui.Tests;

public class RevenueCatPurchaseResultTests
{
    [Fact]
    public void Constructor_DefaultsAreFalseAndNull()
    {
        var result = new RevenueCatPurchaseResult(Success: true);

        result.Success.ShouldBeTrue();
        result.TransactionId.ShouldBeNull();
        result.AppUserId.ShouldBeNull();
        result.WasCancelled.ShouldBeFalse();
        result.ErrorMessage.ShouldBeNull();
        result.ErrorStatus.ShouldBeNull();
        result.Outcome.ShouldBe(RevenueCatPurchaseOutcome.Succeeded);
        result.StoreErrorCode.ShouldBeNull();
    }

    [Fact]
    public void Constructor_SetsAllProperties()
    {
        var result = new RevenueCatPurchaseResult(
            Success: false,
            TransactionId: "txn_1",
            AppUserId: "user_1",
            WasCancelled: true,
            ErrorMessage: "cancelled",
            ErrorStatus: PurchaseErrorStatus.PurchaseCancelledError);

        result.Success.ShouldBeFalse();
        result.TransactionId.ShouldBe("txn_1");
        result.AppUserId.ShouldBe("user_1");
        result.WasCancelled.ShouldBeTrue();
        result.ErrorMessage.ShouldBe("cancelled");
        result.ErrorStatus.ShouldBe(PurchaseErrorStatus.PurchaseCancelledError);
        result.Outcome.ShouldBe(RevenueCatPurchaseOutcome.Cancelled);
        result.StoreErrorCode.ShouldBe(nameof(PurchaseErrorStatus.PurchaseCancelledError));
    }

    [Theory]
    [InlineData(PurchaseErrorStatus.PaymentPendingError, RevenueCatPurchaseOutcome.Pending)]
    [InlineData(PurchaseErrorStatus.ProductAlreadyPurchasedError, RevenueCatPurchaseOutcome.AlreadyOwned)]
    [InlineData(PurchaseErrorStatus.NetworkError, RevenueCatPurchaseOutcome.Failed)]
    public void Outcome_NormalizesTypedStoreError(PurchaseErrorStatus status, RevenueCatPurchaseOutcome expected)
    {
        var result = new RevenueCatPurchaseResult(Success: false, ErrorStatus: status);

        result.Outcome.ShouldBe(expected);
        result.StoreErrorCode.ShouldBe(status.ToString());
    }
}
