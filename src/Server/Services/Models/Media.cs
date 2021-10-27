using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PeterPedia.Shared.Services.Models
{
    public class Media
    {
        [JsonPropertyName("track")]
        public List<Track> Track { get; set; } = new List<Track>();

        [JsonPropertyName("@ref")]
        public string? FileName { get; set; }
    }
}