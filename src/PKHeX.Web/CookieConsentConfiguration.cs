using BytexDigital.Blazor.Components.CookieConsent;

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