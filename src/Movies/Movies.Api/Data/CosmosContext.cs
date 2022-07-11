using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Movies.Api.Models;

namespace Movies.Api.Data;

public class CosmosContext
{
    private readonly string _databaseName = "peterpedia";
    private readonly string _containerName = "movies";

    private readonly Container _container;

    public CosmosContext(CosmosClient cosmosClient) => _container = cosmosClient.GetContainer(_databaseName, _containerName);

    public async Task<MovieEntity> GetAsync(string id)
    {
        try
        {
            ItemResponse<MovieEntity> itemResponse = await _container.ReadItemAsync<MovieEntity>(id, new PartitionKey(id));

            return itemResponse.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task AddAsync(MovieEntity entity) => _ = await _container.CreateItemAsync(entity, new PartitionKey(entity.Id));

    public async Task UpdateAsync(MovieEntity entity) => _ = await _container.ReplaceItemAsync(entity, entity.Id, new PartitionKey(entity.Id));

    public async Task<List<MovieEntity>> GetWatchListAsync()
    {
        var result = new List<MovieEntity>();

        var query = new QueryDefinition(
            query: "SELECT * FROM c WHERE IS_NULL(c.WatchedDate)"
        );
        
        using FeedIterator<MovieEntity> feed = _container.GetItemQueryIterator<MovieEntity>(queryDefinition: query);

        while (feed.HasMoreResults)
        {
            FeedResponse<MovieEntity> response = await feed.ReadNextAsync();
            foreach (MovieEntity item in response)
            {
                result.Add(item);
            }
        }

        return result;
    }

    public async Task<List<MovieEntity>> GetListAsync(Query queryData)
    {
        var result = new List<MovieEntity>();

        var queryText = "SELECT * FROM c WHERE LOWER(c.Title) LIKE @search OR LOWER(c.OriginalTitle) LIKE @search ORDER BY c.Title OFFSET @offset LIMIT @limit";

        QueryDefinition query = new QueryDefinition(query: queryText)
            .WithParameter("@limit", queryData.PageSize)
            .WithParameter("@offset", queryData.Page * queryData.PageSize)
            .WithParameter("@search", queryData.Search);

        using FeedIterator<MovieEntity> feed = _container.GetItemQueryIterator<MovieEntity>(queryDefinition: query);

        while (feed.HasMoreResults)
        {
            FeedResponse<MovieEntity> response = await feed.ReadNextAsync();
            foreach (MovieEntity item in response)
            {
                result.Add(item);
            }
        }

        return result;
    }
}
