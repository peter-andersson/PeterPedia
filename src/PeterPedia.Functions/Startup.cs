using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PeterPedia.Data.Implementations;
using PeterPedia.Data.Interface;
using PeterPedia.Data.Models;
using TheMovieDatabase;

[assembly: FunctionsStartup(typeof(PeterPedia.Functions.Startup))]

namespace PeterPedia.Functions;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        IConfiguration configuration = builder.GetContext().Configuration;

        builder.Services.AddHttpClient();

        builder.Services.AddOptions<CosmosOptions>()
            .Configure<IConfiguration>((settings, configuration) => configuration.GetSection("Cosmos").Bind(settings));

        builder.Services.AddSingleton<ITheMovieDatabaseService>((s) => new TheMovieDatabaseService(
                            configuration["TheMovieDbAccessToken"],
                            s.GetRequiredService<HttpClient>(),
                            s.GetRequiredService<IMemoryCache>()
                        ));

        builder.Services.AddSingleton<IRepository, CosmosRepository>();
    }
}
