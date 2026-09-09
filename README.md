# SyntaxCircus.RevenueCat.Maui

[![Build](https://github.com/Syntax-Circus/SyntaxCircus.RevenueCat.Maui/actions/workflows/build.yml/badge.svg)](https://github.com/Syntax-Circus/SyntaxCircus.RevenueCat.Maui/actions/workflows/build.yml)
[![NuGet](https://img.shields.io/nuget/v/SyntaxCircus.RevenueCat.Maui.svg)](https://www.nuget.org/packages/SyntaxCircus.RevenueCat.Maui)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE.txt)

Client-side RevenueCat helpers for MAUI apps, built on top of [`Kebechet.Maui.RevenueCat.InAppBilling`](https://www.nuget.org/packages/Kebechet.Maui.RevenueCat.InAppBilling) (the vendor SDK binding — not reimplemented here). Covers SDK initialization, identity sync on login, offering-to-DTO mapping, and a purchase/restore orchestrator.

For backend-side RevenueCat integration (webhook verification, REST clients), see [SyntaxCircus.RevenueCat](https://github.com/Syntax-Circus/SyntaxCircus.RevenueCat).

> **No support guaranteed.** Published as-is and maintained on a best-effort basis. Issues and PRs are welcome, but there's no SLA — fork it or vendor what you need if that's not enough.

## Targets

`net10.0-android` and `net10.0-ios` only, matching the vendor SDK's proven real-world coverage. Add maccatalyst/windows yourself if you've verified `Kebechet.Maui.RevenueCat.InAppBilling` supports them for your use case.

## What this library does

| API | Purpose |
| --- | --- |
| `AddRevenueCatMaui(...)` | Registers the vendor billing service and binds `RevenueCatBillingOptions` from configuration. |
| `RevenueCatInitializer.TryInitialize(...)` | Chooses the correct publishable key and initializes the SDK. |
| `RevenueCatIdentitySync.SyncLoginAsync(...)` | Keeps RevenueCat `app_user_id` aligned with your app user after login. |
| `RevenueCatIdentitySync.SyncLogoutAsync(...)` | Detaches the RevenueCat `app_user_id` at sign-out and returns to an anonymous identity. |
| `RevenueCatOfferingsMapper.GetCurrentProductsAsync(...)` | Flattens the current RevenueCat offering into simple product DTOs. |
| `RevenueCatPurchaseOrchestrator.PurchaseAsync(...)` | Resolves a package and starts a purchase flow. |
| `RevenueCatPurchaseOrchestrator.RestoreAsync(...)` | Restores prior store transactions and re-syncs identity when needed. |
| `RevenueCatManagementUrl.GetAsync(...)` | Returns the subscription management URL (App Store / Play Store / customer portal) for the current user. |
| `RevenueCatCustomerInfoReader.GetAsync(...)` / `.IsEntitled(...)` | Reads the current user's active subscriptions/entitlements for immediate client-side UI feedback. |
| `RevenueCatPaymentEligibility.CanMakePaymentsAsync(...)` | Checks whether the store allows this device/user to make payments, before showing a purchase button. |
| `RevenueCatSubscriberAttributes.SetEmail(...)` / `SetDisplayName(...)` / `SetPhoneNumber(...)` / `SetAttributes(...)` | Syncs user profile data to RevenueCat as subscriber attributes (e.g. for CRM/support tooling). |

## Quick start

### 1. Register the service

```csharp
// MauiProgram.cs
builder.Services.AddRevenueCatMaui(builder.Configuration); // binds "RevenueCat", registers IRevenueCatBilling
```

```json
{
  "RevenueCat": {
    "AndroidApiKey": "goog_...",
    "IosApiKey": "appl_..."
  }
}
```

### 2. Initialize the SDK early

```csharp
// App.xaml.cs — initialize once, early in the app lifecycle
protected override void OnStart()
{
    RevenueCatInitializer.TryInitialize(_billing, _options.Value);
    base.OnStart();
}
```

For test hosts or other non-mobile entry points, you can also choose the platform explicitly:

```csharp
RevenueCatInitializer.TryInitialize(_billing, _options.Value, RevenueCatPlatform.Android);
```

Or supply your own key resolver:

```csharp
RevenueCatInitializer.TryInitialize(_billing, _options.Value, options => options.IosApiKey);
```

These are the **public (publishable)** per-platform keys from the RevenueCat dashboard — safe to ship inside the app binary, distinct from the server-side secret key `SyntaxCircus.RevenueCat` uses.

If your app's user id is already known at startup (e.g. the user is already signed in), pass it
along so RevenueCat initializes directly with that id instead of creating an anonymous user you'd
otherwise alias later via `SyncLoginAsync`:

```csharp
RevenueCatInitializer.TryInitialize(_billing, _options.Value, appUserId: userId);
```

## Common flows

### Identity sync

Point RevenueCat's `app_user_id` at your own user id as soon as you know it (e.g. after login), so purchases and the TRANSFER webhook event correctly re-associate across reinstalls and new devices:

```csharp
await RevenueCatIdentitySync.SyncLoginAsync(billing, userId, logger, ct);
```

Failures are logged and swallowed — this is best-effort, not something worth failing app startup over.

Call `SyncLogoutAsync` at sign-out to detach the `app_user_id` and return RevenueCat to an
anonymous identity — same best-effort swallow behavior as `SyncLoginAsync`:

```csharp
await RevenueCatIdentitySync.SyncLogoutAsync(billing, logger, ct);
```

### Products

```csharp
IReadOnlyList<RevenueCatProduct> products =
    await RevenueCatOfferingsMapper.GetCurrentProductsAsync(billing, ct: ct);
```

### Purchases and restore

```csharp
RevenueCatPurchaseResult result =
    await RevenueCatPurchaseOrchestrator.PurchaseAsync(billing, productIdentifier, logger, ct);

if (result.Success)
{
    // record result.TransactionId / result.AppUserId against your own backend here
}
else if (result.Outcome == RevenueCatPurchaseOutcome.Cancelled)
{
    // user-initiated cancellation, not an error
}
else if (result.Outcome is RevenueCatPurchaseOutcome.Pending or RevenueCatPurchaseOutcome.AlreadyOwned)
{
    // Refresh CustomerInfo; the store may already own the product even though no new
    // transaction completed during this call.
}

RevenueCatCustomerInfo customer =
    await RevenueCatCustomerInfoReader.GetAsync(billing, ct);

if (RevenueCatCustomerInfoReader.IsEntitled(customer, "pro"))
{
    // Enable the entitled client experience.
}
```

If you need custom package selection logic, use the resolver overload of `PurchaseAsync(...)`.

```csharp
RevenueCatPurchaseResult customResult =
    await RevenueCatPurchaseOrchestrator.PurchaseAsync(
        billing,
        productIdentifier,
        logger,
        (packages, id) => packages.FirstOrDefault(p => p.Identifier == id || p.Product.Sku == id),
        ct);
```

```csharp
RevenueCatPurchaseResult restoreResult =
    await RevenueCatPurchaseOrchestrator.RestoreAsync(billing, userId, logger, ct);
```

`PurchaseAsync` and `RestoreAsync` only own the store interaction — recording a successful purchase against your own backend (subscriber verification, entitlement grants, etc.) is deliberately left to the caller, the same split `SyntaxCircus.RevenueCat`'s webhook reader uses on the backend side.

A failed `RevenueCatPurchaseResult` also carries `ErrorStatus` — the vendor SDK's typed
`PurchaseErrorStatus` — alongside the human-readable `ErrorMessage`, so you can `switch` on specific
failure modes (network error, payment pending, etc.) instead of string-matching:

```csharp
switch (result.ErrorStatus)
{
    case PurchaseErrorStatus.NetworkError:
        // offer a retry
        break;
    case PurchaseErrorStatus.PaymentPendingError:
        // tell the user the payment is still settling
        break;
}
```

### Customer info & entitlements

```csharp
RevenueCatCustomerInfo customerInfo = await RevenueCatCustomerInfoReader.GetAsync(billing, ct);
bool isPro = RevenueCatCustomerInfoReader.IsEntitled(customerInfo, "pro");
```

> **This is a convenience for perceived responsiveness only, not a source of truth.** Client-reported
> entitlement info can be stale (the device hasn't synced yet) or spoofed (a jailbroken/rooted device
> can lie to the SDK). Use it for immediate UI feedback (e.g. showing a "Pro" badge without waiting
> on a network round trip) — always verify server-side, via `SyntaxCircus.RevenueCat`'s webhook
> handling and subscriber verification, before actually granting access to paid functionality.

### Payment eligibility

Check before showing a purchase button (e.g. the device may be blocked by parental controls or a
region restriction):

```csharp
if (await RevenueCatPaymentEligibility.CanMakePaymentsAsync(billing, ct))
{
    // show the purchase button
}
```

### Subscriber attributes

```csharp
RevenueCatSubscriberAttributes.SetEmail(billing, email);
RevenueCatSubscriberAttributes.SetDisplayName(billing, displayName);
RevenueCatSubscriberAttributes.SetPhoneNumber(billing, phoneNumber);
RevenueCatSubscriberAttributes.SetAttributes(billing, new Dictionary<string, string> { ["plan"] = "annual" });
```

### Subscription management URL

```csharp
string? managementUrl = await RevenueCatManagementUrl.GetAsync(billing, ct);
```

Unlike the identity-sync helpers, failures here are **not** swallowed — this is typically used to
populate a "Manage Subscription" link, so a thrown exception is more useful to the caller than a
silently missing or broken link.

## Behavior notes

- `PurchaseAsync` returns `WasCancelled = true` for a user-cancelled store flow instead of throwing.
- `PurchaseAsync.Outcome` distinguishes success, cancellation, pending payment, already-owned,
  and other failures without requiring callers to parse vendor error strings. Existing
  `Success` and `WasCancelled` behavior remains compatible.
- `RestoreAsync` logs and returns a failure result for non-cancellation errors.
- The default `TryInitialize(billing, options)` method keeps the existing compile-time Android/iOS behavior.
- The explicit `RevenueCatPlatform` and resolver overloads are the non-breaking escape hatches for tests and custom hosts.
- `SyncLogoutAsync` mirrors `SyncLoginAsync`'s best-effort swallow-and-log behavior (cancellation still propagates).
- `RevenueCatManagementUrl.GetAsync` does not swallow exceptions — like `GetCurrentProductsAsync`, it's a direct query and lets failures propagate to the caller.
- `RevenueCatCustomerInfoReader.GetAsync` and `RevenueCatPaymentEligibility.CanMakePaymentsAsync` also don't swallow exceptions — same direct-query behavior as `RevenueCatManagementUrl.GetAsync`.
- `RevenueCatPurchaseResult.ErrorStatus` is populated wherever the vendor SDK reports a typed `PurchaseErrorStatus`; it's `null` for outcomes that aren't a store error (e.g. "product not found") and for the thrown-exception fallback path in `RestoreAsync`.

## Contributing

Issues and pull requests are welcome:
- Keep changes focused, with a clear description of the behavior change.
- Match the existing code style (see `.editorconfig`).
- Call out any breaking changes to the public API in your PR description.

## License

MIT — see [LICENSE.txt](LICENSE.txt).
