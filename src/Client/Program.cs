using Blazored.Toast;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PeterPedia.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddBlazoredToast();

builder.Services.AddScoped<BookService>();
builder.Services.AddScoped<MovieService>();
builder.Services.AddScoped<RSSService>();
builder.Services.AddScoped<TVService>();
builder.Services.AddScoped<VideoService>();
builder.Services.AddScoped<LinkService>();

await builder.Build().RunAsync();
