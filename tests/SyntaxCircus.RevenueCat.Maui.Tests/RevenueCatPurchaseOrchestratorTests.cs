namespace SyntaxCircus.RevenueCat.Maui.Tests;

public class RevenueCatPurchaseOrchestratorTests
{
    private static PackageDto CreatePackage(string identifier, string sku)
        => new() { Identifier = identifier, OfferingIdentifier = "default", Product = new ProductDto { Sku = sku } };

    private static IRevenueCatBilling CreateBillingWithOffering(params PackageDto[] packages)
    {
        var billing = Substitute.For<IRevenueCatBilling>();
        var offering = new OfferingDto { Identifier = "default", IsCurrent = true, AvailablePackages = [.. packages] };
        billing.GetOfferings(Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns([offering]);
        return billing;
    }

    [Fact]
    public async Task PurchaseAsync_NullBilling_ThrowsArgumentNullException()
        => await Should.ThrowAsync<ArgumentNullException>(() =>
            RevenueCatPurchaseOrchestrator.PurchaseAsync(null!, "sku_1", NullLogger.Instance, TestContext.Current.CancellationToken));

    [Fact]
    public async Task PurchaseAsync_NullProductIdentifier_ThrowsArgumentNullException()
        => await Should.ThrowAsync<ArgumentNullException>(() =>
            RevenueCatPurchaseOrchestrator.PurchaseAsync(Substitute.For<IRevenueCatBilling>(), null!, NullLogger.Instance, TestContext.Current.CancellationToken));

    [Fact]
    public async Task PurchaseAsync_NullLogger_ThrowsArgumentNullException()
        => await Should.ThrowAsync<ArgumentNullException>(() =>
            RevenueCatPurchaseOrchestrator.PurchaseAsync(Substitute.For<IRevenueCatBilling>(), "sku_1", null!, TestContext.Current.CancellationToken));

    [Fact]
    public async Task PurchaseAsync_ProductNotFound_ReturnsFailureWithoutPurchasing()
    {
        var billing = CreateBillingWithOffering(CreatePackage("pkg_monthly", "sku_monthly"));

        var result = await RevenueCatPurchaseOrchestrator.PurchaseAsync(billing, "sku_unknown", NullLogger.Instance, TestContext.Current.CancellationToken);

        result.Success.ShouldBeFalse();
        result.ErrorMessage.ShouldNotBeNull();
        result.ErrorMessage.ShouldContain("sku_unknown");
        await billing.DidNotReceiveWithAnyArgs().PurchaseProduct(default!, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task PurchaseAsync_MatchesBySku_PurchasesFoundPackage()
    {
        var package = CreatePackage("pkg_monthly", "sku_monthly");
        var billing = CreateBillingWithOffering(package);
        billing.PurchaseProduct(package, Arg.Any<CancellationToken>()).Returns(new PurchaseResultDto { IsSuccess = true });

        var result = await RevenueCatPurchaseOrchestrator.PurchaseAsync(billing, "sku_monthly", NullLogger.Instance, TestContext.Current.CancellationToken);

        result.Success.ShouldBeTrue();
    }

    [Fact]
    public async Task PurchaseAsync_MatchesByPackageIdentifier_PurchasesFoundPackage()
    {
        var package = CreatePackage("pkg_monthly", "sku_monthly");
        var billing = CreateBillingWithOffering(package);
        billing.PurchaseProduct(package, Arg.Any<CancellationToken>()).Returns(new PurchaseResultDto { IsSuccess = true });

        var result = await RevenueCatPurchaseOrchestrator.PurchaseAsync(billing, "pkg_monthly", NullLogger.Instance, TestContext.Current.CancellationToken);

        result.Success.ShouldBeTrue();
    }

    [Fact]
    public async Task PurchaseAsync_MatchIsCaseInsensitive()
    {
        var package = CreatePackage("pkg_monthly", "sku_monthly");
        var billing = CreateBillingWithOffering(package);
        billing.PurchaseProduct(package, Arg.Any<CancellationToken>()).Returns(new PurchaseResultDto { IsSuccess = true });

        var result = await RevenueCatPurchaseOrchestrator.PurchaseAsync(billing, "SKU_MONTHLY", NullLogger.Instance, TestContext.Current.CancellationToken);

        result.Success.ShouldBeTrue();
    }

    [Fact]
    public async Task PurchaseAsync_MatchesNormalizedIdentifiers()
    {
        var package = CreatePackage("pkg-monthly", "sku-monthly");
        var billing = CreateBillingWithOffering(package);
        billing.PurchaseProduct(package, Arg.Any<CancellationToken>()).Returns(new PurchaseResultDto { IsSuccess = true });

        var result = await RevenueCatPurchaseOrchestrator.PurchaseAsync(billing, " sku_monthly ", NullLogger.Instance, TestContext.Current.CancellationToken);

        result.Success.ShouldBeTrue();
    }

    [Fact]
    public async Task PurchaseAsync_CustomResolverCanSelectPackage()
    {
        var package = CreatePackage("pkg_monthly", "sku_monthly");
        var billing = CreateBillingWithOffering(package);
        billing.PurchaseProduct(package, Arg.Any<CancellationToken>()).Returns(new PurchaseResultDto { IsSuccess = true });

        var result = await RevenueCatPurchaseOrchestrator.PurchaseAsync(
            billing,
            "button-package",
            NullLogger.Instance,
            (_, _) => package,
            TestContext.Current.CancellationToken);

        result.Success.ShouldBeTrue();
    }

    [Fact]
    public async Task PurchaseAsync_SuccessfulPurchase_ReturnsTransactionAndAppUserId()
    {
        var package = CreatePackage("pkg_monthly", "sku_monthly");
        var billing = CreateBillingWithOffering(package);
        billing.PurchaseProduct(package, Arg.Any<CancellationToken>()).Returns(new PurchaseResultDto
        {
            IsSuccess = true,
            Transaction = TestFactories.CreateStoreTransaction("txn_1"),
        });
        billing.GetAppUserId().Returns("user_1");

        var result = await RevenueCatPurchaseOrchestrator.PurchaseAsync(billing, "sku_monthly", NullLogger.Instance, TestContext.Current.CancellationToken);

        result.Success.ShouldBeTrue();
        result.TransactionId.ShouldBe("txn_1");
        result.AppUserId.ShouldBe("user_1");
    }

    [Fact]
    public async Task PurchaseAsync_UserCancelled_ReturnsWasCancelledTrue()
    {
        var package = CreatePackage("pkg_monthly", "sku_monthly");
        var billing = CreateBillingWithOffering(package);
        billing.PurchaseProduct(package, Arg.Any<CancellationToken>()).Returns(new PurchaseResultDto
        {
            IsSuccess = false,
            ErrorStatus = PurchaseErrorStatus.PurchaseCancelledError,
        });

        var result = await RevenueCatPurchaseOrchestrator.PurchaseAsync(billing, "sku_monthly", NullLogger.Instance, TestContext.Current.CancellationToken);

        result.Success.ShouldBeFalse();
        result.WasCancelled.ShouldBeTrue();
    }

    [Fact]
    public async Task PurchaseAsync_OtherStoreError_ReturnsFailureWithErrorStatusMessage()
    {
        var package = CreatePackage("pkg_monthly", "sku_monthly");
        var billing = CreateBillingWithOffering(package);
        billing.PurchaseProduct(package, Arg.Any<CancellationToken>()).Returns(new PurchaseResultDto
        {
            IsSuccess = false,
            ErrorStatus = PurchaseErrorStatus.NetworkError,
        });

        var result = await RevenueCatPurchaseOrchestrator.PurchaseAsync(billing, "sku_monthly", NullLogger.Instance, TestContext.Current.CancellationToken);

        result.Success.ShouldBeFalse();
        result.WasCancelled.ShouldBeFalse();
        result.ErrorMessage.ShouldNotBeNull();
        result.ErrorMessage.ShouldContain("NetworkError");
    }

    [Fact]
    public async Task RestoreAsync_NullBilling_ThrowsArgumentNullException()
        => await Should.ThrowAsync<ArgumentNullException>(() =>
            RevenueCatPurchaseOrchestrator.RestoreAsync(null!, "user1", NullLogger.Instance, TestContext.Current.CancellationToken));

    [Fact]
    public async Task RestoreAsync_NullLogger_ThrowsArgumentNullException()
        => await Should.ThrowAsync<ArgumentNullException>(() =>
            RevenueCatPurchaseOrchestrator.RestoreAsync(Substitute.For<IRevenueCatBilling>(), "user1", null!, TestContext.Current.CancellationToken));

    [Fact]
    public async Task RestoreAsync_UserIdProvided_LogsInBeforeRestoring()
    {
        var billing = Substitute.For<IRevenueCatBilling>();
        billing.Login("user1", Arg.Any<CancellationToken>()).Returns(TestFactories.CreateCustomerInfo());
        billing.RestoreTransactions(Arg.Any<CancellationToken>()).Returns(TestFactories.CreateCustomerInfo());
        billing.GetAppUserId().Returns("user1");

        var result = await RevenueCatPurchaseOrchestrator.RestoreAsync(billing, "user1", NullLogger.Instance, TestContext.Current.CancellationToken);

        result.Success.ShouldBeTrue();
        result.AppUserId.ShouldBe("user1");
        await billing.Received(1).Login("user1", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RestoreAsync_NoUserId_SkipsLogin()
    {
        var billing = Substitute.For<IRevenueCatBilling>();
        billing.RestoreTransactions(Arg.Any<CancellationToken>()).Returns(TestFactories.CreateCustomerInfo());

        await RevenueCatPurchaseOrchestrator.RestoreAsync(billing, null, NullLogger.Instance, TestContext.Current.CancellationToken);

        await billing.DidNotReceiveWithAnyArgs().Login(default!, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task RestoreAsync_RestoreThrows_ReturnsFailureInsteadOfThrowing()
    {
        var billing = Substitute.For<IRevenueCatBilling>();
        billing.RestoreTransactions(Arg.Any<CancellationToken>()).ThrowsAsync(new InvalidOperationException("boom"));

        var result = await RevenueCatPurchaseOrchestrator.RestoreAsync(billing, null, NullLogger.Instance, TestContext.Current.CancellationToken);

        result.Success.ShouldBeFalse();
        result.ErrorMessage.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task RestoreAsync_OperationCanceled_Propagates()
    {
        var billing = Substitute.For<IRevenueCatBilling>();
        billing.RestoreTransactions(Arg.Any<CancellationToken>()).ThrowsAsync(new OperationCanceledException());

        await Should.ThrowAsync<OperationCanceledException>(() =>
            RevenueCatPurchaseOrchestrator.RestoreAsync(billing, null, NullLogger.Instance, TestContext.Current.CancellationToken));
    }
}
