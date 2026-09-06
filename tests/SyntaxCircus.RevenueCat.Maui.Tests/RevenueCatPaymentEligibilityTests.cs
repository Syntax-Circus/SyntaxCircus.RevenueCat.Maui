namespace SyntaxCircus.RevenueCat.Maui.Tests;

public class RevenueCatPaymentEligibilityTests
{
    [Fact]
    public async Task CanMakePaymentsAsync_NullBilling_ThrowsArgumentNullException()
        => await Should.ThrowAsync<ArgumentNullException>(() =>
            RevenueCatPaymentEligibility.CanMakePaymentsAsync(null!, TestContext.Current.CancellationToken));

    [Fact]
    public async Task CanMakePaymentsAsync_BillingReturnsTrue_ReturnsTrue()
    {
        var billing = Substitute.For<IRevenueCatBilling>();
        billing.CanMakePayments(Arg.Any<CancellationToken>()).Returns(new CanMakePaymentsResultDto { Value = true });

        var result = await RevenueCatPaymentEligibility.CanMakePaymentsAsync(billing, TestContext.Current.CancellationToken);

        result.ShouldBeTrue();
    }

    [Fact]
    public async Task CanMakePaymentsAsync_BillingReturnsFalse_ReturnsFalse()
    {
        var billing = Substitute.For<IRevenueCatBilling>();
        billing.CanMakePayments(Arg.Any<CancellationToken>()).Returns(new CanMakePaymentsResultDto { Value = false });

        var result = await RevenueCatPaymentEligibility.CanMakePaymentsAsync(billing, TestContext.Current.CancellationToken);

        result.ShouldBeFalse();
    }

    [Fact]
    public async Task CanMakePaymentsAsync_BillingReturnsError_Throws()
    {
        var billing = Substitute.For<IRevenueCatBilling>();
        billing.CanMakePayments(Arg.Any<CancellationToken>())
            .Returns(new CanMakePaymentsResultDto { ErrorException = new InvalidOperationException("boom") });

        await Should.ThrowAsync<InvalidOperationException>(() =>
            RevenueCatPaymentEligibility.CanMakePaymentsAsync(billing, TestContext.Current.CancellationToken));
    }
}
