using System;
using Newtonsoft.Json;

namespace Movies.Api.Models;

public class MovieEntity
{
    [JsonProperty(PropertyName = "id")]
    public string Id { get; set; }

    public string ImdbId { get; set; }

    public string OriginalTitle { get; set; }

    public string OriginalLanguage { get; set; }

    public string Title { get; set; }

    public DateTime? ReleaseDate { get; set; }

    public DateTime? WatchedDate { get; set; }

    public int? RunTime { get; set; }

    public string ETag { get; set; }

    public override string ToString() => JsonConvert.SerializeObject(this);
}
