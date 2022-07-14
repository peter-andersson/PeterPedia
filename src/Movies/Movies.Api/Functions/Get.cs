using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;

namespace Movies.Api.Functions;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Required by Azure function runtime.")]
public class Get
{
    private readonly IDataStorage<MovieEntity> _dataStoreage;

    public Get(IDataStorage<MovieEntity> dataStorage) => _dataStoreage = dataStorage;

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

        MovieEntity? movie = await _dataStoreage.GetAsync(id, id);

        if (movie is null)
        {
            return new NotFoundResult();
        }

        // 
        return new OkObjectResult(movie.ConvertToMovie());
    }
}
