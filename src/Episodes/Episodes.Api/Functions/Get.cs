using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;

namespace Episodes.Api.Functions;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Required by Azure function runtime.")]
public class Get
{
    private readonly IDataStorage<TVShowEntity> _dataStoreage;

    public Get(IDataStorage<TVShowEntity> dataStorage) => _dataStoreage = dataStorage;

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

        TVShowEntity? show = await _dataStoreage.GetAsync(id, id);

        if (show is null)
        {
            return new NotFoundResult();
        }

        //
        return new OkObjectResult(show.ConvertToShow());
    }
}
