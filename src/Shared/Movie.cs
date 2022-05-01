namespace PeterPedia.Shared;

public class Movie
{
    public Movie()
    {
        TheMovieDbUrl = string.Empty;
        ImdbUrl = string.Empty;
    }

    public int Id { get; set; }

    public string OriginalTitle { get; set; } = null!;

    public string OriginalLanguage { get; set; } = null!;

    public string Title { get; set; } = null!;

    public int? RunTime { get; set; }

    public DateTime? ReleaseDate { get; set; }

    public DateTime? WatchedDate { get; set; }

    public string TheMovieDbUrl { get; set; }

    public string ImdbUrl { get; set; }

    public DateTime LastUpdate { get; set; }

    public bool Search(string searchString)
    {
        if (string.IsNullOrEmpty(searchString))
        {
            return false;
        }

        if (searchString == "*")
        {
            return true;
        }

        if (searchString.Length <= 3)
        {
            return false;
        }

        // 
        return
            Title.Contains(searchString, StringComparison.InvariantCultureIgnoreCase) ||
            OriginalTitle.Contains(searchString, StringComparison.InvariantCultureIgnoreCase);
    }

    public int WatchedState => WatchedDate.HasValue ? 1 : 0;
}
