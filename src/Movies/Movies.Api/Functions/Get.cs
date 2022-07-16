using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;

namespace Movies.Api.Functions;

public class Get
{
    private readonly IDataStorage<MovieEntity> _dataStoreage;

    public Get(IDataStorage<MovieEntity> dataStorage) => _dataStoreage = dataStorage;

    [FunctionName("Get")]    
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "get/{id}")] HttpRequest req,
        string id,
        CancellationToken _)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return req.BadRequest("Missing query parameter id");
        }

        MovieEntity? movie = await _dataStoreage.GetAsync(id, id);

        return movie is not null ? req.Ok(movie.ConvertToMovie()) : req.NotFound();
    }
}
