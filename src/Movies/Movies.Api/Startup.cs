using System;
using System.Net.Http;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Movies.Api.Data;
using TheMovieDatabase;

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

        builder.Services.AddSingleton((s) => {
            var endpoint = configuration["EndPointUrl"];
            if (string.IsNullOrEmpty(endpoint))
            {
                throw new ArgumentException("Please specify a valid EndPointUrl in the appSettings.json file or your Azure Functions Settings.");
            }

            var authKey = configuration["AccountKey"];
            if (string.IsNullOrEmpty(authKey))
            {
                throw new ArgumentException("Please specify a valid AccountKey in the appSettings.json file or your Azure Functions Settings.");
            }

            // 
            return new CosmosClientBuilder(endpoint, authKey)
                .WithApplicationName("PeterPedia.Movies")
                .Build();
        });

        builder.Services.AddAzureClients(azureBuilder => azureBuilder.AddBlobServiceClient(configuration["BlobStorage"]));

        builder.Services.AddSingleton<CosmosContext>();
    }


}
