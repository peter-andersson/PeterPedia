using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;

namespace Movies.Api.Functions;

public class OpenArticle
{
    private readonly ILogger<OpenArticle> _log;
    private readonly IRepository _repository;

    public OpenArticle(ILogger<OpenArticle> log, IRepository repository)
    {
        _log = log;
        _repository = repository;
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

        ArticleEntity? article = await _repository.GetAsync<ArticleEntity>(id);

        if (article is null)
        {
            return req.NotFound();
        }

        try
        {
            article.ReadDate = DateTime.UtcNow;

            await _repository.UpdateAsync(article);

            return req.Redirect(article.Url);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Something went wrong.");
            return req.InternalServerError();
        }
    }
}
