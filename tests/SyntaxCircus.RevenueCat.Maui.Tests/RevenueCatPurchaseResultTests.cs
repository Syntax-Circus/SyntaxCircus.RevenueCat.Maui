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
    }

    [Fact]
    public void Constructor_SetsAllProperties()
    {
        var result = new RevenueCatPurchaseResult(
            Success: false,
            TransactionId: "txn_1",
            AppUserId: "user_1",
            WasCancelled: true,
            ErrorMessage: "cancelled");

        result.Success.ShouldBeFalse();
        result.TransactionId.ShouldBe("txn_1");
        result.AppUserId.ShouldBe("user_1");
        result.WasCancelled.ShouldBeTrue();
        result.ErrorMessage.ShouldBe("cancelled");
    }
}
