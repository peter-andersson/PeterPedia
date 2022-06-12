using Microsoft.EntityFrameworkCore;
using CodeHollow.FeedReader.Feeds;
using System.Xml;
using CodeHollow.FeedReader;
using Ganss.XSS;
using System.Text.Json;
using System.Security.Cryptography;
using System.Text;
using HtmlAgilityPack;
using Quartz;

namespace PeterPedia.Jobs;

[DisallowConcurrentExecution]
public partial class RefreshArticleJob : IJob
{
    private readonly ILogger<RefreshArticleJob> _logger;
    private readonly PeterPediaContext _dbContext;
    private readonly HtmlSanitizer _sanitizer;
    private readonly IHttpClientFactory _httpClientFactory;

    public RefreshArticleJob(ILogger<RefreshArticleJob> logger, PeterPediaContext dbContext, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _dbContext = dbContext;
        _httpClientFactory = httpClientFactory;

        _sanitizer = new HtmlSanitizer
        {
            KeepChildNodes = true,
        };
        _sanitizer.AllowedTags.Clear();
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            LogMessage.ExecuteJob(_logger, "RefreshArticleJob");

            List<SubscriptionEF> subscriptions = await _dbContext.Subscriptions
                .Include(s => s.Articles)
                .AsSplitQuery()
                .AsTracking()
                .ToListAsync()
                .ConfigureAwait(false);

            var updateCount = 0;
            foreach (SubscriptionEF subscription in subscriptions)
            {
                if (updateCount >= 10)
                {
                    break;
                }

                if (subscription.LastUpdate.AddMinutes(subscription.UpdateIntervalMinute) < DateTime.UtcNow)
                {
                    await UpdateSubscriptionAsync(subscription).ConfigureAwait(false);

                    subscription.LastUpdate = DateTime.UtcNow;

                    updateCount += 1;
                }
            }

            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            LogMessage.ReaderException(_logger, "Exception in RefreshArticleService.Execute", e);
        }
    }

    private async Task UpdateSubscriptionAsync(SubscriptionEF subscription)
    {
        if (subscription is null)
        {
            throw new ArgumentNullException(nameof(subscription));
        }

        LogMessage.ReaderUpdateFeed(_logger, subscription.Title);

        try
        {
            Feed? data = await FeedReader.ReadAsync(subscription.Url);
            if (data is null)
            {
                return;
            }

            var hash = CalculateHash(data.Items);

            if (FeedHashNotChanged(hash, subscription.Hash))
            {
                LogMessage.ReaderFeedNotChanged(_logger);
                return;
            }

            subscription.Hash = hash;

            IEnumerable<ArticleEF> articles = await ConvertFeedItemsAsync(data.Items);

            DateTime ageLimit = DateTime.UtcNow.AddDays(-30);
            foreach (ArticleEF article in articles)
            {
                if (article.PublishDate < ageLimit)
                {
                    continue;
                }

                ArticleEF? existingArticle = subscription.Articles.Where(a => a.Url == article.Url).FirstOrDefault();

                if (existingArticle == null)
                {
                    LogMessage.ReaderAddArticle(_logger, article.Title, subscription.Title);
                    subscription.Articles.Add(article);
                }
            }
        }
        catch (TaskCanceledException ex)
        {
            LogMessage.ReaderFeedException(_logger, subscription.Title, ex);
        }
        catch (XmlException x)
        {
            LogMessage.ReaderFeedException(_logger, subscription.Title, x);
        }
    }

    private static byte[] CalculateHash(ICollection<FeedItem> items)
    {
        if (items is null)
        {
            throw new ArgumentNullException(nameof(items));
        }

        var tmp = new List<string>(items.Count * 2);

        foreach (FeedItem item in items)
        {
            tmp.Add(item.Content);
            tmp.Add(item.Link);
        }

        var json = JsonSerializer.Serialize(tmp);
        return GetSha256Hash(json);
    }

    private static byte[] GetSha256Hash(string input)
    {
        using var shaHash = SHA256.Create();

        return shaHash.ComputeHash(Encoding.UTF8.GetBytes(input));
    }

    private async Task<IEnumerable<ArticleEF>> ConvertFeedItemsAsync(ICollection<FeedItem> items)
    {
        if (items is null)
        {
            throw new ArgumentNullException(nameof(items));
        }

        var articles = new List<ArticleEF>(items.Count);

        foreach (FeedItem item in items)
        {
            var article = new ArticleEF()
            {
                Title = item.Title,
                Url = GetArticleLink(item),
                PublishDate = item.PublishingDate ?? DateTime.UtcNow,
            };

            if (string.IsNullOrWhiteSpace(article.Title))
            {
                article.Title = await LoadTitleFromPageAsync(article.Url);
            }

            article.Content = string.IsNullOrWhiteSpace(item.Description) ? FixContent(item.Content) : FixContent(item.Description);

            articles.Add(article);
        }

        return articles;
    }

    private async Task<string> LoadTitleFromPageAsync(string url)
    {
        var result = string.Empty;

        if (string.IsNullOrWhiteSpace(url))
        {
            LogMessage.ReaderInvalidUrl(_logger, url);
            return result;
        }

        try
        {
            using HttpClient httpClient = _httpClientFactory.CreateClient();

            HttpResponseMessage response = await httpClient.GetAsync(url);
            var html = await response.Content.ReadAsStringAsync();

            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            HtmlNode? titleNode = doc.DocumentNode.SelectSingleNode("//head/title");
            result = titleNode?.InnerText ?? string.Empty;
        }
        catch (Exception e)
        {
            LogMessage.ReaderException(_logger, "Failed to load title from article URL.", e);
        }

        return result;
    }

    private static string GetArticleLink(FeedItem item)
    {
        if (item.SpecificItem is AtomFeedItem atomItem)
        {
            AtomLink? link = atomItem.Links.Where(a => a.LinkType == "text/html" && a.Relation == "alternate").FirstOrDefault();

            return link != null ? link.Href : item.Link;
        }
        else
        {
            return item.Link;
        }
    }

    private string FixContent(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return string.Empty;
        }

        var tmp = System.Net.WebUtility.HtmlDecode(content);

        return _sanitizer.Sanitize(tmp);
    }

    private static bool FeedHashNotChanged(byte[] hash1, byte[] hash2)
    {
        for (var i = 0; i < 20; i++)
        {
            if (hash1[i] != hash2[i])
            {
                return false;
            }
        }

        return true;
    }
}
