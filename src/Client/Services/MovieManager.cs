using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Text.Json;
using Microsoft.JSInterop;

namespace PeterPedia.Client.Services;

public class MovieManager : IMovieManager
{
    private static readonly JsonSerializerOptions s_Options = new(JsonSerializerDefaults.Web);
    private static readonly PeterPediaJSONContext s_Context = new(s_Options);

    private readonly HttpClient _http;
    private readonly IToastService _toast;
    private readonly IJSRuntime _js;

    private readonly List<Movie> _movies = new();

    public MovieManager(HttpClient httpClient, IToastService toastService, IJSRuntime js)
    {
        _http = httpClient;
        _toast = toastService;
        _js = js;
    }

    public event Action? MovieChanged;

    public async Task<bool> AddAsync(string movieUrl)
    {
        var movieRegex = new Regex("^https://www.themoviedb.org/movie/(\\d+)");

        if (!int.TryParse(movieUrl, out var movieId))
        {
            if (movieRegex.IsMatch(movieUrl))
            {
                Match? matches = movieRegex.Match(movieUrl);

                if (!int.TryParse(matches.Groups[1].Value, out movieId))
                {
                    _toast.ShowError("Can't add movie, invalid movie id.");
                }
            }
        }

        if (movieId == 0)
        {
            _toast.ShowError("Can't add movie, invalid movie id.");
            return false;
        }

        var postBody = new AddMovie()
        {
            Id = movieId,
        };

        using HttpResponseMessage response = await _http.PostAsJsonAsync("/api/Movie", postBody, s_Context.AddMovie);

        if (response.IsSuccessStatusCode)
        {
            Movie? movie = await response.Content.ReadFromJsonAsync(s_Context.Movie);

            if (movie is not null)
            {
                _toast.ShowSuccess($"Movie {movie.Title} added");

                _movies.Add(movie);

                MovieChanged?.Invoke();

                await _js.InvokeVoidAsync("movieStore.put", movie);

                return true;
            }
            else
            {
                _toast.ShowError($"Failed to add movie. No movie from server.");

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
        Movie? movie = Get(id);
        if (movie is null)
        {
            _toast.ShowError($"{id} is not a valid movie id. Can't remove movie.");
            return false;
        }

        using HttpResponseMessage response = await _http.DeleteAsync($"/api/Movie/{id}");

        if (response.IsSuccessStatusCode)
        {
            _toast.ShowSuccess($"Movie {movie.Title} deleted");

            _movies.Remove(movie);

            MovieChanged?.Invoke();

            await _js.InvokeVoidAsync("movieStore.delete", movie.Id);

            return true;
        }
        else
        {
            _toast.ShowError($"Failed to delete movie. StatusCode = {response.StatusCode}");

            return false;
        }
    }
   
    public async Task<List<Movie>> GetAsync()
    {
        if (_movies.Count == 0)
        {
            Movie[] movies = await _js.InvokeAsync<Movie[]>("movieStore.getAll");

            foreach (Movie movie in movies)
            {
                _movies.Add(movie);
            }
        }

        if (_movies.Count == 0)
        {
            await RefreshAsync();
        }

        return _movies;
    }

    public async Task RefreshAsync()
    {
        if (await FetchChangedMoviesAsync() || await FetchDeletedMoviesAsync())
        {
            MovieChanged?.Invoke();
        }
    }

    public async Task<bool> UpdateAsync(Movie movie)
    {
        if (movie is null)
        {
            _toast.ShowError("Invalid movie, can't update");
            return false;
        }

        Movie? existingMovie = Get(movie.Id);
        if (existingMovie is null)
        {
            _toast.ShowError("Can't update a movie that doesn't exist.");
            return false;
        }

        using HttpResponseMessage response = await _http.PutAsJsonAsync("/api/Movie", movie, s_Context.Movie);

        if (response.IsSuccessStatusCode)
        {
            _toast.ShowSuccess($"Movie {movie.Title} saved");

            existingMovie.Title = movie.Title;
            existingMovie.WatchedDate = movie.WatchedDate;

            await _js.InvokeVoidAsync("movieStore.put", existingMovie);

            MovieChanged?.Invoke();

            return true;
        }
        else
        {
            _toast.ShowError($"Failed to save movie. StatusCode = {response.StatusCode}");
            return false;
        }
    }

    private Movie? Get(int id) => _movies.Where(m => m.Id == id).FirstOrDefault();

    private async Task<bool> FetchChangedMoviesAsync()
    {
        var changed = false;
        Movie? mostRecentlyUpdated = await _js.InvokeAsync<Movie>("movieStore.getFirstFromIndex", "lastUpdate", "prev");
        DateTime since = mostRecentlyUpdated?.LastUpdate ?? DateTime.MinValue;
        var json = await _http.GetStringAsync($"/api/Movie?lastupdate={since:yyyyMMddHHmmss}");

        if (!string.IsNullOrWhiteSpace(json))
        {
            await _js.InvokeVoidAsync("movieStore.putAllFromJson", json);

            Movie[]? movies = JsonSerializer.Deserialize(json, s_Context.MovieArray);

            if (movies is not null)
            {
                foreach (Movie movie in movies)
                {
                    Movie? existing = Get(movie.Id);

                    if (existing is not null)
                    {
                        existing.OriginalTitle = movie.OriginalTitle;
                        existing.OriginalLanguage = movie.OriginalLanguage;
                        existing.WatchedDate = movie.WatchedDate;
                        existing.LastUpdate = movie.LastUpdate;
                        existing.ReleaseDate = movie.ReleaseDate;
                        existing.RunTime = movie.RunTime;
                    }
                    else
                    {
                        _movies.Add(movie);
                        changed = true;
                    }
                }
            }
        }

        return changed;
    }

    private async Task<bool> FetchDeletedMoviesAsync()
    {
        var changed = false;
        DeleteLog? latestDeletion = await _js.InvokeAsync<DeleteLog>("movieStore.getDeleted");
        DateTime since = latestDeletion?.Deleted ?? DateTime.MinValue;
        var json = await _http.GetStringAsync($"/api/Movie/deleted?deleted={since:yyyyMMddHHmmss}");
        if (!string.IsNullOrWhiteSpace(json))
        {
            DeleteLog[]? deletions = JsonSerializer.Deserialize(json, s_Context.DeleteLogArray);

            if (deletions is not null && deletions.Length > 0)
            {
                DeleteLog? maxDeleteTime = null;

                foreach (DeleteLog deletion in deletions)
                {
                    Movie? existing = Get(deletion.Id);
                    if (existing is not null)
                    {
                        await _js.InvokeVoidAsync("movieStore.delete", existing.Id);
                        _movies.Remove(existing);
                        changed = true;
                    }

                    if ((maxDeleteTime is null) || (maxDeleteTime.Deleted < deletion.Deleted))
                    {
                        maxDeleteTime = deletion;
                    }
                }

                if (maxDeleteTime is not null)
                {
                    await _js.InvokeVoidAsync("movieStore.putDeleted", maxDeleteTime);
                }
            }
        }

        return changed;
    }
}
