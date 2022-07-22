namespace Episodes.Shared;

public class Episode
{
    public string Title { get; set; } = string.Empty;

    public int EpisodeNumber { get; set; }

    public DateTime? AirDate { get; set; }

    public bool Watched { get; set; }
}
