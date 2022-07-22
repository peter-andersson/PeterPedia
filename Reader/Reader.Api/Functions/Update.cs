using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;

namespace Reader.Api.Functions;

public class Update
{
    private readonly ILogger<Update> _log;
    private readonly IRepository _repository;

    public Update(ILogger<Update> log, IRepository repository)
    {
        _log = log;
        _repository = repository;
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

        SubscriptionEntity? existing = await _repository.GetAsync<SubscriptionEntity>(subscription.Id);

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
            await _repository.UpdateAsync(existing);
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
