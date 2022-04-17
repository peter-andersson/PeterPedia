using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace PeterPedia.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public partial class ArticleController : Controller
{
    private readonly ILogger<ArticleController> _logger;

    private readonly PeterPediaContext _dbContext;

    public ArticleController(ILogger<ArticleController> logger, PeterPediaContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetAsync()
    {
        List<SubscriptionEF> subscriptions = await _dbContext.Subscriptions.Include(s => s.Articles.Where(a => a.ReadDate == null).OrderBy(a => a.PublishDate)).AsSplitQuery().ToListAsync().ConfigureAwait(false);
        subscriptions = subscriptions.Where(s => s.Articles.Count > 0).ToList();

        var tmpArticles = new Dictionary<string, UnreadArticle>();

        foreach (SubscriptionEF subscription in subscriptions)
        {
            if (tmpArticles.TryGetValue(subscription.Group ?? subscription.Title, out UnreadArticle? unreadArticle))
            {
                foreach (ArticleEF article in subscription.Articles)
                {
                    unreadArticle.Articles.Add(ConvertToArticle(article));
                }                
            }
            else
            {
                unreadArticle = new UnreadArticle()
                {
                    Group = subscription.Group ?? subscription.Title,
                };

                foreach (ArticleEF article in subscription.Articles)
                {
                    unreadArticle.Articles.Add(ConvertToArticle(article));
                }

                tmpArticles.Add(subscription.Group ?? subscription.Title, unreadArticle);
            }
        }
        
        var keys = tmpArticles.Keys.ToList();
        keys.Sort();
        var result = new List<UnreadArticle>(keys.Count);
        foreach (var key in keys)
        {
            result.Add(tmpArticles[key]);
        }

        return Ok(result);
    }

    [HttpGet("history")]
    public async Task<IActionResult> HistoryAsync()
    {
        List<ArticleEF> articles = await _dbContext.Articles.Where(a => a.ReadDate != null).OrderByDescending(a => a.ReadDate).Take(100).ToListAsync().ConfigureAwait(false);

        var result = new List<Article>(articles.Count);
        foreach (ArticleEF article in articles)
        {
            result.Add(ConvertToArticle(article));
        }

        return Ok(result);
    }
 
    [HttpGet("read/{articleId:int}")]
    public async Task<IActionResult> ReadAsync(int articleId)
    {
        ArticleEF? article = await _dbContext.Articles.Where(a => a.Id == articleId).AsTracking().SingleOrDefaultAsync();
        if (article is null)
        {
            return NotFound();
        }

        LogMessage.ReaderReadArticle(_logger, article);

        article.ReadDate = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return Ok();
    }

    private static Article ConvertToArticle(ArticleEF articleEF)
    {
        if (articleEF is null)
        {
            throw new ArgumentNullException(nameof(articleEF));
        }

        var article = new Article()
        {
            Id = articleEF.Id,
            Title = articleEF.Title,
            PublishDate = articleEF.PublishDate,
            Url = articleEF.Url,
            ReadDate = articleEF.ReadDate,
            Feed = articleEF.Subscription?.Title ?? string.Empty,
        };

        return article;
    }
}

