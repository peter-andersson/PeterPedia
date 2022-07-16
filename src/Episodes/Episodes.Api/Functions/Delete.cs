using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;

namespace Episodes.Api.Functions;

public class Delete
{
    private readonly ILogger<Delete> _log;
    private readonly IDataStorage<TVShowEntity> _dataStorage;    

    public Delete(ILogger<Delete> log, IDataStorage<TVShowEntity> dataStorage)
    {
        _log = log;
        _dataStorage = dataStorage;
    }

    [FunctionName("Delete")]    
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "delete/{id}")] HttpRequest req,
        string id,
        CancellationToken _)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return req.BadRequest("Missing query parameter id");
        }

        TVShowEntity? show = await _dataStorage.GetAsync(id);

        if (show is null)
        {
            return req.NotFound();
        }

        try
        {
            await _dataStorage.DeleteAsync(show);

            _log.LogInformation("Deleted tv show with id {id} and title {title}.", show.Id, show.Title);

            return req.Ok();
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Something went wrong.");
            return req.InternalServerError();
        }        
    }
}
