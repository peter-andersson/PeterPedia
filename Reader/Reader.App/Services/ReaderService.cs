using System.Net.Http.Json;

namespace Reader.App.Services;

public class ReaderService : IReaderService
{
    private readonly HttpClient _httpClient;

    private UnreadGroup[] _unreadData = Array.Empty<UnreadGroup>();

    public ReaderService(HttpClient httpClient) => _httpClient = httpClient;

    public UnreadGroup? GetUnreadGroup(string group) => _unreadData.Where(u => u.Group == group).FirstOrDefault();

    public async Task<UnreadGroup[]> UnreadArticles()
    {
        _unreadData = await _httpClient.GetFromJsonAsync<UnreadGroup[]>("/api/unread") ?? Array.Empty<UnreadGroup>();

        return _unreadData;
    }
}
