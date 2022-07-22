using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;

namespace Reader.Api.Functions;

public class Get
{
    private readonly IRepository _repository;

    public Get(IRepository repository) => _repository = repository;

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

        SubscriptionEntity? subscription = await _repository.GetAsync<SubscriptionEntity>(id);

        return subscription is not null ? req.Ok(subscription.ConvertToDTO()) : req.NotFound();
    }
}
