using System.Diagnostics.CodeAnalysis;

namespace SyntaxCircus.RevenueCat.Maui;

public static class RevenueCatMauiServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="RevenueCatBillingOptions"/> (bound from the "RevenueCat" section) and
    /// the vendor SDK's own <c>IRevenueCatBilling</c> via <c>AddRevenueCatBilling()</c>.
    /// Does not call <c>Initialize</c> — use <see cref="RevenueCatInitializer"/> for that once
    /// the app knows which platform it's running on.
    /// </summary>
    [UnconditionalSuppressMessage("Trimming", "IL2026",
        Justification = "RevenueCatBillingOptions is a plain POCO of string properties — nothing for a trimmer to remove.")]
    public static IServiceCollection AddRevenueCatMaui(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<RevenueCatBillingOptions>(configuration.GetSection(RevenueCatBillingOptions.SectionName));
        services.AddRevenueCatBilling();

        return services;
    }
}
