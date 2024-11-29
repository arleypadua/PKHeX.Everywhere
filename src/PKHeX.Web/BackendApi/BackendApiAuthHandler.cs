using System.Net.Http.Headers;
using PKHeX.Web.Extensions;
using PKHeX.Web.Services.Auth;

namespace PKHeX.Web.BackendApi;

public class BackendApiAuthHandler(
    IConfiguration configuration,
    AuthService authService) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!configuration.GetBackendApiOptions().Enabled)
            return await base.SendAsync(request, cancellationToken);
        
        var token = await authService.GetAuthToken();
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}