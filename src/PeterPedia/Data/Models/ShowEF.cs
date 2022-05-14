using System.ComponentModel.DataAnnotations.Schema;

namespace PeterPedia.Data.Models;

[Table("show")]
public class ShowEF
{
    public ShowEF()
    {
        Title = string.Empty;
        Status = string.Empty;
        ETag = string.Empty;
        LastUpdate = DateTime.UtcNow;
        Seasons = new List<SeasonEF>();
    }

    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }

    public string Title { get; set; }

    public string Status { get; set; }

    public string ETag { get; set; }

    public IList<SeasonEF> Seasons { get; private set; }

    public int UnwatchedEpisodeCount
    {
        get
        {
            var count = 0;
            foreach (SeasonEF season in Seasons)
            {
                foreach (EpisodeEF episode in season.Episodes)
                {
                    if (!episode.Watched)
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
            return Seasons.Count;
        }
    }

    public int EpisodeCount
    {
        get
        {
            var count = 0;
            foreach (SeasonEF season in Seasons)
            {
                foreach (EpisodeEF episode in season.Episodes)
                {
                    if (episode.AirDate.HasValue)
                    {
                        count += 1;
                    }
                }
            }

            return count;
        }
    }

    public DateTime LastUpdate { get; set; }
}
