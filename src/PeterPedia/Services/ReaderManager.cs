using CodeHollow.FeedReader;
using CodeHollow.FeedReader.Feeds;
using Microsoft.EntityFrameworkCore;

namespace PeterPedia.Services
{
    public class ReaderManager : IReaderManager
    {
        private readonly ILogger<ReaderManager> _logger;
        private readonly PeterPediaContext _dbContext;

        public ReaderManager(ILogger<ReaderManager> logger, PeterPediaContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task<bool> AddSubscriptionAsync(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                // TODO: Log and better error message to client
                return false;
            }

            SubscriptionEF? existingSubscription = await _dbContext.Subscriptions.Where(s => s.Url == url).SingleOrDefaultAsync();
            if (existingSubscription != null)
            {
                // TODO: Log and better error message to client
                return false;
            }

            IEnumerable<HtmlFeedLink>? feedLinks = await FeedReader.GetFeedUrlsFromUrlAsync(url);

            string feedLink;
            if (!feedLinks.Any())
            {
                // TODO: Log and better error message to client
                // no url - probably the url is already the right feed url
                feedLink = url.ToString();
            }
            else if (feedLinks.Count() == 1)
            {
                // TODO: Log and better error message to client
                feedLink = feedLinks.First().Url;
            }
            else if (feedLinks.Count() == 2)
            {
                // TODO: Log and better error message to client
                // if 2 urls, then its usually a feed and a comments feed, so take the first per default
                feedLink = feedLinks.First().Url;
            }
            else
            {
                // TODO: Log and better error message to client
                return false;
            }

            Feed? data = await FeedReader.ReadAsync(feedLink);
            if (data is null)
            {
                // TODO: Log and better error message to client
                return false;
            }

            var subscriptionEF = new SubscriptionEF
            {
                Title = data.Title,
                Group = null,
                Url = feedLink,
                UpdateIntervalMinute = GetUpdateInterval(data),
                LastUpdate = DateTime.UtcNow.AddYears(-1), // Make sure it gets updated in next refresh
                Hash = new byte[32],
            };

            _dbContext.Subscriptions.Add(subscriptionEF);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);

            return true;
        }

        public async Task<bool> DeleteArticleAsync(int id)
        {
            ArticleEF? article = await _dbContext.Articles.Where(a => a.Id == id).AsTracking().SingleOrDefaultAsync();
            if (article is null)
            {
                return false;
            }

            LogMessage.ReaderReadArticle(_logger, article);

            article.ReadDate = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteSubscriptionAsync(int id)
        {
            if (id <= 0)
            {
                return false;
            }

            SubscriptionEF? subscription = await _dbContext.Subscriptions
                .Where(s => s.Id == id)
                .AsTracking()
                .SingleOrDefaultAsync();

            if (subscription is null)
            {
                return false;
            }

            _dbContext.Subscriptions.Remove(subscription);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<List<Subscription>> GetSubscriptionsAsync()
        {
            List<SubscriptionEF> subscriptions = await _dbContext.Subscriptions.OrderBy(s => s.Title).ToListAsync();

            var result = new List<Subscription>(subscriptions.Count);
            foreach (SubscriptionEF? subscription in subscriptions)
            {
                result.Add(ConvertToSubscription(subscription));
            }

            return result;
        }

        public async Task<List<Article>> GetHistoryAsync()
        {
            List<ArticleEF> articles =
                await _dbContext.Articles
                .Where(a => a.ReadDate != null)
                .OrderByDescending(a => a.ReadDate)
                .Take(100)
                .ToListAsync();

            var result = new List<Article>(articles.Count);
            foreach (ArticleEF article in articles)
            {
                result.Add(ConvertToArticle(article));
            }

            return result;
        }

        public async Task<Subscription?> GetSubscriptionAsync(int id)
        {
            SubscriptionEF? subscription = await _dbContext.Subscriptions.Where(s => s.Id == id).SingleOrDefaultAsync();

            return subscription is not null ? ConvertToSubscription(subscription) : null;
        }

        public async Task<List<UnreadArticle>> GetUnreadAsync()
        {
            List<SubscriptionEF> subscriptions =
                await _dbContext.Subscriptions.Include(s => s.Articles
                .Where(a => a.ReadDate == null)
                .OrderBy(a => a.PublishDate))
                .AsSplitQuery()
                .ToListAsync();

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

            return result;
        }

        public async Task<bool> UpdateSubscriptionAsync(Subscription subscription)
        {
            if (subscription is null)
            {
                return false;
            }

            SubscriptionEF? existingSubscription =
                await _dbContext.Subscriptions
                .Where(s => s.Id == subscription.Id)
                .AsTracking()
                .SingleOrDefaultAsync();
            if (existingSubscription is null)
            {
                return false;
            }

            if (subscription.Url != existingSubscription.Url)
            {
                IEnumerable<HtmlFeedLink>? feedLinks = await FeedReader.GetFeedUrlsFromUrlAsync(subscription.Url.ToString()).ConfigureAwait(false);

                string feedLink;
                if (!feedLinks.Any())
                {
                    // no url - probably the url is already the right feed url
                    feedLink = subscription.Url.ToString();
                }
                else if (feedLinks.Count() == 1)
                {
                    feedLink = feedLinks.First().Url;
                }
                else if (feedLinks.Count() == 2)
                {
                    // if 2 urls, then its usually a feed and a comments feed, so take the first per default
                    feedLink = feedLinks.First().Url;
                }
                else
                {
                    return false;
                }

                Feed? data = await FeedReader.ReadAsync(feedLink).ConfigureAwait(false);
                if (data is null)
                {
                    return false;
                }

                existingSubscription.Url = feedLink;

                existingSubscription.UpdateIntervalMinute = GetUpdateInterval(data);
                existingSubscription.LastUpdate = DateTime.UtcNow.AddYears(-1); // Make sure it gets updated in next refresh
            }
            else
            {
                existingSubscription.UpdateIntervalMinute = subscription.UpdateIntervalMinute;
            }

            existingSubscription.Title = subscription.Title;

            existingSubscription.Group = string.IsNullOrWhiteSpace(subscription.Group) ? null : subscription.Group;

            _dbContext.Subscriptions.Update(existingSubscription);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);

            return true;
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
                Group = subscriptionEF.Group,
                Url = subscriptionEF.Url,
                LastUpdate = subscriptionEF.LastUpdate,
                UpdateIntervalMinute = subscriptionEF.UpdateIntervalMinute,
            };

            return subscription;
        }

        private static int GetUpdateInterval(Feed data)
        {
            if (data is null)
            {
                return 60;
            }

            if (data.SpecificFeed is Rss20Feed rss20)
            {
                if (int.TryParse(rss20.TTL, out var minutes))
                {
                    return minutes < 60 ? 60 : minutes;
                }
            }

            return 60;
        }
    }
}
