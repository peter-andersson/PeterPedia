using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;

namespace Movies.Api.Functions;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Required by Azure function runtime.")]
public class Get
{
    private readonly ILogger<Get> _log;
    private readonly CosmosContext _dbContext;

    public Get(ILogger<Get> log, CosmosContext dbContext)
    {
        _log = log;
        _dbContext = dbContext;
    }

    [FunctionName("Get")]    
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "get/{id}")] HttpRequest req,
        string id,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return new BadRequestObjectResult("Missing query parameter id");
        }

        MovieEntity movie = await _dbContext.GetAsync(id);

        if (movie is null)
        {
            return new NotFoundResult();
        }

        return new OkObjectResult(movie.ConvertToMovie());
    }
}
