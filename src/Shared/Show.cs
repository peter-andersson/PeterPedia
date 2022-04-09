namespace PeterPedia.Shared;

public class Show
{
    public Show()
    {
        Title = string.Empty;
        Status = string.Empty;
        TheMovieDbUrl = string.Empty;
    }

    public int Id { get; set; }

    public string Title { get; set; }

    public string Status { get; set; }

    public IList<Season> Seasons { get; set; } = new List<Season>();

    public string TheMovieDbUrl { get; set; }

    public bool ForceRefresh { get; set; }

    public int UnwatchedEpisodeCount { get; set; }

    public int SeasonCount { get; set; }

    public int EpisodeCount { get; set; }

    public DateTime LastUpdate { get; set; }

    public void Calculate()
    {
        SeasonCount = Seasons.Count;

        EpisodeCount = 0;
        UnwatchedEpisodeCount = 0;
        foreach (Season season in Seasons)
        {
            foreach (Episode episode in season.Episodes)
            {
                if ((episode.AirDate != null) && (episode.AirDate <= DateTime.UtcNow))
                {
                    EpisodeCount += 1;

                    if (!episode.Watched)
                    {
                        UnwatchedEpisodeCount += 1;
                    }
                }
            }
        }
    }

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
        return Title.Contains(searchString, StringComparison.InvariantCultureIgnoreCase);
    }
}
