using Newtonsoft.Json;
using PeterPedia.Data.Interface;

namespace PeterPedia.Data.Models;

public class ArticleEntity : IEntity
{
    [JsonProperty(PropertyName = "id")]
    public string Id { get; set; } = string.Empty;

#pragma warning disable CA1822 // Mark members as static
    public string Type => "article";
#pragma warning restore CA1822 // Mark members as static

    public string Title { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public string Group { get; set; } = string.Empty;

    public string Subscription { get; set; } = string.Empty;

    public DateTime PublishDate { get; set; }

    public DateTime? ReadDate { get; set; }
}
