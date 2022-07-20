using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;

namespace Episodes.Api.Functions;

public class Add
{
    private readonly ILogger<Add> _log;
    private readonly ITheMovieDatabaseService _service;
    private readonly IRepository _repository;
    private readonly IFileStorage _fileStorage;

    public Add(ILogger<Add> log, ITheMovieDatabaseService service, IRepository repository, IFileStorage fileStorage)
    {
        _log = log;
        _service = service;
        _repository = repository;
        _fileStorage = fileStorage;
    }

    [FunctionName("Add")]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "add/{id}")] HttpRequest req,
        string id,
        CancellationToken _)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return req.BadRequest("Missing query parameter id");
        }

        TVShowEntity? show = await _repository.GetAsync<TVShowEntity>(id);

        if (show is not null)
        {
            return req.Conflict();
        }

        TMDbShow? tmdbShow = await _service.GetTvShowAsync(id, string.Empty);

        if (tmdbShow is null)
        {
            _log.LogError("Failed to fetch data for tv show with id {id} from themoviedb.org.", id);
            return req.InternalServerError();
        }

        var posterUrl = await _service.GetImageUrlAsync(tmdbShow.PosterPath);
        var stream = new MemoryStream();
        await _service.DownloadImageUrlToStreamAsync(posterUrl, stream);
        await _fileStorage.UploadAsync($"{id}.jpg", stream);

        show = new TVShowEntity()
        {
            Id = id,
            Title = tmdbShow.Title,
            OriginalTitle = tmdbShow.Title,
            Status = tmdbShow.Status,
            ETag = tmdbShow.ETag,
            NextUpdate = TVShowHelper.GetNextUpdate(tmdbShow.Status)
        };

        foreach (TMDbSeason tmdbSeason in tmdbShow.Seasons)
        {
            var season = new SeasonEntity()
            {
                SeasonNumber = tmdbSeason.SeasonNumber,
            };

            foreach (TMDbEpisode tmdbEpisode in tmdbSeason.Episodes)
            {
                var episode = new EpisodeEntity()
                {
                    EpisodeNumber = tmdbEpisode.EpisodeNumber,
                    Title = tmdbEpisode.Title,
                    AirDate = tmdbEpisode.AirDate,
                    Watched = false,
                };

                season.Episodes.Add(episode);
            }

            show.Seasons.Add(season);
        }

        try
        {
            await _repository.AddAsync(show);

            _log.LogInformation("Added tv show with id {id} and title {title}.", show.Id, show.Title);

            return req.Ok();
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Something went wrong.");
            return req.InternalServerError();
        }
    }
}
