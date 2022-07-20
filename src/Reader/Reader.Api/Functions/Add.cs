using CodeHollow.FeedReader;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;

namespace Reader.Api.Functions;

public class Add
{
    private readonly ILogger<Add> _log;
    private readonly IRepository _repository;

    public Add(ILogger<Add> log, IRepository repository)
    {
        _log = log;
        _repository = repository;
    }

    [FunctionName("Add")]    
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "add")] HttpRequest req,
        CancellationToken _)
    {
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();

        if (string.IsNullOrWhiteSpace(requestBody))
        {
            return req.BadRequest("Missing request body");
        }

        NewSubscription newSubscription = JsonConvert.DeserializeObject<NewSubscription>(requestBody);

        if (string.IsNullOrWhiteSpace(newSubscription.Url))
        {
            return req.BadRequest("Missing url on subscription");
        }

        IEnumerable<HtmlFeedLink> feedLinks = await FeedReader.GetFeedUrlsFromUrlAsync(newSubscription.Url);

        string feedLink;
        if (!feedLinks.Any())
        {
            // no url - probably the url is already the right feed url
            feedLink = newSubscription.Url;
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
            var urls = new List<string>();

            foreach (HtmlFeedLink link in feedLinks)
            {
                urls.Add(link.Url);
            }

            return req.Ok(urls);
        }

        Feed? data = await FeedReader.ReadAsync(feedLink);
        if (data is null)
        {
            _log.LogError("Failed to read any feed data from url {url}.", feedLink);
            return req.InternalServerError();
        }

        var subscription = new SubscriptionEntity
        {
            Id = Guid.NewGuid().ToString(),
            Title = data.Title,
            Group = null,
            Url = feedLink,
            UpdateIntervalMinute = 60,
            NextUpdate = DateTime.UtcNow.AddYears(-1), // Make sure it gets updated in next refresh
            Hash = null,
        };

        try
        {
            await _repository.AddAsync(subscription);

            _log.LogInformation("Added subscription with id {id} and title {title}.", subscription.Id, subscription.Title);

            return req.Ok();
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Something went wrong.");
            return req.InternalServerError();
        }
    }
}
