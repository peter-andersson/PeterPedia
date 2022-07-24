using Newtonsoft.Json;
using PeterPedia.Data.Interface;

namespace PeterPedia.Data.Models;

public class SubscriptionEntity : IEntity
{
    [JsonProperty(PropertyName = "id")]
    public string Id { get; set; } = string.Empty;

#pragma warning disable CA1822 // Mark members as static
    public string Type => "subscription";
#pragma warning restore CA1822 // Mark members as static

    public string Title { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public int UpdateIntervalMinute { get; set; }

    public string? UpdateAt { get; set; }

    public string? Group { get; set; }

    public DateTime NextUpdate { get; set; }

    public string? Hash { get; set; }

    public DateTime? LastUpdated { get; set; }
}
