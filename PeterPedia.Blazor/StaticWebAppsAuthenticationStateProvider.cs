using System.Net.Http.Json;
using System.Security.Claims;
using PeterPedia.Blazor.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace PeterPedia.Blazor;

// From https://github.com/anthonychu/blazor-auth-static-web-apps
public class StaticWebAppsAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly HttpClient _http;

    public StaticWebAppsAuthenticationStateProvider(IWebAssemblyHostEnvironment environment) => _http = new HttpClient { BaseAddress = new Uri(environment.BaseAddress) };

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            AuthenticationData? data = await _http.GetFromJsonAsync<AuthenticationData>("/.auth/me");

            if (data is null)
            {
                return new AuthenticationState(new ClaimsPrincipal());
            }

            ClientPrincipal? principal = data.ClientPrincipal;
            if (principal is null)
            {
                return new AuthenticationState(new ClaimsPrincipal());
            }

            principal.UserRoles = principal.UserRoles.Except(new string[] { "anonymous" }, StringComparer.CurrentCultureIgnoreCase);

            if (!principal.UserRoles.Any())
            {
                return new AuthenticationState(new ClaimsPrincipal());
            }

            var identity = new ClaimsIdentity(principal.IdentityProvider);
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, principal.UserId));
            identity.AddClaim(new Claim(ClaimTypes.Name, principal.UserDetails));
            identity.AddClaims(principal.UserRoles.Select(r => new Claim(ClaimTypes.Role, r)));
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
        catch
        {
            return new AuthenticationState(new ClaimsPrincipal());
        }
    }
}
