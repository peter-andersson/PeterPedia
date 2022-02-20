using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Text.Json;

namespace PeterPedia.Client.Services;

public class MovieService
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
    private static readonly PeterPediaJSONContext Context = new(Options);

    private readonly HttpClient _http;
    private readonly IToastService _toast;

    private readonly List<Movie> _movieList = new();

    public MovieService(HttpClient httpClient, IToastService toastService)
    {
        _http = httpClient;
        _toast = toastService;
    }

    public async Task<List<Movie>> GetMovies(string filter, bool watchList)
    {
        await FetchMovies();

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

    public async Task<Movie?> Get(int id)
    {
        await FetchMovies();
        
        return _movieList.Where(b => b.Id == id).FirstOrDefault();
    }

    public async Task<bool> Add(string movieUrl)
    {
        var movieRegex = new Regex("^https://www.themoviedb.org/movie/(\\d+)");

        if (!int.TryParse(movieUrl, out int movieId))
        {
            if (movieRegex.IsMatch(movieUrl))
            {
                var matches = movieRegex.Match(movieUrl);

                if (!int.TryParse(matches.Groups[1].Value, out movieId))
                {
                    await _toast.ShowError("Can't add movie, invalid movie id.");
                }
            }
        }

        if (movieId == 0)
        {
            await _toast.ShowError("Can't add movie, invalid movie id.");
            return false;
        }

        var postBody = new AddMovie()
        {
            Id = movieId,
        };

        using var response = await _http.PostAsJsonAsync("/api/Movie", postBody, Context.AddMovie);

        if (response.IsSuccessStatusCode)
        {
            Movie? movie = await response.Content.ReadFromJsonAsync(Context.Movie);

            if (movie is not null)
            {
                await _toast.ShowSuccess($"Movie {movie.Title} added");

                _movieList.Add(movie);

                return true;
            }
            else
            {
                await _toast.ShowError($"Failed to add movie. No movie from server.");

                return false;
            }            
        }
        else
        {
            await _toast.ShowError($"Failed to add movie. StatusCode = {response.StatusCode}");

            return false;
        }
    }

    public async Task<bool> Delete(int id)
    {
        var movie = await Get(id);
        if (movie is null)
        {
            await _toast.ShowError($"{id} is not a valid movie id. Can't remove movie.");
            return false;
        }

        using var response = await _http.DeleteAsync($"/api/Movie/{id}");

        if (response.IsSuccessStatusCode)
        {
            await _toast.ShowSuccess($"Movie {movie.Title} deleted");

            _movieList.Remove(movie);

            return true;
        }
        else
        {
            await _toast.ShowError($"Failed to delete movie. StatusCode = {response.StatusCode}");

            return false;
        }
    }

    public async Task<bool> Update(Movie movie)
    {
        if (movie is null)
        {
            await _toast.ShowError("Invalid movie, can't update");
            return false;
        }

        var existingMovie = await Get(movie.Id);
        if (existingMovie is null)
        {
            await _toast.ShowError("Can't update a movie that doesn't exist.");
            return false;
        }

        using var response = await _http.PutAsJsonAsync("/api/Movie", movie, Context.Movie);

        if (response.IsSuccessStatusCode)
        {
            await _toast.ShowSuccess($"Movie {movie.Title} saved");

            existingMovie.Title = movie.Title;
            existingMovie.WatchedDate = movie.WatchedDate;

            return true;
        }
        else
        {
            await _toast.ShowError($"Failed to save movie. StatusCode = {response.StatusCode}");
            return false;
        }
    }    

    private async Task FetchMovies()
    {
        if (_movieList.Count > 0)
        {
            return;
        }

        var movies = await _http.GetFromJsonAsync("/api/Movie", Context.MovieArray);

        _movieList.Clear();

        if (movies is not null)
        {
            _movieList.AddRange(movies);
        }       
    }
}
