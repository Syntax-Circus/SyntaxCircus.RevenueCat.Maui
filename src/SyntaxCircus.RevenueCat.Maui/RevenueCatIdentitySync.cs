namespace SyntaxCircus.RevenueCat.Maui;

/// <summary>
/// Keeps RevenueCat's <c>app_user_id</c> aligned with your own auth state: call
/// <see cref="SyncLoginAsync"/> after sign-in to point it at your user id (so purchases and the
/// TRANSFER webhook event correctly re-associate across reinstalls and new devices), and
/// <see cref="SyncLogoutAsync"/> at sign-out to detach it and return RevenueCat to an anonymous
/// identity.
/// </summary>
public static partial class RevenueCatIdentitySync
{
    /// <summary>
    /// Calls <c>billing.Login(userId)</c> when <paramref name="userId"/> is non-empty. Failures
    /// are logged and swallowed — this is a best-effort sync, not something worth failing app
    /// startup or a purchase attempt over.
    /// </summary>
    public static async Task SyncLoginAsync(
        IRevenueCatBilling billing,
        string? userId,
        ILogger logger,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(billing);
        ArgumentNullException.ThrowIfNull(logger);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return;
        }

        try
        {
            await billing.Login(userId, ct).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            LogSyncFailed(logger, ex);
        }
    }

    /// <summary>
    /// Calls <c>billing.Logout()</c> to detach the current <c>app_user_id</c> and return
    /// RevenueCat to an anonymous identity. Failures are logged and swallowed — this is a
    /// best-effort sync, not something worth failing sign-out over.
    /// </summary>
    public static async Task SyncLogoutAsync(
        IRevenueCatBilling billing,
        ILogger logger,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(billing);
        ArgumentNullException.ThrowIfNull(logger);

        try
        {
            await billing.Logout(ct).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            LogSyncFailed(logger, ex);
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Failed to sync RevenueCat identity with the current user.")]
    private static partial void LogSyncFailed(ILogger logger, Exception exception);
}
