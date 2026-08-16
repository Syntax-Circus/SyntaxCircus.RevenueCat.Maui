global using Maui.RevenueCat.InAppBilling; // RevenueCatBillingInstaller.AddRevenueCatBilling(IServiceCollection)
global using Maui.RevenueCat.InAppBilling.Enums;
global using Maui.RevenueCat.InAppBilling.Extensions;
global using Maui.RevenueCat.InAppBilling.Models;
global using Maui.RevenueCat.InAppBilling.Services;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;

// Maui.RevenueCat.InAppBilling.Enums and Microsoft.Extensions.Logging both define a LogLevel
// type — alias globally so every file in this package resolves the same (MEL) one by default.
global using LogLevel = Microsoft.Extensions.Logging.LogLevel;
