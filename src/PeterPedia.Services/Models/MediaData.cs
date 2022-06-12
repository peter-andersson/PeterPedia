using System.Text.Json.Serialization;

namespace PeterPedia.Services.Models;

public class MediaData
{
    [JsonPropertyName("media")]
    public Media? Media { get; set; }
}
