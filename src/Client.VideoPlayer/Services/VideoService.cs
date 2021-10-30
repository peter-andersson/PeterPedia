namespace PeterPedia.Client.VideoPlayer.Services
{
    using PeterPedia.Shared;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using Blazored.Toast.Services;

    public class VideoService
    {
        private readonly HttpClient _http;

        private readonly IToastService _toast;

        public VideoService(HttpClient httpClient, IToastService toastService)
        {
            _http = httpClient;
            _toast = toastService;
        }

        public List<Video> Videos { get; private set; }

        public async Task FetchData()
        {
            if ((Videos is null) || (Videos.Count == 0))
            {
                var items = await _http.GetFromJsonAsync<Video[]>("/api/Video");

                Videos = new List<Video>(items.Length);
                Videos.AddRange(items);

                Videos = Videos.OrderBy(v => v.Title).ToList();
            }
        }

        public async Task<Video> GetVideoAsync(int id)
        {
            if ((Videos is null) || (Videos.Count == 0))
            {
                await FetchData();
            }

            if (Videos is not null)
            {
                return Videos.Where(v => v.Id == id).SingleOrDefault();
            }
            else
            {
                return null;
            }
        }

        public async Task<bool> Delete(int id)
        {
            var video = Videos.Where(v => v.Id == id).SingleOrDefault();
            if (video is null)
            {
                _toast.ShowError($"{id} is not a valid id. Can't remove item.");
                return false;
            }

            using var response = await _http.DeleteAsync($"/api/Video/{id}");

            if (response.IsSuccessStatusCode)
            {
                _toast.ShowSuccess($"Removed video {video.Title}");

                Videos.Remove(video);

                return true;
            }
            else
            {
                _toast.ShowSuccess($"Failed to delete item. StatusCode = {response.StatusCode}");

                return false;
            }
        }
    }
}
