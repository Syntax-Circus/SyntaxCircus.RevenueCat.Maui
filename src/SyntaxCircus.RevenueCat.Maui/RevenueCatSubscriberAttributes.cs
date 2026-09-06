namespace SyntaxCircus.RevenueCat.Maui;

/// <summary>Syncs user profile data to RevenueCat as subscriber attributes (e.g. for CRM/support tooling).</summary>
public static class RevenueCatSubscriberAttributes
{
    public static void SetEmail(IRevenueCatBilling billing, string email)
    {
        ArgumentNullException.ThrowIfNull(billing);
        ArgumentNullException.ThrowIfNull(email);

        billing.SetEmail(email);
    }

    public static void SetDisplayName(IRevenueCatBilling billing, string name)
    {
        ArgumentNullException.ThrowIfNull(billing);
        ArgumentNullException.ThrowIfNull(name);

        billing.SetDisplayName(name);
    }

    public static void SetPhoneNumber(IRevenueCatBilling billing, string phone)
    {
        ArgumentNullException.ThrowIfNull(billing);
        ArgumentNullException.ThrowIfNull(phone);

        billing.SetPhoneNumber(phone);
    }

    public static void SetAttributes(IRevenueCatBilling billing, IDictionary<string, string> attributes)
    {
        ArgumentNullException.ThrowIfNull(billing);
        ArgumentNullException.ThrowIfNull(attributes);

        billing.SetAttributes(attributes);
    }
}
