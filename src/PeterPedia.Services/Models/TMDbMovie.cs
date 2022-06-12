using System.Text.Json.Serialization;

namespace PeterPedia.Services.Models;

public class TMDbMovie
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("imdb_id")]
    public string ImdbId { get; set; } = null!;

    [JsonPropertyName("original_language")]
    public string OriginalLanguage { get; set; } = null!;

    [JsonPropertyName("original_title")]
    public string OriginalTitle { get; set; } = null!;

    [JsonPropertyName("title")]
    public string Title { get; set; } = null!;

    [JsonPropertyName("poster_path")]
    public string? PosterPath { get; set; }

    [JsonPropertyName("runtime")]
    public int? RunTime { get; set; }

    [JsonPropertyName("release_date")]
    [JsonConverter(typeof(DateTimeConverter))]
    public DateTime? ReleaseDate { get; set; }

    public string ETag { get; set; } = string.Empty;
}
