using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CodeHollow.FeedReader;
using System.Net;
using CodeHollow.FeedReader.Feeds;
using PeterPedia.Server.Data;
using PeterPedia.Shared;
using PeterPedia.Server.Data.Models;

namespace PeterPedia.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public partial class SubscriptionController : Controller
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Used by source generator [LoggerMessaage]")]
    private readonly ILogger<SubscriptionController> _logger;

    private readonly PeterPediaContext _dbContext;

    public SubscriptionController(ILogger<SubscriptionController> logger, PeterPediaContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var subscriptions = await _dbContext.Subscriptions.ToListAsync().ConfigureAwait(false);

        var result = new List<Subscription>(subscriptions.Count);
        foreach (var subscription in subscriptions)
        {
            result.Add(ConvertToSubscription(subscription));
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Subscription subscription)
    {
        if (subscription is null)
        {
            return BadRequest();
        }

        LogAddSubscription(subscription.Url);

        var existingSubscription = await _dbContext.Subscriptions.Where(s => s.Url == subscription.Url).SingleOrDefaultAsync().ConfigureAwait(false);
        if (existingSubscription != null)
        {
            LogSubscriptionAlreadyExists();
            return Conflict();
        }

        var feedLinks = await FeedReader.GetFeedUrlsFromUrlAsync(subscription.Url).ConfigureAwait(false);

        string feedLink;
        if (!feedLinks.Any())
        {
            // no url - probably the url is already the right feed url
            LogNoFeedsFound();
            feedLink = subscription.Url.ToString();
        }
        else if (feedLinks.Count() == 1)
        {
            feedLink = feedLinks.First().Url;
            LogFoundOneFeed(feedLink);
        }
        else if (feedLinks.Count() == 2)
        {
            // if 2 urls, then its usually a feed and a comments feed, so take the first per default
            feedLink = feedLinks.First().Url;
            LogFoundTwoFeeds(feedLink);
        }
        else
        {
            LogFoundManyFeeds();

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
            Group = null,
            Url = subscription.Url,
            UpdateIntervalMinute = GetUpdateInterval(data),
            LastUpdate = DateTime.UtcNow.AddYears(-1), // Make sure it gets updated in next refresh
            Hash = new byte[32],
        };

        _dbContext.Subscriptions.Add(subscriptionEF);
        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        LogSubscriptionAdded(subscriptionEF.Id);

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
                LogNoFeedsFound();
                feedLink = subscription.Url.ToString();
            }
            else if (feedLinks.Count() == 1)
            {
                feedLink = feedLinks.First().Url;
                LogFoundOneFeed(feedLink);
            }
            else if (feedLinks.Count() == 2)
            {
                // if 2 urls, then its usually a feed and a comments feed, so take the first per default
                feedLink = feedLinks.First().Url;
                LogFoundTwoFeeds(feedLink);
            }
            else
            {
                LogFoundManyFeeds();
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
        if (string.IsNullOrWhiteSpace(subscription.Group))
        {
            existingSubscription.Group = null;
        }
        else
        {
            existingSubscription.Group = subscription.Group;
        }        

        _dbContext.Subscriptions.Update(existingSubscription);
        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        LogSubscriptionUpdated(existingSubscription.Id);

        return Ok();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (id <= 0)
        {
            return BadRequest();
        }

        var subscription = await _dbContext.Subscriptions.Where(s => s.Id == id).AsTracking().SingleOrDefaultAsync().ConfigureAwait(false);

        if (subscription is null)
        {
            return NotFound();
        }

        _dbContext.Subscriptions.Remove(subscription);
        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        LogSubscriptionRemoved(subscription.Id);

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

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable CA1822 // Mark members as static
#pragma warning disable IDE0060 // Remove unused parameter
    [LoggerMessage(0, LogLevel.Information, "Add subscription with url {url}.")]
    partial void LogAddSubscription(string url);

    [LoggerMessage(1, LogLevel.Information, "Subscription with link already exists.")]
    partial void LogSubscriptionAlreadyExists();

    [LoggerMessage(2, LogLevel.Information, "No feed urls from link. Probably the right link already.")]
    partial void LogNoFeedsFound();

    [LoggerMessage(3, LogLevel.Information, "Only one url found, selecting {link}.")]
    partial void LogFoundOneFeed(string link);

    [LoggerMessage(4, LogLevel.Information, "Two urls found, selecting {link}.")]
    partial void LogFoundTwoFeeds(string link);

    [LoggerMessage(5, LogLevel.Error, "Found many urls.")]
    partial void LogFoundManyFeeds();

    [LoggerMessage(6, LogLevel.Information, "Subscription addded with id {subscriptionId}.")]
    partial void LogSubscriptionAdded(int subscriptionId);

    [LoggerMessage(7, LogLevel.Information, "Subscription with id {subscriptionId} updated.")]
    partial void LogSubscriptionUpdated(int subscriptionId);

    [LoggerMessage(8, LogLevel.Information, "Subscription with id {subscriptionId} removed.")]
    partial void LogSubscriptionRemoved(int subscriptionId);
#pragma warning restore CA1822 // Mark members as static
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore IDE0079 // Remove unnecessary suppression
}