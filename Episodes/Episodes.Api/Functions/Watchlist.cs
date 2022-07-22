using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;

namespace Episodes.Api.Functions;

public class Watchlist
{
    private readonly ILogger<Watchlist> _log;
    private readonly IRepository _repository;

    public Watchlist(ILogger<Watchlist> log, IRepository repository)
    {
        _log = log;
        _repository = repository;
    }

    [FunctionName("Watchlist")]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "watchlist")] HttpRequest req,
        CancellationToken _)
    {
        try
        {
            var query = new QueryDefinition(query: "SELECT * FROM c WHERE c.UnwatchedEpisodeCount > 0 ORDER BY c.Title");

            List<TVShowEntity> entities = await _repository.QueryAsync<TVShowEntity>(query);
            var result = new List<TVShow>(entities.Count);
            foreach (TVShowEntity entity in entities)
            {
                result.Add(entity.ConvertToShow());
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
