using System.Text.Json.Serialization;

namespace PeterPedia.Models;

public class Video
{
    public Video()
    {
        Title = string.Empty;
        Url = string.Empty;
        Type = string.Empty;
    }

    public int Id { get; set; }

    public string Title { get; set; }

    public string Url { get; set; }

    public string Type { get; set; }

    public TimeSpan Duration { get; set; }
}
