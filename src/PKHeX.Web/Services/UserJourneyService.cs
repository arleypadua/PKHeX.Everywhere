using Microsoft.AspNetCore.Components;
using PKHeX.Web.Extensions;

namespace PKHeX.Web.Services;

public class UserJourneyService(
    NavigationManager navigation)
{
    public bool IgnoredFirstLoad { get; private set; }
    
    public void IgnoreFirstLoadAndGoToHome()
    {
        IgnoredFirstLoad = true;
        navigation.NavigateToHomePage();
    }
}