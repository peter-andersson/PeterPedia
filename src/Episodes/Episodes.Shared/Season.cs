namespace Episodes.Shared;

public class Season
{
    public int SeasonNumber { get; set; }

    public List<Episode> Episodes { get; set; } = new List<Episode>();

    public bool IsAllWatched
    {
        get
        {
            foreach (Episode episode in Episodes)
            {
                if (!episode.Watched && episode.AirDate.HasValue && episode.AirDate.Value > DateTime.UtcNow)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
