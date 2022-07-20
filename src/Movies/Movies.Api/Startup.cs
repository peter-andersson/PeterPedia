using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PeterPedia.Data.Implementations;

[assembly: FunctionsStartup(typeof(Movies.Api.Startup))]

namespace Movies.Api;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        IConfiguration configuration = builder.GetContext().Configuration;

        builder.Services.AddHttpClient();

        builder.Services.AddSingleton<ITheMovieDatabaseService>((s) => new TheMovieDatabaseService(
                    configuration["TheMovieDbAccessToken"],
                    s.GetRequiredService<HttpClient>(),
                    s.GetRequiredService<IMemoryCache>()
                ));

        builder.Services.AddOptions<CosmosOptions>()
            .Configure<IConfiguration>((settings, configuration) => configuration.GetSection("Cosmos").Bind(settings));

        builder.Services.AddOptions<BlobStorageOptions>()
            .Configure<IConfiguration>((settings, configuration) => configuration.GetSection("BlobStorage").Bind(settings));

        builder.Services.AddSingleton<IRepository, CosmosRepository>();
        builder.Services.AddSingleton<IFileStorage, BlobStorage>();
    }
}
