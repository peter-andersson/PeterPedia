using System.Net.Http.Json;
using System.Text.Json;

namespace Reader.App.Services;

public class ReaderService : IReaderService
{
    private readonly HttpClient _httpClient;

    private UnreadGroup[] _unreadData = Array.Empty<UnreadGroup>();

    public ReaderService(HttpClient httpClient) => _httpClient = httpClient;

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

    public UnreadGroup? GetUnreadGroupAsync(string group) => _unreadData.Where(u => u.Group == group).FirstOrDefault();

    public async Task<UnreadGroup[]> UnreadArticlesAsync()
    {
        _unreadData = await _httpClient.GetFromJsonAsync<UnreadGroup[]>("/api/unread") ?? Array.Empty<UnreadGroup>();

        return _unreadData;
    }
}
