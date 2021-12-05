using System.Text.Json.Serialization;

namespace PeterPedia.Shared;

[JsonSerializable(typeof(AddMovie))]
[JsonSerializable(typeof(AddShow))]
[JsonSerializable(typeof(Article))]
[JsonSerializable(typeof(Article[]))]
[JsonSerializable(typeof(Author))]
[JsonSerializable(typeof(Author[]))]
[JsonSerializable(typeof(Book))]
[JsonSerializable(typeof(Book[]))]
[JsonSerializable(typeof(Movie))]
[JsonSerializable(typeof(Movie[]))]
[JsonSerializable(typeof(Show))]
[JsonSerializable(typeof(Show[]))]
[JsonSerializable(typeof(ShowWatchData))]
[JsonSerializable(typeof(Subscription))]
[JsonSerializable(typeof(Subscription[]))]
[JsonSerializable(typeof(Video[]))]
public partial class PeterPediaJSONContext : JsonSerializerContext
{
}