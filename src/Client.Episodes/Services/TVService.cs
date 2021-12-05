using System.Net.Http.Json;
using System.Text.RegularExpressions;
using PeterPedia.Shared;
using Blazored.Toast.Services;
using System.Text.Json;

namespace PeterPedia.Client.Episodes.Services
{
    public class TVService
    {
        private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
        private static readonly PeterPediaJSONContext Context = new(Options);

        private readonly HttpClient _http;
        private readonly IToastService _toast;

        public TVService(HttpClient httpClient, IToastService toastService)
        {
            _http = httpClient;
            _toast = toastService;
        }

        public List<Show> Shows { get; private set; }

        public event Action RefreshRequested;

        public void CallRequestRefresh()
        {
            RefreshRequested?.Invoke();
        }

        public async Task FetchData()
        {
            if (Shows is null)
            {
                var shows = await _http.GetFromJsonAsync<Show[]>("/api/TV", Context.ShowArray);

                Shows = new List<Show>(shows.Length);
                Shows.AddRange(shows);
            }
        }

        public async Task<Show> Get(int id)
        {
            if (Shows is null)
            {
                await FetchData();
            }

            return Shows.Where(s => s.Id == id).FirstOrDefault();
        }

        public async Task<bool> Add(string url)
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

            using var response = await _http.PostAsJsonAsync("/api/TV", postBody, Context.AddShow);

            if (response.IsSuccessStatusCode)
            {
                var show = await response.Content.ReadFromJsonAsync<Show>(Context.Show);
                Shows.Add(show);

                _toast.ShowSuccess($"Show {show.Title} added");

                return true;
            }
            else
            {
                _toast.ShowError($"Failed to add movie. StatusCode = {response.StatusCode}");

                return false;
            }
        }

        public async Task<bool> Delete(int id)
        {
            var show = await Get(id);
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

        public async Task<bool> Update(Show show)
        {
            if (show is null)
            {
                _toast.ShowError("Invalid show, can't update");
                return false;
            }

            var existingShow = await Get(show.Id);
            if (existingShow is null)
            {
                _toast.ShowError("Can't update a show that doesn't exist.");
                return false;
            }

            using var response = await _http.PutAsJsonAsync("/api/TV", show, Context.Show);

            if (response.IsSuccessStatusCode)
            {
                _toast.ShowSuccess($"Show  {show.Title} saved");

                var serverShow = await _http.GetFromJsonAsync<Show>($"/api/TV/{show.Id}", Context.Show);

                var index = Shows.IndexOf(existingShow);
                Shows[index] = serverShow;

                return true;
            }
            else
            {
                _toast.ShowError($"Failed to save show. StatusCode = {response.StatusCode}");
                return false;
            }
        }

        public async Task<bool> WatchSeason(int showId, int seasonId)
        {
            var show = await Get(showId);
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

            using var response = await _http.PostAsJsonAsync("/api/TV/watch", data, Context.ShowWatchData);
            if (response.IsSuccessStatusCode)
            {
                foreach (var season in show.Seasons)
                {
                    if (season.Id == seasonId)
                    {
                        foreach (var episode in season.Episodes)
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

        public async Task<bool> UnwatchSeason(int showId, int seasonId)
        {
            var show = await Get(showId);
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

            using var response = await _http.PostAsJsonAsync("/api/TV/watch", data, Context.ShowWatchData);
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

        public async Task<bool> WatchEpisode(int showId, int episodeId)
        {
            var show = await Get(showId);
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

            using var response = await _http.PostAsJsonAsync("/api/tv/watch", data, Context.ShowWatchData);
            if (response.IsSuccessStatusCode)
            {
                foreach (var season in show.Seasons)
                {
                    foreach (var episode in season.Episodes)
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

        public async Task<bool> UnwatchEpisode(int showId, int episodeId)
        {
            var show = await Get(showId);
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

            using var response = await _http.PostAsJsonAsync("/api/TV/watch", data, Context.ShowWatchData);
            if (response.IsSuccessStatusCode)
            {
                foreach (var season in show.Seasons)
                {
                    foreach (var episode in season.Episodes)
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
}
