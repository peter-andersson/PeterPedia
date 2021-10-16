using System;
using System.Collections.Generic;

namespace PeterPedia.Shared
{
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

        public void Calculate()
        {
            SeasonCount = Seasons.Count;

            int count = 0;
            foreach (var season in Seasons)
            {
                count += season.Episodes.Count;
            }
            EpisodeCount = count;

            count = 0;
            foreach (var season in Seasons)
            {
                foreach (var episode in season.Episodes)
                {
                    if ((episode.Watched == false) && (episode.AirDate != null) && (episode.AirDate <= DateTime.UtcNow))
                    {
                        count += 1;
                    }
                }
            }
            UnwatchedEpisodeCount = count;
        }
    }
}
