using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;

namespace Movies.Api.Functions;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Required by Azure function runtime.")]
public class Query
{
    private readonly ILogger<Query> _log;
    private readonly IDataStorage<MovieEntity> _dataStorage;

    public Query(ILogger<Query> log, IDataStorage<MovieEntity> dataStorage)
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

        if (string.IsNullOrWhiteSpace(requestBody))
        {
            return req.BadRequest("Missing query data in request");
        }

        QueryData query = JsonConvert.DeserializeObject<QueryData>(requestBody);

        try
        {
            var queryText = "SELECT * FROM c WHERE LOWER(c.Title) LIKE @search OR LOWER(c.OriginalTitle) LIKE @search ORDER BY c.Title OFFSET @offset LIMIT @limit";

            QueryDefinition queryDefinition = new QueryDefinition(query: queryText)
                .WithParameter("@limit", query.PageSize)
                .WithParameter("@offset", query.Page * query.PageSize)
                .WithParameter("@search", query.Search);

            List<MovieEntity> entities = await _dataStorage.QueryAsync(queryDefinition);
            var result = new List<Movie>(entities.Count);
            foreach (MovieEntity entity in entities)
            {
                result.Add(entity.ConvertToMovie());
            }

            return req.Ok(result);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Something went wrong.");
            return req.InternalServerError();
        }
    }
}
