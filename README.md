# SyntaxCircus.RevenueCat.Maui

[![Build](https://github.com/Syntax-Circus/SyntaxCircus.RevenueCat.Maui/actions/workflows/build.yml/badge.svg)](https://github.com/Syntax-Circus/SyntaxCircus.RevenueCat.Maui/actions/workflows/build.yml)
[![NuGet](https://img.shields.io/nuget/v/SyntaxCircus.RevenueCat.Maui.svg)](https://www.nuget.org/packages/SyntaxCircus.RevenueCat.Maui)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE.txt)

Client-side RevenueCat helpers for MAUI apps, built on top of [`Kebechet.Maui.RevenueCat.InAppBilling`](https://www.nuget.org/packages/Kebechet.Maui.RevenueCat.InAppBilling) (the vendor SDK binding — not reimplemented here). Covers SDK initialization, identity sync on login, offering-to-DTO mapping, and a purchase/restore orchestrator.

For backend-side RevenueCat integration (webhook verification, REST clients), see [SyntaxCircus.RevenueCat](https://github.com/Syntax-Circus/SyntaxCircus.RevenueCat).

> **No support guaranteed.** Published as-is and maintained on a best-effort basis. Issues and PRs are welcome, but there's no SLA — fork it or vendor what you need if that's not enough.

## Targets

`net10.0-android` and `net10.0-ios` only, matching the vendor SDK's proven real-world coverage. Add maccatalyst/windows yourself if you've verified `Kebechet.Maui.RevenueCat.InAppBilling` supports them for your use case.

## Setup

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

```csharp
// App.xaml.cs — initialize once, early in the app lifecycle
protected override void OnStart()
{
    RevenueCatInitializer.TryInitialize(_billing, _options.Value);
    base.OnStart();
}
```

These are the **public (publishable)** per-platform keys from the RevenueCat dashboard — safe to ship inside the app binary, distinct from the server-side secret key `SyntaxCircus.RevenueCat` uses.

## Identity sync

Point RevenueCat's `app_user_id` at your own user id as soon as you know it (e.g. after login), so purchases and the TRANSFER webhook event correctly re-associate across reinstalls and new devices:

```csharp
await RevenueCatIdentitySync.SyncLoginAsync(billing, userId, logger, ct);
```

Failures are logged and swallowed — this is best-effort, not something worth failing app startup over.

## Products

```csharp
IReadOnlyList<RevenueCatProduct> products =
    await RevenueCatOfferingsMapper.GetCurrentProductsAsync(billing, ct: ct);
```

## Purchases and restore

```csharp
RevenueCatPurchaseResult result =
    await RevenueCatPurchaseOrchestrator.PurchaseAsync(billing, productIdentifier, logger, ct);

if (result.Success)
{
    // record result.TransactionId / result.AppUserId against your own backend here
}
else if (result.WasCancelled)
{
    // user-initiated cancellation, not an error
}
```

```csharp
RevenueCatPurchaseResult restoreResult =
    await RevenueCatPurchaseOrchestrator.RestoreAsync(billing, userId, logger, ct);
```

`PurchaseAsync` and `RestoreAsync` only own the store interaction — recording a successful purchase against your own backend (subscriber verification, entitlement grants, etc.) is deliberately left to the caller, the same split `SyntaxCircus.RevenueCat`'s webhook reader uses on the backend side.

## Contributing

Issues and pull requests are welcome:
- Keep changes focused, with a clear description of the behavior change.
- Match the existing code style (see `.editorconfig`).
- Call out any breaking changes to the public API in your PR description.

## License

MIT — see [LICENSE.txt](LICENSE.txt).
