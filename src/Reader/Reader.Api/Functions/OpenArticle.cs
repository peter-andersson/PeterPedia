using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;

namespace Movies.Api.Functions;

public class OpenArticle
{
    private readonly ILogger<OpenArticle> _log;
    private readonly IDataStorage<ArticleEntity> _dataStorage;

    public OpenArticle(ILogger<OpenArticle> log, IDataStorage<ArticleEntity> dataStorage)
    {
        _log = log;
        _dataStorage = dataStorage;
    }

    [FunctionName("OpenArticle")]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "open/{id}")] HttpRequest req,
        string id,
        CancellationToken _)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return req.BadRequest("Missing id for article to open.");
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

            return req.Redirect(article.Url);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Something went wrong.");
            return req.InternalServerError();
        }
    }
}
