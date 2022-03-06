using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Text.Json;

namespace PeterPedia.Client.Services;

public class TVService
{
    private static readonly JsonSerializerOptions s_Options = new(JsonSerializerDefaults.Web);
    private static readonly PeterPediaJSONContext s_Context = new(s_Options);

    private readonly HttpClient _http;
    private readonly IToastService _toast;

    public TVService(HttpClient httpClient, IToastService toastService)
    {
        _http = httpClient;
        _toast = toastService;
    }

    public List<Show> Shows { get; private set; } = null!;

    public event Action? RefreshRequested;

    public void CallRequestRefresh() => RefreshRequested?.Invoke();

    public async Task FetchDataAsync()
    {
        if (Shows is null)
        {
            Show[]? shows = await _http.GetFromJsonAsync("/api/TV", s_Context.ShowArray);

            if (shows is not null)
            {
                Shows = new List<Show>(shows.Length);
                Shows.AddRange(shows);
            }
        }
    }

    public async Task<Show?> GetAsync(int id)
    {
        if (Shows is null)
        {
            await FetchDataAsync();
        }

        return Shows is not null ? Shows.Where(s => s.Id == id).FirstOrDefault() : null;
    }

    public async Task<bool> AddAsync(string url)
    {
        var showRegex = new Regex("^https://www.themoviedb.org/tv/(\\d+)");

        if (!int.TryParse(url, out int id))
        {
            if (showRegex.IsMatch(url))
            {
                var matches = showRegex.Match(url);

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

        using HttpResponseMessage response = await _http.PostAsJsonAsync("/api/TV", postBody, s_Context.AddShow);

        if (response.IsSuccessStatusCode)
        {
            Show? show = await response.Content.ReadFromJsonAsync(s_Context.Show);
            if (show is not null)
            {
                Shows.Add(show);

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
        Show? show = await GetAsync(id);
        if (show is null)
        {
            _toast.ShowError($"{id} is not a valid show id. Can't remove show.");
            return false;
        }

        using var response = await _http.DeleteAsync($"/api/TV/{id}");

        if (response.IsSuccessStatusCode)
        {
            _toast.ShowSuccess($"Show {show.Title} deleted");

            Shows.Remove(show);

            return true;
        }
        else
        {
            _toast.ShowError($"Failed to delete show. StatusCode = {response.StatusCode}");

            return false;
        }
    }

    public async Task<bool> UpdateAsync(Show show)
    {
        if (show is null)
        {
            _toast.ShowError("Invalid show, can't update");
            return false;
        }

        var existingShow = await GetAsync(show.Id);
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
                var index = Shows.IndexOf(existingShow);
                Shows[index] = serverShow;

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

    public async Task<bool> WatchSeasonAsync(int showId, int seasonId)
    {
        var show = await GetAsync(showId);
        if (show is null)
        {
            return false;
        }

        var data = new ShowWatchData()
        {
            EpisodeId = null,
            SeasonId = seasonId,
            Watched = true,
        };

        using HttpResponseMessage response = await _http.PostAsJsonAsync("/api/TV/watch", data, s_Context.ShowWatchData);
        if (response.IsSuccessStatusCode)
        {
            foreach (Season season in show.Seasons)
            {
                if (season.Id == seasonId)
                {
                    foreach (Episode episode in season.Episodes)
                    {
                        episode.Watched = true;
                    }

                    show.Calculate();

                    _toast.ShowSuccess($"{show.Title} - Season {season.SeasonNumber} watched.");

                    return true;
                }
            }
        }
        else
        {
            _toast.ShowError($"Failed to set season as watched. StatusCode = {response.StatusCode}");
        }

        return false;
    }

    public async Task<bool> UnwatchSeasonAsync(int showId, int seasonId)
    {
        Show? show = await GetAsync(showId);
        if (show is null)
        {
            return false;
        }

        var data = new ShowWatchData()
        {
            EpisodeId = null,
            SeasonId = seasonId,
            Watched = false,
        };

        using HttpResponseMessage response = await _http.PostAsJsonAsync("/api/TV/watch", data, s_Context.ShowWatchData);
        if (response.IsSuccessStatusCode)
        {
            foreach (var season in show.Seasons)
            {
                if (season.Id == seasonId)
                {
                    foreach (var episode in season.Episodes)
                    {
                        episode.Watched = false;
                    }

                    show.Calculate();

                    _toast.ShowSuccess($"{show.Title} - Season {season.SeasonNumber} unwatched.");

                    return true;
                }
            }
        }
        else
        {
            _toast.ShowError($"Failed to set season as unwatched. StatusCode = {response.StatusCode}");
        }

        return false;
    }

    public async Task<bool> WatchEpisodeAsync(int showId, int episodeId)
    {
        Show? show = await GetAsync(showId);
        if (show is null)
        {
            return false;
        }

        var data = new ShowWatchData()
        {
            EpisodeId = episodeId,
            SeasonId = null,
            Watched = true,
        };

        using HttpResponseMessage response = await _http.PostAsJsonAsync("/api/tv/watch", data, s_Context.ShowWatchData);
        if (response.IsSuccessStatusCode)
        {
            foreach (Season season in show.Seasons)
            {
                foreach (Episode episode in season.Episodes)
                {
                    if (episode.Id == episodeId)
                    {
                        episode.Watched = true;

                        show.Calculate();

                        _toast.ShowSuccess($"{show.Title} - S{season.SeasonNumber}E{episode.EpisodeNumber} watched.");

                        return true;
                    }
                }
            }
        }
        else
        {
            _toast.ShowError($"Failed to set episode as watched. StatusCode = {response.StatusCode}");
        }

        return false;
    }

    public async Task<bool> UnwatchEpisodeAsync(int showId, int episodeId)
    {
        Show? show = await GetAsync(showId);
        if (show is null)
        {
            return false;
        }

        var data = new ShowWatchData()
        {
            EpisodeId = episodeId,
            SeasonId = null,
            Watched = false,
        };

        using HttpResponseMessage response = await _http.PostAsJsonAsync("/api/TV/watch", data, s_Context.ShowWatchData);
        if (response.IsSuccessStatusCode)
        {
            foreach (Season season in show.Seasons)
            {
                foreach (Episode episode in season.Episodes)
                {
                    if (episode.Id == episodeId)
                    {
                        episode.Watched = false;

                        show.Calculate();

                        _toast.ShowSuccess($"{show.Title} - S{season.SeasonNumber}E{episode.EpisodeNumber} unwatched.");

                        return true;
                    }
                }
            }
        }
        else
        {
            _toast.ShowError($"Failed to set episode as unwatched. StatusCode = {response.StatusCode}");
        }

        return false;
    }
}
