using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Text.Json;
using Microsoft.JSInterop;

namespace PeterPedia.Client.Services;

public class EpisodeManager : IEpisodeManager
{
    private static readonly JsonSerializerOptions s_Options = new(JsonSerializerDefaults.Web);
    private static readonly PeterPediaJSONContext s_Context = new(s_Options);

    private readonly HttpClient _http;
    private readonly IToastService _toast;
    private readonly IJSRuntime _js;

    private readonly List<Show> _shows = new();

    public EpisodeManager(HttpClient httpClient, IToastService toastService, IJSRuntime js)
    {
        _http = httpClient;
        _toast = toastService;
        _js = js;
    }

    public event Action? EpisodeChanged;

    public async Task<bool> AddAsync(string url)
    {
        var showRegex = new Regex("^https://www.themoviedb.org/tv/(\\d+)");

        if (!int.TryParse(url, out var id))
        {
            if (showRegex.IsMatch(url))
            {
                Match matches = showRegex.Match(url);

                if (!int.TryParse(matches.Groups[1].Value, out id))
                {
                    _toast.ShowError("Can't add show, invalid show id.");
                }
            }
        }

        if (id == 0)
        {
            _toast.ShowError("Can't add show, invalid show id.");
            return false;
        }

        var postBody = new AddShow()
        {
            Id = id,
        };

        using HttpResponseMessage response = await _http.PostAsJsonAsync("/api/Episode", postBody, s_Context.AddShow);

        if (response.IsSuccessStatusCode)
        {
            Show? show = await response.Content.ReadFromJsonAsync(s_Context.Show);
            if (show is not null)
            {
                _shows.Add(show);

                EpisodeChanged?.Invoke();

                await _js.InvokeVoidAsync("episodeStore.put", show);

                _toast.ShowSuccess($"Show {show.Title} added");

                return true;
            }
            else
            {
                _toast.ShowError("Failed to add show");

                return false;
            }
        }
        else
        {
            _toast.ShowError($"Failed to add movie. StatusCode = {response.StatusCode}");

            return false;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        Show? show = Get(id);
        if (show is null)
        {
            _toast.ShowError($"{id} is not a valid show id. Can't remove show.");
            return false;
        }

        using HttpResponseMessage response = await _http.DeleteAsync($"/api/Episode/{id}");

        if (response.IsSuccessStatusCode)
        {
            _toast.ShowSuccess($"Show {show.Title} deleted");

            _shows.Remove(show);

            EpisodeChanged?.Invoke();

            await _js.InvokeVoidAsync("episodeStore.delete", show.Id);

            return true;
        }
        else
        {
            _toast.ShowError($"Failed to delete show. StatusCode = {response.StatusCode}");

            return false;
        }
    }

    public async Task<List<Show>> GetAsync()
    {
        if (_shows.Count == 0)
        {
            Show[] shows = await _js.InvokeAsync<Show[]>("episodeStore.getAll");

            foreach (Show show in shows)
            {
                _shows.Add(show);
            }
        }

        if (_shows.Count == 0)
        {
            await RefreshAsync();
        }

        return _shows;
    }

    public async Task<List<Episode>> GetEpisodesAsync()
    {
        var json = await _http.GetStringAsync($"/api/Episode/episodes");

        var result = new List<Episode>();
        if (!string.IsNullOrWhiteSpace(json))
        {
            Episode[]? episodes = JsonSerializer.Deserialize(json, s_Context.EpisodeArray);

            if (episodes is not null)
            {
                foreach (Episode episode in episodes)
                {
                    result.Add(episode);
                }
            }
        }

        return result;
    }

    public async Task RefreshAsync()
    {
        if (await FetchChangedShowsAsync() || await FetchDeletedShowsAsync())
        {
            EpisodeChanged?.Invoke();
        }
    }

    public async Task<bool> UpdateAsync(Show show)
    {
        if (show is null)
        {
            _toast.ShowError("Invalid show, can't update");
            return false;
        }

        Show? existingShow = Get(show.Id);
        if (existingShow is null)
        {
            _toast.ShowError("Can't update a show that doesn't exist.");
            return false;
        }

        using HttpResponseMessage response = await _http.PutAsJsonAsync("/api/TV", show, s_Context.Show);

        if (response.IsSuccessStatusCode)
        {
            _toast.ShowSuccess($"Show {show.Title} saved");

            Show? serverShow = await _http.GetFromJsonAsync($"/api/TV/{show.Id}", s_Context.Show);

            if (serverShow is not null)
            {
                existingShow.Title = serverShow.Title;
                existingShow.Status = serverShow.Status;
                existingShow.Seasons = serverShow.Seasons;
                existingShow.Calculate();

                await _js.InvokeVoidAsync("episodeStore.put", existingShow);

                EpisodeChanged?.Invoke();

                return true;
            }

            return false;
        }
        else
        {
            _toast.ShowError($"Failed to save show. StatusCode = {response.StatusCode}");
            return false;
        }
    }

    public async Task<bool> WatchAsync(int showId, ShowWatchData data)
    {
        Show? show = Get(showId);
        if (show is null)
        {
            _toast.ShowError($"No show to update watch on.");
            return false;
        }

        using HttpResponseMessage response = await _http.PostAsJsonAsync("/api/Episode/watch", data, s_Context.ShowWatchData);
        if (response.IsSuccessStatusCode)
        {
            if (data.SeasonId.HasValue)
            {
                Season? season = show.Seasons.Where(s => s.Id == data.SeasonId).FirstOrDefault();

                if (season is null)
                {
                    _toast.ShowError($"Didn't find season to update watched on.");
                    return false;
                }

                foreach (Episode episode in season.Episodes)
                {
                    episode.Watched = data.Watched;
                }

                show.Calculate();

                _toast.ShowSuccess($"{show.Title} - Season {season.SeasonNumber} watched.");

                await _js.InvokeVoidAsync("episodeStore.put", show);

                EpisodeChanged?.Invoke();
            }
            else
            {
                foreach (Season season in show.Seasons)
                {
                    foreach (Episode episode in season.Episodes)
                    {
                        if (episode.Id == data.EpisodeId)
                        {
                            episode.Watched = data.Watched;

                            show.Calculate();

                            _toast.ShowSuccess($"{show.Title} - Season {season.SeasonNumber} watched.");

                            await _js.InvokeVoidAsync("episodeStore.put", show);

                            EpisodeChanged?.Invoke();

                            return true;
                        }
                    }
                }
            }
        }
        else
        {
            _toast.ShowError($"Failed to update watch state. StatusCode = {response.StatusCode}");
        }

        return false;
    }

    private Show? Get(int id) => _shows.Where(s => s.Id == id).FirstOrDefault();

    private async Task<bool> FetchChangedShowsAsync()
    {
        var changed = false;
        Show? mostRecentlyUpdated = await _js.InvokeAsync<Show>("episodeStore.getFirstFromIndex", "lastUpdate", "prev");
        DateTime since = mostRecentlyUpdated?.LastUpdate ?? DateTime.MinValue;
        var json = await _http.GetStringAsync($"/api/Episode?lastupdate={since:yyyyMMddHHmmss}");

        if (!string.IsNullOrWhiteSpace(json))
        {
            await _js.InvokeVoidAsync("episodeStore.putAllFromJson", json);

            Show[]? shows = JsonSerializer.Deserialize(json, s_Context.ShowArray);

            if (shows is not null)
            {
                foreach (Show show in shows)
                {
                    Show? existing = Get(show.Id);

                    if (existing is not null)
                    {
                        existing.Status = show.Status;
                        existing.Title = show.Title;
                        existing.LastUpdate = show.LastUpdate;
                        existing.Seasons = show.Seasons;
                        existing.Calculate();
                    }
                    else
                    {
                        _shows.Add(show);
                        changed = true;
                    }
                }
            }
        }

        return changed;
    }

    private async Task<bool> FetchDeletedShowsAsync()
    {
        var changed = false;
        DeleteLog? latestDeletion = await _js.InvokeAsync<DeleteLog>("episodeStore.getDeleted");
        DateTime since = latestDeletion?.Deleted ?? DateTime.MinValue;
        var json = await _http.GetStringAsync($"/api/Episode/deleted?deleted={since:yyyyMMddHHmmss}");
        if (!string.IsNullOrWhiteSpace(json))
        {
            DeleteLog[]? deletions = JsonSerializer.Deserialize(json, s_Context.DeleteLogArray);

            if (deletions is not null && deletions.Length > 0)
            {
                DeleteLog? maxDeleteTime = null;

                foreach (DeleteLog deletion in deletions)
                {
                    Show? existing = Get(deletion.Id);
                    if (existing is not null)
                    {
                        await _js.InvokeVoidAsync("episodeStore.delete", existing.Id);
                        _shows.Remove(existing);
                        changed = true;
                    }

                    if ((maxDeleteTime is null) || (maxDeleteTime.Deleted < deletion.Deleted))
                    {
                        maxDeleteTime = deletion;
                    }
                }

                if (maxDeleteTime is not null)
                {
                    await _js.InvokeVoidAsync("episodeStore.putDeleted", maxDeleteTime);
                }
            }
        }

        return changed;
    }
}
