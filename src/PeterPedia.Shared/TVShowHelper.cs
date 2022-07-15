using PeterPedia.Data.Models;
using TheMovieDatabase.Models;

namespace PeterPedia.Shared
{
    public static class TVShowHelper
    {
        public static DateTime GetNextUpdate(string status)
        {
            return status.ToUpperInvariant() switch
            {
                "RETURNING SERIES" => DateTime.UtcNow.AddDays(1),
                "PLANNED" or "PILOT" or "IN PRODUCTION" => DateTime.UtcNow.AddDays(7),
                "ENDED" or "CANCELED" => DateTime.UtcNow.AddMonths(1),
                _ => DateTime.UtcNow.AddMonths(1),
            };
        }

        public static void UpdateFromTheMovieDb(TVShowEntity show, TMDbShow tmdbShow)
        {
            // Remove season that no longer exists.
            var i = 0;
            while (i < show.Seasons.Count)
            {
                SeasonEntity season = show.Seasons[i];

                TMDbSeason? tmdbSeason = tmdbShow.Seasons.Where(s => s.SeasonNumber == season.SeasonNumber).SingleOrDefault();
                if (tmdbSeason is null)
                {
                    show.Seasons.Remove(season);
                    continue;
                }

                i += 1;
            }

            foreach (TMDbSeason tmdbSeason in tmdbShow.Seasons)
            {
                // Update or add new seasons.
                SeasonEntity? season = show.Seasons.Where(s => s.SeasonNumber == tmdbSeason.SeasonNumber).SingleOrDefault();
                if (season is null)
                {
                    season = new SeasonEntity()
                    {
                        SeasonNumber = tmdbSeason.SeasonNumber,
                    };

                    show.Seasons.Add(season);
                }

                // Remove episode that no longer exists.
                i = 0;
                while (i < season.Episodes.Count)
                {
                    EpisodeEntity episode = season.Episodes[i];

                    TMDbEpisode? tmdbEpisode = tmdbSeason.Episodes.Where(e => e.EpisodeNumber == episode.EpisodeNumber).SingleOrDefault();
                    if (tmdbEpisode is null)
                    {
                        season.Episodes.Remove(episode);
                        continue;
                    }

                    i += 1;
                }

                // Update or add new episodes.
                foreach (TMDbEpisode? tmdbEpisode in tmdbSeason.Episodes)
                {
                    EpisodeEntity? episode = season.Episodes.Where(e => e.EpisodeNumber == tmdbEpisode.EpisodeNumber).SingleOrDefault();
                    if (episode is null)
                    {
                        episode = new EpisodeEntity()
                        {
                            EpisodeNumber = tmdbEpisode.EpisodeNumber,
                            Watched = false
                        };

                        season.Episodes.Add(episode);
                    }

                    episode.Title = tmdbEpisode.Title;
                    episode.AirDate = tmdbEpisode.AirDate;
                }
            }
        }
    }
}
