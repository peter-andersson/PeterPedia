using System.Net.Http.Json;
using PeterPedia.Shared;
using Blazored.Toast.Services;
using System.Text.Json;

namespace PeterPedia.Client.Services;

public class RSSService
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
    private static readonly PeterPediaJSONContext Context = new(Options);

    private readonly HttpClient _http;
    private readonly IToastService _toast;

    public RSSService(HttpClient httpClient, IToastService toastService)
    {
        _http = httpClient;
        _toast = toastService;
    }

    public List<Subscription> Subscriptions { get; private set; } = null!;

    public async Task FetchData()
    {
        if (Subscriptions is null)
        {
            var subscriptions = await _http.GetFromJsonAsync("/api/Subscription", Context.SubscriptionArray);

            if (subscriptions is not null)
            {
                Subscriptions = new List<Subscription>(subscriptions.Length);
                Subscriptions.AddRange(subscriptions);
            }
        }
    }

    public async Task<List<Subscription>> GetUnread()
    {
        var unread = await _http.GetFromJsonAsync("/api/Article", Context.SubscriptionArray);

        if (unread is not null)
        {
            return unread.OrderBy(s => s.Title).ToList();
        }

        return new List<Subscription>();
    }

    public async Task<List<Article>> GetHistory()
    {
        var articles = await _http.GetFromJsonAsync("/api/Article/history", Context.ArticleArray);

        if (articles is not null)
        {
            return articles.ToList();
        }

        return new List<Article>();
    }

    public async Task<Subscription?> GetSubscription(int id)
    {
        if (Subscriptions is null)
        {
            await FetchData();
        }

        if (Subscriptions is not null)
        {
            return Subscriptions.Where(b => b.Id == id).FirstOrDefault();
        }

        return null;
    }

    public async Task<bool> AddSubscription(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            _toast.ShowError("Invalid url.");
            return false;
        }

        if (Subscriptions is null)
        {
            await FetchData();
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

        using var response = await _http.PostAsJsonAsync("/api/subscription", postBody, Context.Subscription);

        if (response.IsSuccessStatusCode)
        {
            _toast.ShowSuccess("Subscription added");

            var subscription = await response.Content.ReadFromJsonAsync(Context.Subscription);
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

    public async Task<bool> DeleteSubscription(int id)
    {
        var subscription = await GetSubscription(id);
        if (subscription is null)
        {
            _toast.ShowError($"{id} is not a valid subscription id. Can't remove subscription.");
            return false;
        }

        using var response = await _http.DeleteAsync($"/api/Subscription/{id}");

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

    public async Task<bool> UpdateSubscription(Subscription subscription)
    {
        if (subscription is null)
        {
            _toast.ShowError("Invalid subscription, can't update");
            return false;
        }

        var existingSubscription = await GetSubscription(subscription.Id);
        if (existingSubscription is null)
        {
            _toast.ShowError("Can't update a subscription that doesn't exist.");
            return false;
        }

        using var response = await _http.PutAsJsonAsync("/api/Subscription", subscription, Context.Subscription);

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

    public async Task<bool> DeleteArticle(int id)
    {
        if (id == 0)
        {
            return false;
        }

        using var response = await _http.GetAsync($"/api/article/read/{id}");

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

    public async Task<int> GetArticleCount()
    {
        return await _http.GetFromJsonAsync<int>("/api/Article/stats");
    }
}