using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using PeterPedia.Server.Services.Models;

namespace PeterPedia.Server.Services
{
    public class TheMovieDatabaseService
    {
        private readonly string _apiKey;
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly string baseUrl = "https://api.themoviedb.org/3";

        public TheMovieDatabaseService(string apiKey, IHttpClientFactory httpClientFactory)
        {
            _apiKey = apiKey;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<TMDbMovie?> GetMovieAsync(int id)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, GetMovieUrl(id));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            using var httpClient = _httpClientFactory.CreateClient();

            var response = await httpClient.SendAsync(request).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var movie = JsonSerializer.Deserialize<TMDbMovie>(content);

                return movie;
            }

            throw new InvalidOperationException($"Failed to fetch movie with status code {response.StatusCode}");
        }

        public async Task<TMDbShow?> GetTvShowAsync(int id, string? etag)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, GetTvShowUrl(id));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            if (!string.IsNullOrWhiteSpace(etag))
            {
                if (EntityTagHeaderValue.TryParse(etag, out var etagHeaderValue))
                {
                    request.Headers.IfNoneMatch.Add(etagHeaderValue);
                }
            }

            using var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.SendAsync(request).ConfigureAwait(false);

            if (response is null)
            {
                throw new InvalidOperationException("Failed to fetch tv");
            }

            if (response.StatusCode == System.Net.HttpStatusCode.NotModified)
            {
                return null;
            }

            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var show = JsonSerializer.Deserialize<TMDbShow>(content);

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

                foreach (var season in show.Seasons)
                {
                    var seasonData = await GetTvShowSeasonAsync(show.Id, season.SeasonNumber).ConfigureAwait(false);

                    if (seasonData is not null)
                    {
                        season.Episodes = seasonData.Episodes;
                    }
                }

                return show;
            }

            throw new InvalidOperationException($"Failed to fetch tv show with status code {response.StatusCode}");
        }

        private async Task<TMDbSeason?> GetTvShowSeasonAsync(int showId, int seasonNumber)
        {
            using HttpRequestMessage request = new(HttpMethod.Get, GetTvSeasonUrl(showId, seasonNumber));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            using var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.SendAsync(request).ConfigureAwait(false);
            if (response.StatusCode == System.Net.HttpStatusCode.NotModified)
            {
                return null;
            }

            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (string.IsNullOrWhiteSpace(content))
                {
                    return null;
                }

                return JsonSerializer.Deserialize<TMDbSeason>(content);
            }

            throw new InvalidOperationException($"Failed to fetch tv show with status code {response.StatusCode}");
        }

        private string GetTvShowUrl(int id)
        {
            return $"{baseUrl}/tv/{id}";
        }

        private string GetTvSeasonUrl(int showId, int seasonNumber)
        {
            return $"{baseUrl}/tv/{showId}/season/{seasonNumber}";
        }

        private string GetMovieUrl(int id)
        {
            return $"{baseUrl}/movie/{id}";
        }
    }
}
