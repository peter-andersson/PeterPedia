using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Options;
using PeterPedia.Data.Interface;
using PeterPedia.Data.Models;

namespace PeterPedia.Data.Implementations;

public class CosmosRepository : IRepository
{
    private readonly CosmosClient _cosmosClient;
    private readonly Container _container;
    private readonly string _database;
    private readonly Dictionary<string, Container> _containers = new();

    public CosmosRepository(IOptions<CosmosOptions> options)
    {
        if (string.IsNullOrEmpty(options.Value.EndPointUrl))
        {
            throw new ArgumentException("Please specify a valid EndPointUrl in the appSettings.json file or your Azure Functions Settings.");
        }

        if (string.IsNullOrEmpty(options.Value.AccountKey))
        {
            throw new ArgumentException("Please specify a valid AccountKey in the appSettings.json file or your Azure Functions Settings.");
        }

        if (string.IsNullOrEmpty(options.Value.Container))
        {
            throw new ArgumentException("Please specify a valid Container in the appSettings.json file or your Azure Functions Settings.");
        }

        _cosmosClient = new CosmosClientBuilder(options.Value.EndPointUrl, options.Value.AccountKey)
            .WithApplicationName(options.Value.ApplicationName)
            .Build();

        _container = _cosmosClient.GetContainer(options.Value.Database, options.Value.Container);
        _containers.Add(options.Value.Container, _container);

        _database = options.Value.Database;
    }

    public async Task AddAsync<TEntity>(TEntity entity) where TEntity : BaseEntity => await AddAsync(_container, entity);

    public async Task AddAsync<TEntity>(string containerId, TEntity entity) where TEntity : BaseEntity
    {
        Container container = GetContainer(containerId);

        await AddAsync(container, entity);
    }

    public async Task DeleteAsync<TEntity>(TEntity entity) where TEntity : BaseEntity => await DeleteAsync(_container, entity);

    public async Task DeleteAsync<TEntity>(string containerId, TEntity entity) where TEntity : BaseEntity
    {
        Container container = GetContainer(containerId);

        await DeleteAsync(container, entity);
    }

    public async Task<TEntity?> GetAsync<TEntity>(string id) where TEntity : BaseEntity => await GetAsync<TEntity>(_container, id);

    public async Task<TEntity?> GetAsync<TEntity>(string containerId, string id) where TEntity : BaseEntity
    {
        Container container = GetContainer(containerId);

        return await GetAsync<TEntity>(container, id);
    }

    public async Task<List<TEntity>> QueryAsync<TEntity>(QueryDefinition query) where TEntity : BaseEntity => await QueryAsync<TEntity>(_container, query);

    public Task<List<TEntity>> QueryAsync<TEntity>(string containerId, QueryDefinition query) where TEntity : BaseEntity
    {
        Container container = GetContainer(containerId);

        return QueryAsync<TEntity>(container, query);
    }

    public async Task UpdateAsync<TEntity>(TEntity entity) where TEntity : BaseEntity => await UpdateAsync(_container, entity);

    public async Task UpdateAsync<TEntity>(string containerId, TEntity entity) where TEntity : BaseEntity
    {
        Container container = GetContainer(containerId);

        await UpdateAsync(container, entity);
    }

    private static async Task AddAsync<TEntity>(Container container, TEntity entity) where TEntity : BaseEntity =>
        _ = await container.CreateItemAsync(entity, entity.PartitionKey);

    private static async Task DeleteAsync<TEntity>(Container container, TEntity entity) where TEntity : BaseEntity =>
        await container.DeleteItemAsync<TEntity>(entity.Id, entity.PartitionKey);

    private static async Task<TEntity?> GetAsync<TEntity>(Container container, string id) where TEntity : BaseEntity
    {
        try
        {
            ItemResponse<TEntity> itemResponse = await container.ReadItemAsync<TEntity>(id, new PartitionKey(id));

            return itemResponse.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return default;
        }
    }

    public static async Task<List<TEntity>> QueryAsync<TEntity>(Container container, QueryDefinition query) where TEntity : BaseEntity
    {
        var result = new List<TEntity>();

        using FeedIterator<TEntity> feed = container.GetItemQueryIterator<TEntity>(queryDefinition: query);

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

    public static async Task UpdateAsync<TEntity>(Container container, TEntity entity) where TEntity : BaseEntity =>
        _ = await container.ReplaceItemAsync(entity, entity.Id, entity.PartitionKey);

    private Container GetContainer(string containerId)
    {
        if (_containers.TryGetValue(containerId, out Container? container))
        {
            return container;
        }

        container= _cosmosClient.GetContainer(_database, containerId);
        _containers.Add(containerId, container);
        return container;
    }
}
