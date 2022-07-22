namespace Episodes.Shared;

public class TVShow
{
    public string Id { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string OriginalTitle { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public List<Season> Seasons { get; set; } = new List<Season>();

    public bool Refresh { get; set; }

    public string Source { get; set; } = string.Empty;

    public string TheMovieDbUrl => $"https://www.themoviedb.org/tv/{Id}";

    public int UnwatchedEpisodeCount
    {
        get
        {
            var count = 0;
            foreach (Season season in Seasons)
            {
                foreach (Episode episode in season.Episodes)
                {
                    if (!episode.Watched && episode.AirDate.HasValue && episode.AirDate.Value < DateTime.UtcNow)
                    {
                        count += 1;
                    }
                }
            }

            return count;
        }
    }

    public int SeasonCount
    {
        get
        {
            var count = 0;
            foreach (Season season in Seasons)
            {
                if (season.SeasonNumber > 0)
                {
                    count += 1;
                }
            }

            return count;
        }
    }

    public int EpisodeCount
    {
        get
        {
            var count = 0;
            foreach (Season season in Seasons)
            {
                if (season.SeasonNumber > 0)
                {
                    foreach (Episode episode in season.Episodes)
                    {
                        if (episode.AirDate.HasValue)
                        {
                            count += 1;
                        }
                    }
                }
            }

            return count;
        }
    }
}
