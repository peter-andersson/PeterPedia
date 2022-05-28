using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using PeterPedia.Services.Models;

namespace PeterPedia.Services;

public class TheMovieDatabaseService : ITheMovieDatabaseService
{
    private readonly string _apiKey;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _baseUrl = "https://api.themoviedb.org/3";
    private readonly IMemoryCache _cache;
    private readonly PeterPediaContext _dbContext;

    public TheMovieDatabaseService(string apiKey, IHttpClientFactory httpClientFactory, IMemoryCache cache, PeterPediaContext dbContext)
    {
        _apiKey = apiKey;
        _httpClientFactory = httpClientFactory;
        _cache = cache;
        _dbContext = dbContext;
    }

    public async Task<TMDbMovie?> GetMovieAsync(int id, string? etag)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, GetMovieUrl(id));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        if (!string.IsNullOrWhiteSpace(etag))
        {
            if (EntityTagHeaderValue.TryParse(etag, out EntityTagHeaderValue? etagHeaderValue))
            {
                request.Headers.IfNoneMatch.Add(etagHeaderValue);
            }
        }

        using HttpClient httpClient = _httpClientFactory.CreateClient();

        HttpResponseMessage response = await httpClient.SendAsync(request).ConfigureAwait(false);

        if (response.StatusCode == System.Net.HttpStatusCode.NotModified)
        {
            return null;
        }

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            TMDbMovie? movie = JsonSerializer.Deserialize<TMDbMovie>(content);

            if (movie is null)
            {
                return null;
            }

            if (response.Headers != null &&
                response.Headers.ETag != null &&
                !string.IsNullOrWhiteSpace(response.Headers.ETag.Tag))
            {
                movie.ETag = response.Headers.ETag.Tag;
            }

            return movie;
        }

        var error = new ErrorEF()
        {
            Module = "TMDB",
            Error = $"Failed to fetch movie {id} with status code {response.StatusCode}"
        };

        _dbContext.Errors.Add(error);
        await _dbContext.SaveChangesAsync();

        throw new InvalidOperationException(error.Error);
    }

    public async Task<TMDbShow?> GetTvShowAsync(int id, string? etag)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, GetTvShowUrl(id));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        if (!string.IsNullOrWhiteSpace(etag))
        {
            if (EntityTagHeaderValue.TryParse(etag, out EntityTagHeaderValue? etagHeaderValue))
            {
                request.Headers.IfNoneMatch.Add(etagHeaderValue);
            }
        }

        using HttpClient httpClient = _httpClientFactory.CreateClient();
        HttpResponseMessage response = await httpClient.SendAsync(request).ConfigureAwait(false);

        if (response.StatusCode == System.Net.HttpStatusCode.NotModified)
        {
            return null;
        }

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            TMDbShow? show = JsonSerializer.Deserialize<TMDbShow>(content);

            if (show is null)
            {
                return null;
            }

            if (response.Headers != null &&
                response.Headers.ETag != null &&
                !string.IsNullOrWhiteSpace(response.Headers.ETag.Tag))
            {
                show.ETag = response.Headers.ETag.Tag;
            }

            foreach (TMDbSeason season in show.Seasons)
            {
                TMDbSeason? seasonData = await GetTvShowSeasonAsync(show.Id, season.SeasonNumber).ConfigureAwait(false);

                if (seasonData is not null)
                {
                    season.Episodes = seasonData.Episodes;
                }
            }

            return show;
        }

        var error = new ErrorEF()
        {
            Module = "TMDB",
            Error = $"Failed to fetch tv show {id} with status code {response.StatusCode}"
        };

        _dbContext.Errors.Add(error);
        await _dbContext.SaveChangesAsync();

        throw new InvalidOperationException(error.Error);
    }

    public async Task<string> GetImageUrlAsync(string? path, string size = "w185")
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return string.Empty;
        }

        Configuration? configuration = await GetConfigurationAsync();

        return configuration is null
            ? string.Empty
            : $"{configuration.Images.SecureBaseUrl}{size}{path}";
    }

    private async Task<TMDbSeason?> GetTvShowSeasonAsync(int showId, int seasonNumber)
    {
        using HttpRequestMessage request = new(HttpMethod.Get, GetTvSeasonUrl(showId, seasonNumber));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        using HttpClient httpClient = _httpClientFactory.CreateClient();
        HttpResponseMessage response = await httpClient.SendAsync(request).ConfigureAwait(false);
        if (response.StatusCode == System.Net.HttpStatusCode.NotModified)
        {
            return null;
        }

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return string.IsNullOrWhiteSpace(content) ? null : JsonSerializer.Deserialize<TMDbSeason>(content);
        }

        var error = new ErrorEF()
        {
            Module = "TMDB",
            Error = $"Failed to fetch tv show {showId} season {seasonNumber} with status code {response.StatusCode}"
        };

        _dbContext.Errors.Add(error);
        await _dbContext.SaveChangesAsync();

        throw new InvalidOperationException(error.Error);
    }

    private string GetTvShowUrl(int id) => $"{_baseUrl}/tv/{id}";

    private string GetTvSeasonUrl(int showId, int seasonNumber) => $"{_baseUrl}/tv/{showId}/season/{seasonNumber}";

    private string GetMovieUrl(int id) => $"{_baseUrl}/movie/{id}";

    private async Task<Configuration?> GetConfigurationAsync()
    {
        var cacheKey = "TMDBConfiguration";

        if (_cache.TryGetValue(cacheKey, out Configuration data))
        {
            return data;
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/configuration");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        using HttpClient httpClient = _httpClientFactory.CreateClient();

        HttpResponseMessage response = await httpClient.SendAsync(request).ConfigureAwait(false);

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            Configuration? configuration = JsonSerializer.Deserialize<Configuration>(content);

            if (configuration is not null)
            {
                _cache.Set(cacheKey, configuration, TimeSpan.FromDays(1));
            }

            return configuration;
        }

        var error = new ErrorEF()
        {
            Module = "TMDB",
            Error = $"Failed to fetch configuration with status code {response.StatusCode}"
        };

        _dbContext.Errors.Add(error);
        await _dbContext.SaveChangesAsync();
        
        throw new InvalidOperationException(error.Error);
    }
}
