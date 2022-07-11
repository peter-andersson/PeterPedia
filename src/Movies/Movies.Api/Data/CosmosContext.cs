using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Movies.Api.Models;

namespace Movies.Api.Data;

public class CosmosContext
{
    private CosmosClient _cosmosClient;

    private readonly string _databaseName = "peterpedia";
    private readonly string _containerName = "movies";

    public CosmosContext(CosmosClient cosmosClient)
    {
        _cosmosClient = cosmosClient;
    }

    public async Task<MovieEntity> GetAsync(string id)
    {
        Container container = _cosmosClient.GetContainer(_databaseName, _containerName);

        try
        {
            ItemResponse<MovieEntity> itemResponse = await container.ReadItemAsync<MovieEntity>(id, new PartitionKey(id));

            return itemResponse.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task AddAsync(MovieEntity entity)
    {
        Container container = _cosmosClient.GetContainer(_databaseName, _containerName);

        ItemResponse<MovieEntity> itemResponse = await container.CreateItemAsync(entity, new PartitionKey(entity.Id));
    }
}
