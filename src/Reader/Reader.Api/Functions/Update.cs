using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;

namespace Reader.Api.Functions;

public class Update
{
    private readonly ILogger<Update> _log;
    private readonly IDataStorage<SubscriptionEntity> _dataStorage;

    public Update(ILogger<Update> log, IDataStorage<SubscriptionEntity> dbContext)
    {
        _log = log;
        _dataStorage = dbContext;
    }

    [FunctionName("Update")]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "update")] HttpRequest req,
        CancellationToken _)
    {
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();

        if (string.IsNullOrWhiteSpace(requestBody))
        {
            return req.BadRequest("Missing request body");
        }

        Subscription subscription = JsonConvert.DeserializeObject<Subscription>(requestBody);

        if (subscription is null)
        {
            return req.BadRequest("Missing movie object");
        }

        SubscriptionEntity? existing = await _dataStorage.GetAsync(subscription.Id);

        if (existing is null)
        {
            return req.NotFound();
        }
        
        existing.Title = subscription.Title;
        existing.UpdateIntervalMinute = subscription.UpdateIntervalMinute;
        existing.NextUpdate = DateTime.UtcNow.AddMinutes(-1);
        existing.Url = subscription.Url;
        existing.Group = subscription.Group;

        try
        {            
            await _dataStorage.UpdateAsync(existing);
            _log.LogInformation("Updated subscription with id {id}, title {title}", existing.Id, existing.Title);

            return req.Ok();
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Something went wrong.");
            return req.InternalServerError();
        }
    }
}
