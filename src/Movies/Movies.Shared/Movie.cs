namespace Movies.Api.Models;

public class Movie
{
    public string Id { get; set; } = string.Empty;

    public string? ImdbId { get; set; }

    public string? OriginalTitle { get; set; }

    public string? OriginalLanguage { get; set; }

    public string Title { get; set; } = string.Empty;

    public DateTime? ReleaseDate { get; set; }

    public DateTime? WatchedDate { get; set; }

    public int? RunTime { get; set; }

    public bool Refresh { get; set; }
}
