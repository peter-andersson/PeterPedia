namespace PeterPedia.Services.Models;

public class Season
{
    public int Id { get; set; }

    public int SeasonNumber { get; set; }

    public IList<Episode> Episodes { get; set; } = new List<Episode>();

    public bool IsAllWatched
    {
        get
        {
            foreach (Episode episode in Episodes)
            {
                if ((episode.Watched == false) && (episode.AirDate != null) && (episode.AirDate <= DateTime.UtcNow))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
