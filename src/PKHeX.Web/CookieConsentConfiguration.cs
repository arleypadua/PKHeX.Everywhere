using Blazor.Analytics;
using BytexDigital.Blazor.Components.CookieConsent;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PKHeX.Web.Services;

namespace PKHeX.Web;

public static class CookieConsentConfiguration
{
    public static IServiceCollection ConfigureCookieConsent(this IServiceCollection services)
    {
        return services.AddCookieConsent(o =>
        {
            o.Revision = 1;
            o.PolicyUrl = "/privacy-policy";
    
            // Call optional
            o.UseDefaultConsentPrompt(prompt =>
            {
                prompt.Position = ConsentModalPosition.BottomRight;
                prompt.Layout = ConsentModalLayout.Bar;
                prompt.SecondaryActionOpensSettings = false;
                prompt.AcceptAllButtonDisplaysFirst = false;
            });

            o.Categories.Add(new CookieCategory
            {
                TitleText = new()
                {
                    ["en"] = "Google Services",
                    ["de"] = "Google Dienste"
                },
                DescriptionText = new()
                {
                    ["en"] = "Allows the integration and usage of Google services.",
                    ["de"] = "Erlaubt die Verwendung von Google Diensten."
                },
                Identifier = "google",
                IsPreselected = true,

                Services = new()
                {
                    new CookieCategoryService
                    {
                        Identifier = "google-analytics",
                        PolicyUrl = "https://policies.google.com/privacy",
                        TitleText = new()
                        {
                            ["en"] = "Google Analytics",
                            ["de"] = "Google Analytics"
                        },
                        ShowPolicyText = new()
                        {
                            ["en"] = "Display policies",
                            ["de"] = "Richtlinien anzeigen"
                        },
                    },
                    new CookieCategoryService
                    {
                        Identifier = "google-adsense",
                        PolicyUrl = "https://policies.google.com/privacy",
                        TitleText = new()
                        {
                            ["en"] = "Google AdSense",
                            ["de"] = "Google AdSense"
                        },
                        ShowPolicyText = new()
                        {
                            ["en"] = "Display policies",
                            ["de"] = "Richtlinien anzeigen"
                        },
                    }
                }
            });
        });
    }
}

public static class CookieConsentAnalyticsToggle
{
    public static async Task ConfigureCookieConsentToggle(this WebAssemblyHost host)
    {
        var cookieConsentService = host.Services.GetRequiredService<CookieConsentService>();
        var analytics = host.Services.GetRequiredService<IAnalytics>();
        var logger = host.Services.GetRequiredService<ILoggerFactory>()
            .CreateLogger("CookieConsentAnalyticsToggle");
        
        // no need to dispose as it is only one per instance anyway
        cookieConsentService.CategoryConsentChanged += (object? sender, ConsentChangedArgs e) =>
        {
            if (e.CategoryIdentifier != "google") return;
            
            HandleConsentChanged(analytics, e.ChangedTo == ConsentChangedArgs.ConsentChangeType.Granted, logger);
        };

        var cookiePreferences = await cookieConsentService.GetPreferencesAsync();
        var isGoogleAllowed = cookiePreferences?.IsCategoryAllowed("google") ?? false;
        
        HandleConsentChanged(analytics, isGoogleAllowed, logger);
    }

    private static void HandleConsentChanged(IAnalytics analytics, bool isGoogleAllowed, ILogger logger)
    {
        if (isGoogleAllowed)
        {
            analytics.Enable();
            logger.LogInformation("Google Analytics enabled");
        }
        else
        {
            analytics.Disable();
            logger.LogInformation("Google Analytics disabled");
        }
    }
}