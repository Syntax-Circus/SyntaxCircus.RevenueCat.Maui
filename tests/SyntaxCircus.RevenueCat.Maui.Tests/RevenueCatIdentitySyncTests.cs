namespace SyntaxCircus.RevenueCat.Maui.Tests;

public class RevenueCatIdentitySyncTests
{
    [Fact]
    public async Task SyncLoginAsync_NullBilling_ThrowsArgumentNullException()
        => await Should.ThrowAsync<ArgumentNullException>(() =>
            RevenueCatIdentitySync.SyncLoginAsync(null!, "user1", NullLogger.Instance, TestContext.Current.CancellationToken));

    [Fact]
    public async Task SyncLoginAsync_NullLogger_ThrowsArgumentNullException()
        => await Should.ThrowAsync<ArgumentNullException>(() =>
            RevenueCatIdentitySync.SyncLoginAsync(Substitute.For<IRevenueCatBilling>(), "user1", null!, TestContext.Current.CancellationToken));

    [Fact]
    public async Task SyncLoginAsync_NullUserId_DoesNotCallLogin()
    {
        var billing = Substitute.For<IRevenueCatBilling>();

        await RevenueCatIdentitySync.SyncLoginAsync(billing, null, NullLogger.Instance, TestContext.Current.CancellationToken);

        await billing.DidNotReceiveWithAnyArgs().Login(default!, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task SyncLoginAsync_WhitespaceUserId_DoesNotCallLogin()
    {
        var billing = Substitute.For<IRevenueCatBilling>();

        await RevenueCatIdentitySync.SyncLoginAsync(billing, "   ", NullLogger.Instance, TestContext.Current.CancellationToken);

        await billing.DidNotReceiveWithAnyArgs().Login(default!, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task SyncLoginAsync_ValidUserId_CallsLogin()
    {
        var billing = Substitute.For<IRevenueCatBilling>();
        billing.Login("user1", Arg.Any<CancellationToken>()).Returns(TestFactories.CreateCustomerInfo());

        await RevenueCatIdentitySync.SyncLoginAsync(billing, "user1", NullLogger.Instance, TestContext.Current.CancellationToken);

        await billing.Received(1).Login("user1", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SyncLoginAsync_LoginThrows_SwallowsExceptionInsteadOfPropagating()
    {
        var billing = Substitute.For<IRevenueCatBilling>();
        billing.Login("user1", Arg.Any<CancellationToken>()).ThrowsAsync(new InvalidOperationException("boom"));

        await Should.NotThrowAsync(() =>
            RevenueCatIdentitySync.SyncLoginAsync(billing, "user1", NullLogger.Instance, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task SyncLoginAsync_LoginThrowsOperationCanceled_Propagates()
    {
        var billing = Substitute.For<IRevenueCatBilling>();
        billing.Login("user1", Arg.Any<CancellationToken>()).ThrowsAsync(new OperationCanceledException());

        await Should.ThrowAsync<OperationCanceledException>(() =>
            RevenueCatIdentitySync.SyncLoginAsync(billing, "user1", NullLogger.Instance, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task SyncLogoutAsync_NullBilling_ThrowsArgumentNullException()
        => await Should.ThrowAsync<ArgumentNullException>(() =>
            RevenueCatIdentitySync.SyncLogoutAsync(null!, NullLogger.Instance, TestContext.Current.CancellationToken));

    [Fact]
    public async Task SyncLogoutAsync_NullLogger_ThrowsArgumentNullException()
        => await Should.ThrowAsync<ArgumentNullException>(() =>
            RevenueCatIdentitySync.SyncLogoutAsync(Substitute.For<IRevenueCatBilling>(), null!, TestContext.Current.CancellationToken));

    [Fact]
    public async Task SyncLogoutAsync_Success_CallsLogout()
    {
        var billing = Substitute.For<IRevenueCatBilling>();
        billing.Logout(Arg.Any<CancellationToken>()).Returns(TestFactories.CreateCustomerInfo());

        await RevenueCatIdentitySync.SyncLogoutAsync(billing, NullLogger.Instance, TestContext.Current.CancellationToken);

        await billing.Received(1).Logout(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SyncLogoutAsync_LogoutThrows_SwallowsExceptionInsteadOfPropagating()
    {
        var billing = Substitute.For<IRevenueCatBilling>();
        billing.Logout(Arg.Any<CancellationToken>()).ThrowsAsync(new InvalidOperationException("boom"));

        await Should.NotThrowAsync(() =>
            RevenueCatIdentitySync.SyncLogoutAsync(billing, NullLogger.Instance, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task SyncLogoutAsync_LogoutThrowsOperationCanceled_Propagates()
    {
        var billing = Substitute.For<IRevenueCatBilling>();
        billing.Logout(Arg.Any<CancellationToken>()).ThrowsAsync(new OperationCanceledException());

        await Should.ThrowAsync<OperationCanceledException>(() =>
            RevenueCatIdentitySync.SyncLogoutAsync(billing, NullLogger.Instance, TestContext.Current.CancellationToken));
    }
}
