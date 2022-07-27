using System.Net;
using System.Net.Http.Json;

namespace Episodes.App.Services;

public class TVService : ITVService
{
    private readonly HttpClient _httpClient;

    private TVShow[] _shows = Array.Empty<TVShow>();

    public TVService(HttpClient httpClient) => _httpClient = httpClient;

    public event Action? OnChange;

    public async Task<Result> AddAsync(int? id)
    {
        if (id is null)
        {
            return Result.Error("Missing id to add");            
        }

        try
        {
            HttpResponseMessage response = await _httpClient.PostAsync($"/api/add/{id}", null);

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    _shows = Array.Empty<TVShow>();
                    NotifyChanged();
                    return Result.Ok();
                case HttpStatusCode.Conflict:
                    return Result.Error("TV show already exists");
                case HttpStatusCode.InternalServerError:
                    return Result.Error("Failed to add TV show");
            }
        }
        catch (Exception e)
        {
            return Result.Error(e.Message);
        }

        return Result.Error("Unknown error");
    }

    public async Task<Result> DeleteAsync(TVShow show)
    {
        HttpResponseMessage response = await _httpClient.DeleteAsync($"/api/delete/{show.Id}");

        if (response.IsSuccessStatusCode)
        {
            NotifyChanged();
            return Result.Ok();
        }

        return Result.Error("Failed to delete tv show.");
    }

    public async Task<TVShow?> GetAsync(string id)
    {
        TVShow? show = _shows.Where(s => s.Id == id).FirstOrDefault();
        if (show is not null)
        {
            return show;
        }

        // 
        return await _httpClient.GetFromJsonAsync<TVShow>($"/api/get/{id}");
    }

    public async Task<TVShow[]> GetAsync(QueryData query)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync("/api/query", query);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<TVShow[]>() ?? Array.Empty<TVShow>();
            }
        }
        catch
        {
        }

        return Array.Empty<TVShow>();
    }

    public async Task<TVShow[]> GetWatchListAsync()
    {
        if (_shows.Length == 0)
        {
            _shows = await _httpClient.GetFromJsonAsync<TVShow[]>("/api/watchlist") ?? Array.Empty<TVShow>();
            _shows = _shows.Where(s => s.UnwatchedEpisodeCount > 0).ToArray();
        }

        return _shows;
    }

    public async Task<Result> UpdateAsync(TVShow show)
    {
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("/api/update", show);

        if (response.IsSuccessStatusCode)
        {
            NotifyChanged();
            return Result.Ok();
        }

        return Result.Error("Failed to update show");
    }

    private void NotifyChanged() => OnChange?.Invoke();
}

