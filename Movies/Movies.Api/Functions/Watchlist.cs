using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;

namespace Movies.Api.Functions;

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
            var query = new QueryDefinition(query: "SELECT * FROM c WHERE IS_NULL(c.WatchedDate) ORDER BY c.Title");

            List<MovieEntity> entities = await _repository.QueryAsync<MovieEntity>(query);
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
