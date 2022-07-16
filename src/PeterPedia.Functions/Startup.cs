using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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

        builder.Services.AddSingleton<ITheMovieDatabaseService>((s) => new TheMovieDatabaseService(
                            configuration["TheMovieDbAccessToken"],
                            s.GetRequiredService<HttpClient>(),
                            s.GetRequiredService<IMemoryCache>()
                        ));

        builder.Services.AddSingleton<IDataStorage<TVShowEntity>>((s) =>
        {
            var cosmosOptions = new CosmosOptions()
            {
                ApplicationName = configuration["Cosmos:ApplicationName"],
                EndPointUrl = configuration["Cosmos:EndPointUrl"],
                AccountKey = configuration["Cosmos:AccountKey"],
                Database = configuration["Cosmos:Database"],
                Container = configuration["Cosmos:EpisodesContainer"]
            };
            return new CosmosDataStorage<TVShowEntity>(Options.Create(cosmosOptions));
        });

        builder.Services.AddSingleton<IDataStorage<MovieEntity>>((s) =>
        {
            var cosmosOptions = new CosmosOptions()
            {
                ApplicationName = configuration["Cosmos:ApplicationName"],
                EndPointUrl = configuration["Cosmos:EndPointUrl"],
                AccountKey = configuration["Cosmos:AccountKey"],
                Database = configuration["Cosmos:Database"],
                Container = configuration["Cosmos:MoviesContainer"]
            };
            return new CosmosDataStorage<MovieEntity>(Options.Create(cosmosOptions));
        });

        builder.Services.AddSingleton<IDataStorage<BookEntity>>((s) =>
        {
            var cosmosOptions = new CosmosOptions()
            {
                ApplicationName = configuration["Cosmos:ApplicationName"],
                EndPointUrl = configuration["Cosmos:EndPointUrl"],
                AccountKey = configuration["Cosmos:AccountKey"],
                Database = configuration["Cosmos:Database"],
                Container = configuration["Cosmos:BooksContainer"]
            };
            return new CosmosDataStorage<BookEntity>(Options.Create(cosmosOptions));
        });

        builder.Services.AddSingleton<IDataStorage<SubscriptionEntity>>((s) =>
        {
            var cosmosOptions = new CosmosOptions()
            {
                ApplicationName = configuration["Cosmos:ApplicationName"],
                EndPointUrl = configuration["Cosmos:EndPointUrl"],
                AccountKey = configuration["Cosmos:AccountKey"],
                Database = configuration["Cosmos:Database"],
                Container = configuration["Cosmos:ReaderContainer"]
            };
            return new CosmosDataStorage<SubscriptionEntity>(Options.Create(cosmosOptions));
        });

        builder.Services.AddSingleton<IDataStorage<ArticleEntity>>((s) =>
        {
            var cosmosOptions = new CosmosOptions()
            {
                ApplicationName = configuration["Cosmos:ApplicationName"],
                EndPointUrl = configuration["Cosmos:EndPointUrl"],
                AccountKey = configuration["Cosmos:AccountKey"],
                Database = configuration["Cosmos:Database"],
                Container = configuration["Cosmos:ReaderContainer"]
            };
            return new CosmosDataStorage<ArticleEntity>(Options.Create(cosmosOptions));
        });
    }
}
