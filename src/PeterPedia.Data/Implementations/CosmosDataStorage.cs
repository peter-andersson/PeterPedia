using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Options;
using PeterPedia.Data.Interface;
using PeterPedia.Data.Models;

namespace PeterPedia.Data.Implementations;

public class CosmosDataStorage<TEntity> : IDataStorage<TEntity> where TEntity : IEntity
{
    private readonly CosmosClient _cosmosClient;
    private readonly Container _container;
    
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

        _container = _cosmosClient.GetContainer(options.Value.Database, options.Value.Container);
    }

    public async Task AddAsync(TEntity entity) => _ = await _container.CreateItemAsync(entity, new PartitionKey(entity.PartitionKey));

    public async Task DeleteAsync(TEntity entity) => await _container.DeleteItemAsync<TEntity>(entity.Id, new PartitionKey(entity.PartitionKey));

    public async Task<TEntity?> GetAsync(string id, string partitionKey)
    {
        try
        {
            ItemResponse<TEntity> itemResponse = await _container.ReadItemAsync<TEntity>(id, new PartitionKey(partitionKey));

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

        using FeedIterator<TEntity> feed = _container.GetItemQueryIterator<TEntity>(queryDefinition: query);

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

    public async Task UpdateAsync(TEntity entity) => _ = await _container.ReplaceItemAsync(entity, entity.Id, new PartitionKey(entity.PartitionKey));
}
