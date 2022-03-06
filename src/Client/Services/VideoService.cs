using System.Net.Http.Json;
using System.Text.Json;

namespace PeterPedia.Client.Services;

public class VideoService
{
    private static readonly JsonSerializerOptions s_Options = new(JsonSerializerDefaults.Web);
    private static readonly PeterPediaJSONContext s_Context = new(s_Options);

    private readonly HttpClient _http;

    private readonly IToastService _toast;

    public VideoService(HttpClient httpClient, IToastService toastService)
    {
        _http = httpClient;
        _toast = toastService;
    }

    public List<Video> Videos { get; private set; } = new List<Video>();

    public async Task FetchDataAsync()
    {
        if (Videos.Count == 0)
        {
            Video[]? items = await _http.GetFromJsonAsync("/api/Video", s_Context.VideoArray);

            if (items is not null)
            {
                Videos.AddRange(items);

                Videos = Videos.OrderBy(v => v.Title).ToList();
            }
        }
    }

    public async Task<Video?> GetVideoAsync(int id)
    {
        if (Videos.Count == 0)
        {
            await FetchDataAsync();
        }

        return Videos.Count > 0 ? Videos.Where(v => v.Id == id).SingleOrDefault() : null;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        Video? video = Videos.Where(v => v.Id == id).SingleOrDefault();
        if (video is null)
        {
            _toast.ShowError($"{id} is not a valid id. Can't remove item.");
            return false;
        }

        using HttpResponseMessage response = await _http.DeleteAsync($"/api/Video/{id}");

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
