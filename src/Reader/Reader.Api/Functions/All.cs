using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;

namespace Reader.Api.Functions;

public class All
{
    private readonly ILogger<All> _log;
    private readonly IRepository _repository;

    public All(ILogger<All> log, IRepository repository)
    {
        _log = log;
        _repository = repository;
    }

    [FunctionName("All")]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "all")] HttpRequest req,
        CancellationToken _)
    {      
        try
        {
            var queryText = "SELECT * FROM c WHERE c.Type = 'subscription' ORDER BY c.Title";

            var queryDefinition = new QueryDefinition(query: queryText);

            List<SubscriptionEntity> entities = await _repository.QueryAsync<SubscriptionEntity>(queryDefinition);
            var result = new List<Subscription>(entities.Count);
            foreach (SubscriptionEntity entity in entities)
            {
                result.Add(entity.ConvertToDTO());
            }

            return req.Ok(result);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Something went wrong.");
            return req.InternalServerError();
        }
    }
}
