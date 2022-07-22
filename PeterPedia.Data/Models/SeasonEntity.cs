namespace PeterPedia.Data.Models;

public class SeasonEntity
{
    public int SeasonNumber { get; set; }

    public List<EpisodeEntity> Episodes { get; private set; } = new List<EpisodeEntity>();

    public bool IsAllWatched
    {
        get
        {
            foreach (EpisodeEntity episode in Episodes)
            {
                if (!episode.Watched)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
