using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Text.Json;

namespace PeterPedia.Client.Services;

public class MovieService
{
    private static readonly JsonSerializerOptions s_Options = new(JsonSerializerDefaults.Web);
    private static readonly PeterPediaJSONContext s_Context = new(s_Options);

    private readonly HttpClient _http;
    private readonly IToastService _toast;

    private readonly List<Movie> _movieList = new();

    public MovieService(HttpClient httpClient, IToastService toastService)
    {
        _http = httpClient;
        _toast = toastService;
    }

    public async Task<List<Movie>> GetMoviesAsync(string filter, bool watchList)
    {
        await FetchMoviesAsync();

        IEnumerable<Movie> movies;

        if (watchList)
        {
            movies = _movieList.Where(m => !m.WatchedDate.HasValue);
        }
        else
        {
            movies = _movieList;
        }

        if (!string.IsNullOrWhiteSpace(filter))
        {
            movies = movies.Where(m => m.Title.Contains(filter, StringComparison.InvariantCultureIgnoreCase) || m.OriginalTitle.Contains(filter, StringComparison.InvariantCultureIgnoreCase));
        }

        return movies.OrderBy(m => m.Title).ToList();
    }

    public async Task<Movie?> GetAsync(int id)
    {
        await FetchMoviesAsync();
        
        return _movieList.Where(b => b.Id == id).FirstOrDefault();
    }

    public async Task<bool> AddAsync(string movieUrl)
    {
        var movieRegex = new Regex("^https://www.themoviedb.org/movie/(\\d+)");

        if (!int.TryParse(movieUrl, out int movieId))
        {
            if (movieRegex.IsMatch(movieUrl))
            {
                var matches = movieRegex.Match(movieUrl);

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

                _movieList.Add(movie);

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
        Movie? movie = await GetAsync(id);
        if (movie is null)
        {
            _toast.ShowError($"{id} is not a valid movie id. Can't remove movie.");
            return false;
        }

        using HttpResponseMessage response = await _http.DeleteAsync($"/api/Movie/{id}");

        if (response.IsSuccessStatusCode)
        {
            _toast.ShowSuccess($"Movie {movie.Title} deleted");

            _movieList.Remove(movie);

            return true;
        }
        else
        {
            _toast.ShowError($"Failed to delete movie. StatusCode = {response.StatusCode}");

            return false;
        }
    }

    public async Task<bool> UpdateAsync(Movie movie)
    {
        if (movie is null)
        {
            _toast.ShowError("Invalid movie, can't update");
            return false;
        }

        var existingMovie = await GetAsync(movie.Id);
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

            return true;
        }
        else
        {
            _toast.ShowError($"Failed to save movie. StatusCode = {response.StatusCode}");
            return false;
        }
    }    

    private async Task FetchMoviesAsync()
    {
        if (_movieList.Count > 0)
        {
            return;
        }

        Movie[]? movies = await _http.GetFromJsonAsync("/api/Movie", s_Context.MovieArray);

        _movieList.Clear();

        if (movies is not null)
        {
            _movieList.AddRange(movies);
        }       
    }
}
