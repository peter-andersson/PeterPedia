using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;

namespace Episodes.Api.Functions;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Required by Azure function runtime.")]
public class Query
{
    private readonly ILogger<Query> _log;
    private readonly IDataStorage<TVShowEntity> _dataStorage;

    public Query(ILogger<Query> log, IDataStorage<TVShowEntity> dataStorage)
    {
        _log = log;
        _dataStorage = dataStorage;
    }

    [FunctionName("Query")]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "query")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        QueryData query = JsonConvert.DeserializeObject<QueryData>(requestBody);

        try
        {
            var queryText = "SELECT * FROM c WHERE LOWER(c.Title) LIKE @search OR LOWER(c.OriginalTitle) LIKE @search ORDER BY c.Title OFFSET @offset LIMIT @limit";

            QueryDefinition queryDefinition = new QueryDefinition(query: queryText)
                .WithParameter("@limit", query.PageSize)
                .WithParameter("@offset", query.Page * query.PageSize)
                .WithParameter("@search", query.Search);

            List<TVShowEntity> entities = await _dataStorage.QueryAsync(queryDefinition);
            var result = new List<TVShow>(entities.Count);
            foreach (TVShowEntity entity in entities)
            {
                result.Add(entity.ConvertToShow());
            }

            return new OkObjectResult(result);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Something went wrong.");
            return new StatusCodeResult(500);
        }
    }
}
