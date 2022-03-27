using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PeterPedia.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddBlazoredModal();
builder.Services.AddBlazoredToast();

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddScoped<IAuthorManager, AuthorManager>();
builder.Services.AddScoped<IBookManager, BookManager>();
builder.Services.AddScoped<IMovieManager, MovieManager>();
builder.Services.AddScoped<RSSService>();
builder.Services.AddScoped<TVService>();
builder.Services.AddScoped<VideoService>();
builder.Services.AddScoped<LinkService>();
builder.Services.AddScoped<PhotoService>();

builder.Services.AddScoped<SyncService>();

await builder.Build().RunAsync();
