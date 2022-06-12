namespace PeterPedia.Services.Models;

public class Episode
{
    public Episode()
    {
        Title = string.Empty;
        ShowTitle = string.Empty;
    }

    public int Id { get; set; }

    public string Title { get; set; }

    public int EpisodeNumber { get; set; }

    public DateTime? AirDate { get; set; }

    public bool Watched { get; set; }

    public string ShowTitle { get; set; }

    public int SeasonNumber { get; set; }
}
