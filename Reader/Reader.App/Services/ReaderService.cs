using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Reader.App.Services;

public class ReaderService : IReaderService
{
    private readonly HttpClient _httpClient;

    private UnreadGroup[] _unreadData = Array.Empty<UnreadGroup>();

    public ReaderService(HttpClient httpClient) => _httpClient = httpClient;

    public async Task<UnreadGroup[]> UnreadArticles()
    {
        _unreadData = await _httpClient.GetFromJsonAsync<UnreadGroup[]>("/api/unread") ?? Array.Empty<UnreadGroup>();

        return _unreadData;
    }
}
