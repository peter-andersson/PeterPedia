using System;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using CodeHollow.FeedReader;
using CodeHollow.FeedReader.Feeds;
using HtmlAgilityPack;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Scripts;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PeterPedia.Data.Interface;
using PeterPedia.Data.Models;

namespace PeterPedia.Functions.Functions;

public class UpdateSubscriptions
{
    private readonly ILogger<UpdateSubscriptions> _log;
    private readonly IDataStorage<SubscriptionEntity> _subscriptionStorage;
    private readonly IDataStorage<ArticleEntity> _articleStorage;
    private readonly IHttpClientFactory _httpClientFactory;


    public UpdateSubscriptions(ILogger<UpdateSubscriptions> log, IDataStorage<SubscriptionEntity> subscriptionStorage, IDataStorage<ArticleEntity> articleStorage, IHttpClientFactory httpClientFactory)
    {
        _log = log;
        _subscriptionStorage = subscriptionStorage;
        _articleStorage = articleStorage;
        _httpClientFactory = httpClientFactory;
    }

    [FunctionName("UpdateSubscriptions")]
    public async Task RunAsync([TimerTrigger("0 0 * * * *", RunOnStartup = true)] TimerInfo myTimer)
    {
        _log.LogDebug(myTimer.FormatNextOccurrences(1));

        var query = new QueryDefinition(query: "SELECT * FROM c WHERE c.Type = \"subscription\" AND c.NextUpdate < GetCurrentDateTime() ORDER BY c.NextUpdate OFFSET 0 LIMIT 20");

        List<SubscriptionEntity> entities = await _subscriptionStorage.QueryAsync(query);

        foreach (SubscriptionEntity subscription in entities)
        {
            await ReadFeedAsync(subscription);

            subscription.NextUpdate = DateTime.UtcNow.AddMinutes(subscription.UpdateIntervalMinute);

            await _subscriptionStorage.UpdateAsync(subscription);
        }
    }

    private async Task ReadFeedAsync(SubscriptionEntity subscription)
    {
        if (subscription is null)
        {
            throw new ArgumentNullException(nameof(subscription));
        }

        try
        {
            Feed? data = await FeedReader.ReadAsync(subscription.Url);
            if (data is null)
            {
                return;
            }

            var hash = CalculateHash(data.Items);

            if (string.Equals(subscription.Hash, hash))
            {
                _log.LogDebug("Feed hasn't changed");
                return;
            }

            subscription.Hash = hash;

            IEnumerable<ArticleEntity> articles = await ConvertFeedItemsAsync(data.Items, subscription);

            DateTime ageLimit = DateTime.UtcNow.AddDays(-30);
            foreach (ArticleEntity article in articles)
            {
                if (article.PublishDate < ageLimit)
                {
                    continue;
                }

                QueryDefinition query = new QueryDefinition(query: "SELECT * FROM c WHERE c.Type = \"article\" AND c.Url = @url")
                    .WithParameter("@url", article.Url);

                List<ArticleEntity> entities = await _articleStorage.QueryAsync(query);

                if (entities.Count == 0)
                {
                    await _articleStorage.AddAsync(article);
                }
            }
        }
        catch (TaskCanceledException ex)
        {
            _log.LogError(ex, "TaskCanceledException");
        }
        catch (XmlException x)
        {
            _log.LogError(x, "Invalid XML");
        }
    }

    private static string CalculateHash(ICollection<FeedItem> items)
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

        var json = JsonConvert.SerializeObject(tmp);
        return GetSha256Hash(json);
    }

    private static string GetSha256Hash(string input)
    {
        var stringBuilder = new StringBuilder();

        using var shaHash = SHA256.Create();

        var result = shaHash.ComputeHash(Encoding.UTF8.GetBytes(input));

        foreach (var b in result)
        {
            stringBuilder.Append(b.ToString("x2"));
        }

        return stringBuilder.ToString();
    }

    private async Task<IEnumerable<ArticleEntity>> ConvertFeedItemsAsync(ICollection<FeedItem> items, SubscriptionEntity subscription)
    {
        if (items is null)
        {
            throw new ArgumentNullException(nameof(items));
        }

        var articles = new List<ArticleEntity>(items.Count);

        foreach (FeedItem item in items)
        {
            var article = new ArticleEntity()
            {
                Id = Guid.NewGuid().ToString(),
                Title = item.Title,
                Url = GetArticleLink(item),
                PublishDate = item.PublishingDate ?? DateTime.UtcNow,
                Group = subscription.Group ?? subscription.Title,
                Subscription = subscription.Title,
                ReadDate = null,
            };

            if (string.IsNullOrWhiteSpace(article.Title))
            {
                article.Title = await LoadTitleFromPageAsync(article.Url);
            }

            articles.Add(article);
        }

        return articles;
    }

    private async Task<string> LoadTitleFromPageAsync(string url)
    {
        var result = string.Empty;

        if (string.IsNullOrWhiteSpace(url))
        {
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
            _log.LogError(e, "Failed to load title from article URL.");
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
}
