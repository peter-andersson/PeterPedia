using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using PeterPedia.Server.Data;
using PeterPedia.Server.Data.Models;
using PeterPedia.Shared;

namespace PeterPedia.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ArticleController : Controller
    {
        private readonly ILogger<ArticleController> _logger;
        private readonly PeterPediaContext _dbContext;

        public ArticleController(ILogger<ArticleController> logger, PeterPediaContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            _logger.LogDebug("Get unread articles");
            var subscriptions = await _dbContext.Subscriptions.Include(s => s.Articles.Where(a => a.ReadDate == null).OrderBy(a => a.PublishDate)).AsSplitQuery().AsNoTracking().ToListAsync().ConfigureAwait(false);
            subscriptions = subscriptions.Where(s => s.Articles.Count > 0).ToList();

            var result = new List<Subscription>(subscriptions.Count);
            foreach (var subscription in subscriptions)
            {
                result.Add(ConvertToSubscription(subscription));
            }

            return Ok(result);
        }

        [HttpGet("history")]
        public async Task<IActionResult> History()
        {
            _logger.LogDebug($"Get read articles");
            var articles = await _dbContext.Articles.AsNoTracking().Where(a => a.ReadDate != null).OrderByDescending(a => a.ReadDate).Take(100).ToListAsync().ConfigureAwait(false);

            var result = new List<Article>(articles.Count);
            foreach (var article in articles)
            {
                result.Add(ConvertToArticle(article));
            }

            return Ok(result);
        }

        [HttpGet("stats")]
        public async Task<IActionResult> Stats()
        {
            _logger.LogDebug($"Get articles statistics");
            int articleCount = await _dbContext.Articles.AsNoTracking().CountAsync().ConfigureAwait(false);

            return Ok(articleCount);
        }

        [HttpGet("read/{articleId:int}")]
        public async Task<IActionResult> Read(int articleId)
        {
            _logger.LogDebug($"Read articleId: {articleId}");

            var article = await _dbContext.Articles.FindAsync(articleId).ConfigureAwait(false);
            if (article is null)
            {
                return NotFound();
            }

            article.ReadDate = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);

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
                Content = articleEF.Content,
                PublishDate = articleEF.PublishDate,
                Url = articleEF.Url,
                ReadDate = articleEF.ReadDate
            };

            return article;
        }

        private static Subscription ConvertToSubscription(SubscriptionEF subscriptionEF)
        {
            if (subscriptionEF is null)
            {
                throw new ArgumentNullException(nameof(subscriptionEF));
            }

            var subscription = new Subscription()
            {
                Id = subscriptionEF.Id,
                Title = subscriptionEF.Title,
                Url = subscriptionEF.Url,
                LastUpdate = subscriptionEF.LastUpdate,
                UpdateIntervalMinute = subscriptionEF.UpdateIntervalMinute,
            };

            foreach (var article in subscriptionEF.Articles)
            {
                subscription.Articles.Add(ConvertToArticle(article));
            }

            return subscription;
        }
    }
}
