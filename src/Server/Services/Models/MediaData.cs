using System.Text.Json.Serialization;

namespace PeterPedia.Shared.Services.Models
{
    public class MediaData
    {
        [JsonPropertyName("media")]
        public Media? Media { get; set; }
    }
}