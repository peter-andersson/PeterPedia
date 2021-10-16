using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CodeHollow.FeedReader.Feeds;
using System.Xml;
using System.Collections.Generic;
using CodeHollow.FeedReader;
using Ganss.XSS;
using System.Text.Json;
using System.Security.Cryptography;
using System.Text;
using PeterPedia.Server.Data;
using PeterPedia.Server.Data.Models;
using System.Net.Http;
using HtmlAgilityPack;

namespace PeterPedia.Server.Services
{
    internal class RefreshArticleService
    {
        private readonly ILogger<RefreshArticleService> _logger;
        private readonly PeterPediaContext _dbContext;
        private readonly HtmlSanitizer _sanitizer;
        private readonly IHttpClientFactory _httpClientFactory;

        public RefreshArticleService(ILogger<RefreshArticleService> logger, PeterPediaContext dbContext, IHttpClientFactory httpClientFactory)
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

        public async Task DoWork(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Execute();

                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        private async Task Execute()
        {
            try
            {
                _logger.LogInformation("RefreshArticleService - Execute");

                var subscriptions = await _dbContext.Subscriptions
                    .Include(s => s.Articles)
                    .AsSplitQuery()
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
                _logger.LogError(e, "Exception in RefreshArticleService.Execute");
            }
        }

        private async Task UpdateSubscription(SubscriptionEF subscription)
        {
            if (subscription is null)
            {
                throw new ArgumentNullException(nameof(subscription));
            }

            _logger.LogInformation("UpdateSubscription - {0}", subscription.Title);

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
                    _logger.LogInformation("Feed has not changed");
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
                        _logger.LogInformation($"Adding new article {article.Title} to subscription {subscription.Title}");
                        subscription.Articles.Add(article);
                    }
                }
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, $"Exception when fetching feed data for subscription {subscription.Title}");
            }
            catch (XmlException x)
            {
                _logger.LogError(x, $"Exception when fetching feed data for subscription {subscription.Title}");
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
                _logger.LogError($"'{nameof(url)}' cannot be null or whitespace.", nameof(url));
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
                _logger.LogError(e, "Failed to load title from article URL.");
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
    }
}