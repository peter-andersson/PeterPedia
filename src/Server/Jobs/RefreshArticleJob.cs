using Microsoft.EntityFrameworkCore;
using CodeHollow.FeedReader.Feeds;
using System.Xml;
using CodeHollow.FeedReader;
using Ganss.XSS;
using System.Text.Json;
using System.Security.Cryptography;
using System.Text;
using PeterPedia.Server.Data;
using PeterPedia.Server.Data.Models;
using HtmlAgilityPack;
using Quartz;

namespace PeterPedia.Server.Jobs;

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
            LogExecute();

            var subscriptions = await _dbContext.Subscriptions
                .Include(s => s.Articles)
                .AsSplitQuery()
                .AsTracking()
                .ToListAsync().ConfigureAwait(false);

            int updateCount = 0;
            foreach (var subscription in subscriptions)
            {
                if (updateCount >= 10)
                {
                    break;
                }

                if (subscription.LastUpdate.AddMinutes(subscription.UpdateIntervalMinute) < DateTime.UtcNow)
                {
                    await UpdateSubscription(subscription).ConfigureAwait(false);

                    subscription.LastUpdate = DateTime.UtcNow;

                    updateCount += 1;
                }
            }

            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            LogException("Exception in RefreshArticleService.Execute", e);
        }
    }

    private async Task UpdateSubscription(SubscriptionEF subscription)
    {
        if (subscription is null)
        {
            throw new ArgumentNullException(nameof(subscription));
        }

        LogUpdate(subscription.Title);

        try
        {
            var data = await FeedReader.ReadAsync(subscription.Url).ConfigureAwait(false);
            if (data is null)
            {
                return;
            }

            byte[] hash = CalculateHash(data.Items);

            if (FeedHashNotChanged(hash, subscription.Hash))
            {
                LogNotChanged();
                return;
            }

            subscription.Hash = hash;

            var articles = await ConvertFeedItems(data.Items);

            DateTime ageLimit = DateTime.UtcNow.AddDays(-30);
            foreach (var article in articles)
            {
                if (article.PublishDate < ageLimit)
                {
                    continue;
                }

                var existingArticle = subscription.Articles.Where(a => a.Url == article.Url).FirstOrDefault();

                if (existingArticle == null)
                {
                    LogAddArticle(article.Title, subscription.Title);
                    subscription.Articles.Add(article);
                }
            }
        }
        catch (TaskCanceledException ex)
        {
            LogFeedException(subscription.Title, ex);
        }
        catch (XmlException x)
        {
            LogFeedException(subscription.Title, x);
        }
    }

    private static byte[] CalculateHash(ICollection<FeedItem> items)
    {
        if (items is null)
        {
            throw new ArgumentNullException(nameof(items));
        }

        var tmp = new List<string>(items.Count * 2);

        foreach (var item in items)
        {
            tmp.Add(item.Content);
            tmp.Add(item.Link);
        }

        string json = JsonSerializer.Serialize(tmp);
        return GetSha256Hash(json);
    }

    private static byte[] GetSha256Hash(string input)
    {
        using SHA256 shaHash = SHA256.Create();

        return shaHash.ComputeHash(Encoding.UTF8.GetBytes(input));
    }

    private async Task<IEnumerable<ArticleEF>> ConvertFeedItems(ICollection<FeedItem> items)
    {
        if (items is null)
        {
            throw new ArgumentNullException(nameof(items));
        }

        var articles = new List<ArticleEF>(items.Count);

        foreach (var item in items)
        {
            var article = new ArticleEF()
            {
                Title = item.Title,
                Url = GetArticleLink(item),
                PublishDate = item.PublishingDate ?? DateTime.UtcNow,
            };

            if (string.IsNullOrWhiteSpace(article.Title))
            {
                article.Title = await LoadTitleFromPage(article.Url);
            }

            if (string.IsNullOrWhiteSpace(item.Description))
            {
                article.Content = FixContent(item.Content);
            }
            else
            {
                article.Content = FixContent(item.Description);
            }

            articles.Add(article);
        }

        return articles;
    }

    private async Task<string> LoadTitleFromPage(string url)
    {
        string result = string.Empty;

        if (string.IsNullOrWhiteSpace(url))
        {
            LogInvalidUrl(url);
            return result;
        }

        try
        {
            using var httpClient = _httpClientFactory.CreateClient();

            HttpResponseMessage response = await httpClient.GetAsync(url);
            string html = await response.Content.ReadAsStringAsync();

            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var titleNode = doc.DocumentNode.SelectSingleNode("//head/title");
            result = titleNode.InnerText;
        }
        catch (Exception e)
        {
            LogException("Failed to load title from article URL.", e);
        }

        return result;
    }

    private static string GetArticleLink(FeedItem item)
    {
        if (item.SpecificItem is AtomFeedItem atomItem)
        {
            var link = atomItem.Links.Where(a => a.LinkType == "text/html" && a.Relation == "alternate").FirstOrDefault();

            if (link != null)
            {
                return link.Href;
            }
            else
            {
                return item.Link;
            }
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

        string tmp = System.Net.WebUtility.HtmlDecode(content);

        return _sanitizer.Sanitize(tmp);
    }

    private static bool FeedHashNotChanged(byte[] hash1, byte[] hash2)
    {
        for (int i = 0; i < 20; i++)
        {
            if (hash1[i] != hash2[i])
            {
                return false;
            }
        }

        return true;
    }

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable CA1822 // Mark members as static
#pragma warning disable IDE0060 // Remove unused parameter
    [LoggerMessage(0, LogLevel.Error, "{message}")]
    partial void LogException(string message, Exception ex);

    [LoggerMessage(1, LogLevel.Error, "Exception when fetching feed data for subscription {title}")]
    partial void LogFeedException(string title, Exception ex);

    [LoggerMessage(2, LogLevel.Error, "'{url}' is not a valid url.")]
    partial void LogInvalidUrl(string url);

    [LoggerMessage(3, LogLevel.Debug, "RefreshArticleJob - Execute")]
    partial void LogExecute();

    [LoggerMessage(4, LogLevel.Information, "UpdateSubscription - {title}")]
    partial void LogUpdate(string title);

    [LoggerMessage(5, LogLevel.Information, "Feed has not changed.")]
    partial void LogNotChanged();

    [LoggerMessage(6, LogLevel.Information, "Adding new article {article} to subscription {subscription}")]
    partial void LogAddArticle(string article, string subscription);
#pragma warning restore CA1822 // Mark members as static
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore IDE0079 // Remove unnecessary suppression
}