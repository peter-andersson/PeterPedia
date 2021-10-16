using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using PeterPedia.Client.Reader.Services;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Blazored.Toast;

namespace PeterPedia.Client.Reader
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            var baseAddress = builder.Configuration["BaseAddress"] ?? builder.HostEnvironment.BaseAddress;

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            builder.Services.AddBlazoredToast();

            builder.Services.AddScoped<RSSService>();

            await builder.Build().RunAsync();
        }
    }
}
