using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;

namespace Reader.Api.Functions;

public class UnreadArticles
{
    private readonly ILogger<UnreadArticles> _log;
    private readonly IDataStorage<ArticleEntity> _dataStorage;

    public UnreadArticles(ILogger<UnreadArticles> log, IDataStorage<ArticleEntity> dataStorage)
    {
        _log = log;
        _dataStorage = dataStorage;
    }

    [FunctionName("UnreadArticles")]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "unread")] HttpRequest req,
        CancellationToken _)
    {
        try
        {
            var query = new QueryDefinition(query: "SELECT * FROM c WHERE c.Type = \"article\" AND IS_NULL(c.ReadDate) ORDER BY c.PublishDate");

            List<ArticleEntity> entities = await _dataStorage.QueryAsync(query);

            var groups = new Dictionary<string, UnreadGroup>();
            
            foreach (ArticleEntity entity in entities)
            {
                var item = new UnreadItem()
                {
                    Id = entity.Id,
                    Title = entity.Title,
                    Subscription = entity.Subscription
                };

                if (!groups.TryGetValue(entity.Group, out UnreadGroup? group))
                {
                    group = new UnreadGroup()
                    {
                        Group = entity.Group
                    };

                    groups.Add(entity.Group, group);
                }
                
                group.Items.Add(item);
            }

            var result = new List<UnreadGroup>();

            foreach (var key in groups.Keys.OrderBy(k => k))
            {
                if (groups.TryGetValue(key, out UnreadGroup? group))
                {
                    result.Add(group);
                }
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
