namespace SyntaxCircus.RevenueCat.Maui.Tests;

public class RevenueCatSubscriberAttributesTests
{
    [Fact]
    public void SetEmail_NullBilling_ThrowsArgumentNullException()
        => Should.Throw<ArgumentNullException>(() => RevenueCatSubscriberAttributes.SetEmail(null!, "a@b.com"));

    [Fact]
    public void SetEmail_NullEmail_ThrowsArgumentNullException()
        => Should.Throw<ArgumentNullException>(() => RevenueCatSubscriberAttributes.SetEmail(Substitute.For<IRevenueCatBilling>(), null!));

    [Fact]
    public void SetEmail_ValidEmail_CallsBilling()
    {
        var billing = Substitute.For<IRevenueCatBilling>();

        RevenueCatSubscriberAttributes.SetEmail(billing, "a@b.com");

        billing.Received(1).SetEmail("a@b.com");
    }

    [Fact]
    public void SetDisplayName_NullBilling_ThrowsArgumentNullException()
        => Should.Throw<ArgumentNullException>(() => RevenueCatSubscriberAttributes.SetDisplayName(null!, "Jane"));

    [Fact]
    public void SetDisplayName_NullName_ThrowsArgumentNullException()
        => Should.Throw<ArgumentNullException>(() => RevenueCatSubscriberAttributes.SetDisplayName(Substitute.For<IRevenueCatBilling>(), null!));

    [Fact]
    public void SetDisplayName_ValidName_CallsBilling()
    {
        var billing = Substitute.For<IRevenueCatBilling>();

        RevenueCatSubscriberAttributes.SetDisplayName(billing, "Jane");

        billing.Received(1).SetDisplayName("Jane");
    }

    [Fact]
    public void SetPhoneNumber_NullBilling_ThrowsArgumentNullException()
        => Should.Throw<ArgumentNullException>(() => RevenueCatSubscriberAttributes.SetPhoneNumber(null!, "+15555550100"));

    [Fact]
    public void SetPhoneNumber_NullPhone_ThrowsArgumentNullException()
        => Should.Throw<ArgumentNullException>(() => RevenueCatSubscriberAttributes.SetPhoneNumber(Substitute.For<IRevenueCatBilling>(), null!));

    [Fact]
    public void SetPhoneNumber_ValidPhone_CallsBilling()
    {
        var billing = Substitute.For<IRevenueCatBilling>();

        RevenueCatSubscriberAttributes.SetPhoneNumber(billing, "+15555550100");

        billing.Received(1).SetPhoneNumber("+15555550100");
    }

    [Fact]
    public void SetAttributes_NullBilling_ThrowsArgumentNullException()
        => Should.Throw<ArgumentNullException>(() => RevenueCatSubscriberAttributes.SetAttributes(null!, new Dictionary<string, string>()));

    [Fact]
    public void SetAttributes_NullAttributes_ThrowsArgumentNullException()
        => Should.Throw<ArgumentNullException>(() => RevenueCatSubscriberAttributes.SetAttributes(Substitute.For<IRevenueCatBilling>(), null!));

    [Fact]
    public void SetAttributes_ValidAttributes_CallsBilling()
    {
        var billing = Substitute.For<IRevenueCatBilling>();
        var attributes = new Dictionary<string, string> { ["favorite_color"] = "blue" };

        RevenueCatSubscriberAttributes.SetAttributes(billing, attributes);

        billing.Received(1).SetAttributes(attributes);
    }
}
