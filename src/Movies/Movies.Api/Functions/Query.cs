using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;

namespace Movies.Api.Functions;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Required by Azure function runtime.")]
public class Query
{
    private readonly ILogger<Query> _log;
    private readonly CosmosContext _dbContext;

    public Query(ILogger<Query> log, CosmosContext dbContext)
    {
        _log = log;
        _dbContext = dbContext;
    }

    [FunctionName("Query")]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "query")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        Shared.Query query = JsonConvert.DeserializeObject<Shared.Query>(requestBody);

        try
        {
            List<MovieEntity> entities = await _dbContext.GetListAsync(query);
            var result = new List<Movie>(entities.Count);
            foreach (MovieEntity entity in entities)
            {
                result.Add(new Movie()
                {
                    Id = entity.Id,
                    ImdbId = entity.ImdbId,
                    OriginalLanguage = entity.OriginalLanguage,
                    OriginalTitle = entity.OriginalTitle,
                    ReleaseDate = entity.ReleaseDate,
                    RunTime = entity.RunTime,
                    Title = entity.Title,
                    WatchedDate = entity.WatchedDate
                });
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
