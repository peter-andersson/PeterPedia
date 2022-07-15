namespace Episodes.Api;

public static class TVShowMapper
{
    public static TVShow ConvertToShow(this TVShowEntity entity)
    {
        var show = new TVShow()
        {
            Id = entity.Id,
            Title = entity.Title,
            OriginalTitle = entity.OriginalTitle,
            Status = entity.Status,
            Source = entity.Source,
        };

        foreach (SeasonEntity seasonEntity in entity.Seasons)
        {
            var season = new Season()
            {
                SeasonNumber = seasonEntity.SeasonNumber,
            };

            foreach (EpisodeEntity episodeEntity in seasonEntity.Episodes)
            {
                var episode = new Episode()
                {
                    EpisodeNumber = episodeEntity.EpisodeNumber,
                    Title = episodeEntity.Title,
                    AirDate = episodeEntity.AirDate,
                    Watched = episodeEntity.Watched,
                };

                season.Episodes.Add(episode);
            }

            show.Seasons.Add(season);
        }

        return show;
    }
        
}
