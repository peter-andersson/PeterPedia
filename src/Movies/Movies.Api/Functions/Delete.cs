using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;

namespace Movies.Api.Functions;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Required by Azure function runtime.")]
public class Delete
{
    private readonly ILogger<Delete> _log;
    private readonly IDataStorage<MovieEntity> _dataStorage;    

    public Delete(ILogger<Delete> log, IDataStorage<MovieEntity> dataStorage)
    {
        _log = log;
        _dataStorage = dataStorage;
    }

    [FunctionName("Delete")]    
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "delete/{id}")] HttpRequest req,
        string id,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return new BadRequestObjectResult("Missing query parameter id");
        }

        MovieEntity? movie = await _dataStorage.GetAsync(id, id);

        if (movie is null)
        {
            return new NotFoundResult();
        }

        try
        {
            await _dataStorage.DeleteAsync(movie);

            _log.LogInformation("Deleted movie with id {id} and title {title}.", movie.Id, movie.Title);

            return new OkResult();
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Something went wrong.");
            return new StatusCodeResult(500);
        }        
    }
}
