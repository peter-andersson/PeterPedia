using System.Text.Json.Serialization;

namespace PeterPedia.Services.Models;

public class TMDbEpisode
{
    [JsonPropertyName("air_date")]
    [JsonConverter(typeof(DateTimeConverter))]
    public DateTime? AirDate { get; set; }

    [JsonPropertyName("episode_number")]
    public int EpisodeNumber { get; set; }

    [JsonPropertyName("name")]
    public string Title { get; set; } = string.Empty;
}
