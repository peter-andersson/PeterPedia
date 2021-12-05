using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using CodeHollow.FeedReader;
using System.Net;
using CodeHollow.FeedReader.Feeds;
using PeterPedia.Server.Data;
using PeterPedia.Shared;
using PeterPedia.Server.Data.Models;

namespace PeterPedia.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubscriptionController : Controller
    {
        private readonly ILogger<SubscriptionController> _logger;
        private readonly PeterPediaContext _dbContext;

        public SubscriptionController(ILogger<SubscriptionController> logger, PeterPediaContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        [HttpGet("{id:int?}")]
        public async Task<IActionResult> Get(int? id)
        {
            _logger.LogDebug($"Get subscription id: {id}");
            if (id.GetValueOrDefault(0) > 0)
            {
                var subscriptionEF = await _dbContext.Subscriptions.Where(s => s.Id == id).SingleOrDefaultAsync().ConfigureAwait(false);

                if (subscriptionEF is null)
                {
                    return NotFound();
                }

                return Ok(ConvertToSubscription(subscriptionEF));
            }
            else
            {
                var subscriptions = await _dbContext.Subscriptions.ToListAsync().ConfigureAwait(false);

                var result = new List<Subscription>(subscriptions.Count);
                foreach (var subscription in subscriptions)
                {
                    result.Add(ConvertToSubscription(subscription));
                }

                return Ok(result);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Subscription subscription)
        {
            if (subscription is null)
            {
                return BadRequest();
            }

            _logger.LogDebug($"Adding subscription for link {subscription.Url}");

            var existingSubscription = await _dbContext.Subscriptions.Where(s => s.Url == subscription.Url).SingleOrDefaultAsync().ConfigureAwait(false);
            if (existingSubscription != null)
            {
                _logger.LogDebug("Subscription with link already exists");
                return Conflict();
            }

            var feedLinks = await FeedReader.GetFeedUrlsFromUrlAsync(subscription.Url).ConfigureAwait(false);

            string feedLink;
            if (!feedLinks.Any())
            {
                // no url - probably the url is already the right feed url
                _logger.LogDebug("No feed urls from link. Probably the right link already.");
                feedLink = subscription.Url.ToString();
            }
            else if (feedLinks.Count() == 1)
            {
                feedLink = feedLinks.First().Url;
                _logger.LogDebug($"Only one url found, selecting {feedLink}");
            }
            else if (feedLinks.Count() == 2)
            {
                // if 2 urls, then its usually a feed and a comments feed, so take the first per default
                feedLink = feedLinks.First().Url;
                _logger.LogDebug($"Two urls found, selecting {feedLink}");
            }
            else
            {
                _logger.LogDebug("Found many urls");
                // TODO: Display all links to user to choose from.
                return BadRequest("Url wasn't a valid feed and there are multiple feeds to choose from.");
            }

            var data = await FeedReader.ReadAsync(feedLink).ConfigureAwait(false);
            if (data is null)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "Failed to fetch feed from server.");
            }

            var subscriptionEF = new SubscriptionEF
            {
                Title = data.Title,
                Url = subscription.Url,
                UpdateIntervalMinute = GetUpdateInterval(data),
                LastUpdate = DateTime.UtcNow.AddYears(-1), // Make sure it gets updated in next refresh
                Hash = new byte[32],
            };

            _dbContext.Subscriptions.Add(subscriptionEF);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
            _logger.LogDebug("Subscription addded");

            return Ok(ConvertToSubscription(subscriptionEF));
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] Subscription subscription)
        {
            if (subscription is null)
            {
                return BadRequest();
            }

            var existingSubscription = await _dbContext.Subscriptions.Where(s => s.Id == subscription.Id).AsTracking().SingleOrDefaultAsync().ConfigureAwait(false);
            if (existingSubscription is null)
            {
                return NotFound();
            }

            if (subscription.Url != existingSubscription.Url)
            {
                var feedLinks = await FeedReader.GetFeedUrlsFromUrlAsync(subscription.Url.ToString()).ConfigureAwait(false);

                string feedLink;
                if (!feedLinks.Any())
                {
                    // no url - probably the url is already the right feed url
                    _logger.LogDebug("No feed urls from link. Probably the right link already.");
                    feedLink = subscription.Url.ToString();
                }
                else if (feedLinks.Count() == 1)
                {
                    feedLink = feedLinks.First().Url;
                    _logger.LogDebug($"Only one url found, selecting {feedLink}");
                }
                else if (feedLinks.Count() == 2)
                {
                    // if 2 urls, then its usually a feed and a comments feed, so take the first per default
                    feedLink = feedLinks.First().Url;
                    _logger.LogDebug($"Two urls found, selecting {feedLink}");
                }
                else
                {
                    _logger.LogDebug("Found many urls");
                    // TODO: Display all links to user to choose from.
                    return BadRequest("Url wasn't a valid feed and there are multiple feeds to choose from.");
                }

                var data = await FeedReader.ReadAsync(feedLink).ConfigureAwait(false);
                if (data is null)
                {
                    return StatusCode((int)HttpStatusCode.InternalServerError, "Failed to fetch feed from server.");
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

            _dbContext.Subscriptions.Update(existingSubscription);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
            _logger.LogDebug("Subscription updated");

            return Ok();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogDebug($"Delete subscription with id {id}");
            if (id <= 0)
            {
                return BadRequest();
            }

            var subscription = await _dbContext.Subscriptions.Where(s => s.Id == id).AsTracking().SingleOrDefaultAsync().ConfigureAwait(false);

            if (subscription is null)
            {
                _logger.LogDebug("Subscription not found");
                return NotFound();
            }

            _dbContext.Subscriptions.Remove(subscription);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);

            _logger.LogDebug("Subscription removed");

            return Ok();
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
                if (int.TryParse(rss20.TTL, out int minutes))
                {
                    if (minutes < 60)
                    {
                        return 60;
                    }

                    return minutes;
                }
            }

            return 60;
        }
    }
}
