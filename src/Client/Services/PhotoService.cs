using System.Net.Http.Json;
using System.Text.Json;

namespace PeterPedia.Client.Services;

public class PhotoService
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
    private static readonly PeterPediaJSONContext Context = new(Options);

    private readonly HttpClient _http;

    public PhotoService(HttpClient httpClient)
    {
        _http = httpClient;
    }

    public List<Photo> Photos { get; private set; } = new List<Photo>();

    public async Task FetchData()
    {
        if (Photos.Count == 0)
        {
            Photo[]? items = await _http.GetFromJsonAsync("/api/Photo", Context.PhotoArray);

            if (items is not null)
            {
                Photos.AddRange(items);
            }
        }
    }  
}