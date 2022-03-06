using System.Text.Json.Serialization;

namespace PeterPedia.Shared;

public class Author
{
    public Author() => Name = string.Empty;

    public int Id { get; set; }

    public string Name { get; set; }

    [JsonConverter(typeof(JsonDateOnlyConverter))]
    public DateOnly DateOfBirth { get; set; }

    public DateTime LastUpdated { get; set; }
}
