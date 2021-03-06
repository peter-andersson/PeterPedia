using System.Text.Json.Serialization;

namespace TheMovieDatabase.Models;

public class TMDbSeason
{
    [JsonPropertyName("season_number")]
    public int SeasonNumber { get; set; }

    [JsonPropertyName("episodes")]
    public List<TMDbEpisode> Episodes { get; set; } = new List<TMDbEpisode>();
}
