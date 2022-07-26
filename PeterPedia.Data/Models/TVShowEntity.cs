namespace PeterPedia.Data.Models;

public class TVShowEntity : BaseEntity
{
    public string Title { get; set; } = string.Empty;

    public string OriginalTitle { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string Source { get; set; } = string.Empty;

    public string ETag { get; set; } = string.Empty;

    public DateTime? NextUpdate { get; set; }

    public List<SeasonEntity> Seasons { get; private set; } = new List<SeasonEntity>();

    public int UnwatchedEpisodeCount
    {
        get
        {
            var count = 0;
            foreach (SeasonEntity season in Seasons)
            {
                foreach (EpisodeEntity episode in season.Episodes)
                {
                    if (!episode.Watched && episode.AirDate.HasValue)
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
            foreach (SeasonEntity season in Seasons)
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
            foreach (SeasonEntity season in Seasons)
            {
                if (season.SeasonNumber > 0)
                {
                    foreach (EpisodeEntity episode in season.Episodes)
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
