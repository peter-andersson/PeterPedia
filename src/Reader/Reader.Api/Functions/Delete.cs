using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;

namespace Reader.Api.Functions;

public class Delete
{
    private readonly ILogger<Delete> _log;
    private readonly IDataStorage<SubscriptionEntity> _dataStorage;    

    public Delete(ILogger<Delete> log, IDataStorage<SubscriptionEntity> dataStorage)
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

        SubscriptionEntity? subscription = await _dataStorage.GetAsync(id);

        if (subscription is null)
        {
            return req.NotFound();
        }

        try
        {
            await _dataStorage.DeleteAsync(subscription);

            _log.LogInformation("Deleted subscription with id {id} and title {title}.", subscription.Id, subscription.Title);

            return req.Ok();
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Something went wrong.");
            return req.InternalServerError();
        }        
    }
}
