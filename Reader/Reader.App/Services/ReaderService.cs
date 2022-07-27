using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

namespace Reader.App.Services;

public class ReaderService : IReaderService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;

    public ReaderService(HttpClient httpClient, IMemoryCache cache)
    {
        _httpClient = httpClient;
        _cache = cache;
    }

    public async Task<AddResult> AddAsync(NewSubscription subscription)
    {
        var result = new AddResult();

        if (string.IsNullOrWhiteSpace(subscription?.Url))
        {
            result.ErrorMessage = "No url specified";
            return result;
        }

        try
        {
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync("/api/add", subscription);

            switch (response.StatusCode)
            {
                case System.Net.HttpStatusCode.OK:
                    var contentString = await response.Content.ReadAsStringAsync();

                    if (!string.IsNullOrWhiteSpace(contentString))
                    {
                        List<string>? urls = JsonSerializer.Deserialize<List<string>>(contentString);

                        if (urls is not null)
                        {
                            result.Urls.AddRange(urls);
                        }
                    }

                    return result;

                case System.Net.HttpStatusCode.Conflict:
                    result.ErrorMessage = "Subscription with url already exists";
                    return result;

                case System.Net.HttpStatusCode.BadRequest:
                    result.ErrorMessage = "Invalid request to server";
                    return result;

                case System.Net.HttpStatusCode.InternalServerError:
                    result.ErrorMessage = "Something went wrong";
                    return result;
            }
        }
        catch (Exception e)
        {
            result.ErrorMessage = e.Message;            
        }

        return result;
    }

    public async Task<bool> Delete(Subscription subscription)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.DeleteAsync($"/api/delete/{subscription.Id}");

            return response.IsSuccessStatusCode;            
        }
        catch
        {
        }

        return false;
    }

    public async Task<HistoryArticle[]> GetHistoryAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<HistoryArticle[]>("/api/history") ?? Array.Empty<HistoryArticle>();
        }
        catch
        {
            return Array.Empty<HistoryArticle>();
        }
    }

    public async Task<Subscription?> GetSubscriptionAsync(string id)
    {
        Subscription[] subscriptions = await GetSubscriptionsAsync();

        return subscriptions.Where(s => s.Id == id).FirstOrDefault();
    }

    public async Task<Subscription[]> GetSubscriptionsAsync()
    {
        try
        {
            return await _cache.GetOrCreateAsync(
                "Subscriptions",
                async cacheEntry =>
                {
                    cacheEntry.SlidingExpiration = TimeSpan.FromMinutes(10);
                    cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(20);

                    return await _httpClient.GetFromJsonAsync<Subscription[]>("/api/all") ?? Array.Empty<Subscription>();
                });
        }
        catch
        {
            return Array.Empty<Subscription>();
        }
    }

    public async Task<UnreadGroup?> GetUnreadGroupAsync(string group)
    {
        UnreadGroup[] data = await UnreadArticlesAsync();

        return data.Where(u => u.Group == group).FirstOrDefault();
    }
    

    public async Task<UnreadGroup[]> UnreadArticlesAsync()
    {
        try
        {
            return await _cache.GetOrCreateAsync(
                "Unread",
                async cacheEntry =>
                {
                    cacheEntry.SlidingExpiration = TimeSpan.FromMinutes(10);
                    cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(20);

                    return await _httpClient.GetFromJsonAsync<UnreadGroup[]>("/api/unread") ?? Array.Empty<UnreadGroup>();
                });
        }
        catch
        {
            return Array.Empty<UnreadGroup>();
        }
    }

    public async Task<bool> UpdateSubscriptionAsync(Subscription subscription)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync("/api/update", subscription);

            return response.IsSuccessStatusCode;
        }
        catch
        {
        }

        return false;
    }
}
