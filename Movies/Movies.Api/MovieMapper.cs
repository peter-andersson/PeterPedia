namespace Movies.Api;

public static class MovieMapper
{
    public static Movie ConvertToMovie(this MovieEntity entity) =>
        new()
        {
            Id = entity.Id,
            ImdbId = entity.ImdbId,
            OriginalLanguage = entity.OriginalLanguage,
            OriginalTitle = entity.OriginalTitle,
            ReleaseDate = entity.ReleaseDate,
            RunTime = entity.RunTime,
            Title = entity.Title,
        };
}
