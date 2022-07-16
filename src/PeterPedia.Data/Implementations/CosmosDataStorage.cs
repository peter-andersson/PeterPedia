using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Options;
using PeterPedia.Data.Interface;
using PeterPedia.Data.Models;

namespace PeterPedia.Data.Implementations;

public class CosmosDataStorage<TEntity> : IDataStorage<TEntity> where TEntity : IEntity
{
    private readonly CosmosClient _cosmosClient;

    public CosmosDataStorage(IOptions<CosmosOptions> options)
    {
        if (string.IsNullOrEmpty(options.Value.EndPointUrl))
        {
            throw new ArgumentException("Please specify a valid EndPointUrl in the appSettings.json file or your Azure Functions Settings.");
        }

        if (string.IsNullOrEmpty(options.Value.AccountKey))
        {
            throw new ArgumentException("Please specify a valid AccountKey in the appSettings.json file or your Azure Functions Settings.");
        }

        _cosmosClient = new CosmosClientBuilder(options.Value.EndPointUrl, options.Value.AccountKey)
            .WithApplicationName(options.Value.ApplicationName)
            .Build();

        Container = _cosmosClient.GetContainer(options.Value.Database, options.Value.Container);
    }

    public Container Container { get; }

    public async Task AddAsync(TEntity entity) => _ = await Container.CreateItemAsync(entity, new PartitionKey(entity.Id));

    public async Task DeleteAsync(TEntity entity) => await Container.DeleteItemAsync<TEntity>(entity.Id, new PartitionKey(entity.Id));

    public async Task<TEntity?> GetAsync(string id)
    {
        try
        {
            ItemResponse<TEntity> itemResponse = await Container.ReadItemAsync<TEntity>(id, new PartitionKey(id));

            return itemResponse.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return default;
        }
    }

    public async Task<List<TEntity>> QueryAsync(QueryDefinition query)
    {
        var result = new List<TEntity>();

        using FeedIterator<TEntity> feed = Container.GetItemQueryIterator<TEntity>(queryDefinition: query);

        while (feed.HasMoreResults)
        {
            FeedResponse<TEntity> response = await feed.ReadNextAsync();
            foreach (TEntity item in response)
            {
                result.Add(item);
            }
        }

        return result;
    }

    public async Task UpdateAsync(TEntity entity) => _ = await Container.ReplaceItemAsync(entity, entity.Id, new PartitionKey(entity.Id));
}
