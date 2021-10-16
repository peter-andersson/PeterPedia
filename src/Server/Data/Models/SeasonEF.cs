using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PeterPedia.Server.Data.Models
{
    [Table("season")]
    public class SeasonEF
    {
        public SeasonEF()
        {
            Episodes = new List<EpisodeEF>();
        }

        public int Id { get; set; }

        public int SeasonNumber { get; set; }

        public IList<EpisodeEF> Episodes { get; private set; }

        public int ShowId { get; set; }

        public ShowEF Show { get; set; } = null!;

        public bool IsAllWatched
        {
            get
            {
                foreach (var episode in Episodes)
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
}
