using Newtonsoft.Json;
using PeterPedia.Data.Interface;

namespace PeterPedia.Data.Models;

public class BookEntity : IEntity
{
    [JsonProperty(PropertyName = "id")]
    public string Id { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public bool Reading { get; set; }

    public bool Read { get; set; }

    public bool WantToRead { get; set; }

    public List<string> Authors { get; set; } = new();
}
