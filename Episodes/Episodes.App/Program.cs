using Episodes.App;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Caching.Memory;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSingleton<IMemoryCache, MemoryCache>();

builder.Services.AddSingleton(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddBlazoredToast();

builder.Services.AddSingleton<Navigation>();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, StaticWebAppPolicyProvider>();
builder.Services.AddSingleton<AuthenticationStateProvider, StaticWebAppsAuthenticationStateProvider>();
builder.Services.AddSingleton<ITVService, TVService>();

builder.Services.AddOptions();
builder.Services.AddAuthorizationCore();

await builder.Build().RunAsync();
