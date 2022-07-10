using System.Text.Json.Serialization;

namespace TheMovieDatabase.Models;

public class Configuration
{
    [JsonPropertyName("images")]
    public Images Images { get; set; } = new Images();

    [JsonPropertyName("change_keys")]
    public List<string> ChangeKeys { get; set; } = new List<string>();  
}
