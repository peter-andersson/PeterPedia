using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;

namespace Reader.Api.Functions;

public class Get
{
    private readonly IDataStorage<SubscriptionEntity> _dataStoreage;

    public Get(IDataStorage<SubscriptionEntity> dataStorage) => _dataStoreage = dataStorage;

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

        SubscriptionEntity? subscription = await _dataStoreage.GetAsync(id);

        return subscription is not null ? req.Ok(subscription.ConvertToDTO()) : req.NotFound();
    }
}
