using System;
using System.Text.Json.Serialization;

namespace PeterPedia.Server.Services.Models
{
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

        [JsonPropertyName("runtime")]
        public int? RunTime { get; set; }

        [JsonPropertyName("release_date")]
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime? ReleaseDate { get; set; }
    }
}
