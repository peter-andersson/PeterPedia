using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;

namespace Movies.Api.Functions;

public class DeleteArticle
{
    private readonly ILogger<DeleteArticle> _log;
    private readonly IDataStorage<ArticleEntity> _dataStorage;

    public DeleteArticle(ILogger<DeleteArticle> log, IDataStorage<ArticleEntity> dataStorage)
    {
        _log = log;
        _dataStorage = dataStorage;
    }

    [FunctionName("DeleteArticle")]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "deleteArticle/{id}")] HttpRequest req,
        string id,
        CancellationToken _)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return req.BadRequest("Missing id for article to delete.");
        }

        ArticleEntity? article = await _dataStorage.GetAsync(id);

        if (article is null)
        {
            return req.NotFound();
        }

        try
        {
            article.ReadDate = DateTime.UtcNow;

            await _dataStorage.UpdateAsync(article);

            return req.Ok();
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Something went wrong.");
            return req.InternalServerError();
        }
    }
}
