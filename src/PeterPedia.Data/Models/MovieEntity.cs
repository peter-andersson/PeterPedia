using Newtonsoft.Json;
using PeterPedia.Data.Interface;

namespace PeterPedia.Data.Models;

public class MovieEntity : IEntity
{
    [JsonProperty(PropertyName = "id")]
    public string Id { get; set; } = string.Empty;

    public string ImdbId { get; set; } = string.Empty;

    public string OriginalTitle { get; set; } = string.Empty;

    public string OriginalLanguage { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public DateTime? ReleaseDate { get; set; }

    public DateTime? WatchedDate { get; set; }

    public int? RunTime { get; set; }

    public string ETag { get; set; } = string.Empty;

    public string PartitionKey => Id;    
}
