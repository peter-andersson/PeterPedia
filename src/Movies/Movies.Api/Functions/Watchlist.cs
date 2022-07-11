using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;

namespace Movies.Api.Functions;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Required by Azure function runtime.")]
public class Watchlist
{
    private readonly ILogger<Watchlist> _log;
    private readonly CosmosContext _dbContext;

    public Watchlist(ILogger<Watchlist> log, CosmosContext dbContext)
    {
        _log = log;
        _dbContext = dbContext;
    }

    [FunctionName("Watchlist")]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
        CancellationToken cancellationToken)
    {
        try
        {
            List<MovieEntity> entities = await _dbContext.GetWatchListAsync();
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
