using System.Text.Json.Serialization;

namespace TheMovieDatabase.Models;

public class Images
{
    [JsonPropertyName("base_url")]
    public string? BaseUrl { get; set; }

    [JsonPropertyName("secure_base_url")]
    public string? SecureBaseUrl { get; set; } = string.Empty;

    [JsonPropertyName("backdrop_sizes")]
    public List<string> BackdropSizes { get; set; } = new List<string>();

    [JsonPropertyName("logo_sizes")]
    public List<string> LogoSizes { get; set; } = new List<string>();

    [JsonPropertyName("poster_sizes")]
    public List<string> PosterSizes { get; set; } = new List<string>();

    [JsonPropertyName("profile_sizes")]
    public List<string> ProfileSizes { get; set; } = new List<string>();

    [JsonPropertyName("still_sizes")]
    public List<string> StillSizes { get; set; } = new List<string>();
}
