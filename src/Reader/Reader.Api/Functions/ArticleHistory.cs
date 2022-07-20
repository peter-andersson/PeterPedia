using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;

namespace Reader.Api.Functions;

public class ArticleHistory
{
    private readonly ILogger<ArticleHistory> _log;
    private readonly IRepository _repository;

    public ArticleHistory(ILogger<ArticleHistory> log, IRepository repository)
    {
        _log = log;
        _repository = repository;
    }

    [FunctionName("ArticleHistory")]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "history")] HttpRequest req,
        CancellationToken _)
    {
        try
        {
            var query = new QueryDefinition(query: "SELECT * FROM c WHERE c.Type = 'article' AND NOT IS_NULL(c.ReadDate) ORDER BY c.ReadDate DESC OFFSET 0 LIMIT 20");

            List<ArticleEntity> entities = await _repository.QueryAsync<ArticleEntity>(query);
            var result = new List<HistoryArticle>(entities.Count);
            foreach (ArticleEntity entity in entities)
            {
                var history = new HistoryArticle()
                {
                    Title = entity.Title,
                    Url = entity.Url,
                    ReadDate = entity.ReadDate ?? DateTime.UtcNow,
                };

                result.Add(history);
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
