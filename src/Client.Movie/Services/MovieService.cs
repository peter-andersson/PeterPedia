namespace PeterPedia.Client.Movie.Services
{
    using PeterPedia.Shared;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Blazored.Toast;
    using Blazored.Toast.Services;
    using System.Text.Json;

    public class MovieService
    {
        private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
        private static readonly PeterPediaJSONContext Context = new(Options);

        private readonly HttpClient _http;
        private readonly IToastService _toast;

        public MovieService(HttpClient httpClient, IToastService toastService)
        {
            _http = httpClient;
            _toast = toastService;
        }

        public List<Movie> Movies { get; private set; }

        public async Task FetchData()
        {
            if (Movies is null)
            {
                await FetchMovies();
            }
        }

        public async Task<Movie> Get(int id)
        {
            if (Movies is null)
            {
                await FetchData();
            }

            return Movies.Where(b => b.Id == id).FirstOrDefault();
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

            using var response = await _http.PostAsJsonAsync("/api/Movie", postBody, Context.AddMovie);

            if (response.IsSuccessStatusCode)
            {
                var movie = await response.Content.ReadFromJsonAsync<Movie>(Context.Movie);

                _toast.ShowSuccess($"Movie {movie.Title} added");

                Movies.Add(movie);

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
            var movie = await Get(id);
            if (movie is null)
            {
                _toast.ShowError($"{id} is not a valid movie id. Can't remove movie.");
                return false;
            }

            using var response = await _http.DeleteAsync($"/api/Movie/{id}");

            if (response.IsSuccessStatusCode)
            {
                _toast.ShowSuccess($"Movie {movie.Title} deleted");

                Movies.Remove(movie);

                return true;
            }
            else
            {
                _toast.ShowError($"Failed to delete movie. StatusCode = {response.StatusCode}");

                return false;
            }
        }

        public async Task<bool> Update(Movie movie)
        {
            if (movie is null)
            {
                _toast.ShowError("Invalid movie, can't update");
                return false;
            }

            var existingMovie = await Get(movie.Id);
            if (existingMovie is null)
            {
                _toast.ShowError("Can't update a movie that doesn't exist.");
                return false;
            }

            using var response = await _http.PutAsJsonAsync("/api/Movie", movie, Context.Movie);

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

        private async Task FetchMovies()
        {
            var movies = await _http.GetFromJsonAsync<Movie[]>("/api/Movie", Context.MovieArray);

            Movies = new List<Movie>(movies.Length);
            Movies.AddRange(movies);
        }
    }
}
