using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;

namespace Episodes.Api.Functions;

public class Get
{
    private readonly IDataStorage<TVShowEntity> _dataStoreage;

    public Get(IDataStorage<TVShowEntity> dataStorage) => _dataStoreage = dataStorage;

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

        TVShowEntity? show = await _dataStoreage.GetAsync(id);

        return show is not null ? req.Ok(show.ConvertToShow()) : req.NotFound();
    }
}
