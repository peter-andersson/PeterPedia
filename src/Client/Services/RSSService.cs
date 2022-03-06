using System.Net.Http.Json;
using System.Text.Json;

namespace PeterPedia.Client.Services;

public class RSSService
{
    private static readonly JsonSerializerOptions s_Options = new(JsonSerializerDefaults.Web);
    private static readonly PeterPediaJSONContext s_Context = new(s_Options);

    private readonly HttpClient _http;
    private readonly IToastService _toast;

    public RSSService(HttpClient httpClient, IToastService toastService)
    {
        _http = httpClient;
        _toast = toastService;
    }

    public List<Subscription> Subscriptions { get; private set; } = null!;

    public async Task FetchDataAsync()
    {
        if (Subscriptions is null)
        {
            Subscription[]? subscriptions = await _http.GetFromJsonAsync("/api/Subscription", s_Context.SubscriptionArray);

            if (subscriptions is not null)
            {
                Subscriptions = new List<Subscription>(subscriptions.Length);
                Subscriptions.AddRange(subscriptions);
            }
        }
    }

    public async Task<List<UnreadArticle>> GetUnreadAsync()
    {
        UnreadArticle[]? unread = await _http.GetFromJsonAsync("/api/Article", s_Context.UnreadArticleArray);

        return unread is not null ? unread.ToList() : new List<UnreadArticle>();
    }

    public async Task<List<Article>> GetHistoryAsync()
    {
        Article[]? articles = await _http.GetFromJsonAsync("/api/Article/history", s_Context.ArticleArray);

        return articles is not null ? articles.ToList() : new List<Article>();
    }

    public async Task<Subscription?> GetSubscriptionAsync(int id)
    {
        if (Subscriptions is null)
        {
            await FetchDataAsync();
        }

        return Subscriptions is not null ? Subscriptions.Where(b => b.Id == id).FirstOrDefault() : null;
    }

    public async Task<bool> AddSubscriptionAsync(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            _toast.ShowError("Invalid url.");
            return false;
        }

        if (Subscriptions is null)
        {
            await FetchDataAsync();
        }

        if (Subscriptions is null)
        {
            Subscriptions = new List<Subscription>();
        }

        var postBody = new Subscription()
        {
            Url = url,
            Title = string.Empty,
        };

        using HttpResponseMessage response = await _http.PostAsJsonAsync("/api/subscription", postBody, s_Context.Subscription);

        if (response.IsSuccessStatusCode)
        {
            _toast.ShowSuccess("Subscription added");

            var subscription = await response.Content.ReadFromJsonAsync(s_Context.Subscription);
            if (subscription is not null)
            {
                Subscriptions.Add(subscription);

                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            _toast.ShowError($"Failed to add subscription. StatusCode = {response.StatusCode}");

            return false;
        }
    }

    public async Task<bool> DeleteSubscriptionAsync(int id)
    {
        var subscription = await GetSubscriptionAsync(id);
        if (subscription is null)
        {
            _toast.ShowError($"{id} is not a valid subscription id. Can't remove subscription.");
            return false;
        }

        using HttpResponseMessage response = await _http.DeleteAsync($"/api/Subscription/{id}");

        if (response.IsSuccessStatusCode)
        {
            _toast.ShowSuccess("Subscription deleted");

            Subscriptions.Remove(subscription);

            return true;
        }
        else
        {
            _toast.ShowError($"Failed to delete subscription. StatusCode = {response.StatusCode}");

            return false;
        }
    }

    public async Task<bool> UpdateSubscriptionAsync(Subscription subscription)
    {
        if (subscription is null)
        {
            _toast.ShowError("Invalid subscription, can't update");
            return false;
        }

        var existingSubscription = await GetSubscriptionAsync(subscription.Id);
        if (existingSubscription is null)
        {
            _toast.ShowError("Can't update a subscription that doesn't exist.");
            return false;
        }

        using HttpResponseMessage response = await _http.PutAsJsonAsync("/api/Subscription", subscription, s_Context.Subscription);

        if (response.IsSuccessStatusCode)
        {
            _toast.ShowSuccess("Subscription saved");

            existingSubscription.Title = subscription.Title;
            existingSubscription.UpdateIntervalMinute = subscription.UpdateIntervalMinute;

            return true;
        }
        else
        {
            _toast.ShowError($"Failed to save movie. StatusCode = {response.StatusCode}");
            return false;
        }
    }

    public async Task<bool> DeleteArticleAsync(int id)
    {
        if (id == 0)
        {
            return false;
        }

        using HttpResponseMessage response = await _http.GetAsync($"/api/article/read/{id}");

        if (response.IsSuccessStatusCode)
        {
            return true;
        }
        else
        {
            _toast.ShowError("Failed to delete article.");
            return false;
        }
    }
}
