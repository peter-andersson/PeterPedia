using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;

namespace Movies.Api.Functions;

public class Add
{
    private readonly ILogger<Add> _log;
    private readonly ITheMovieDatabaseService _service;
    private readonly IDataStorage<MovieEntity> _dataStorage;
    private readonly IFileStorage _fileStorage;

    public Add(ILogger<Add> log, ITheMovieDatabaseService service, IDataStorage<MovieEntity> dataStorage, IFileStorage fileStorage)
    {
        _log = log;
        _service = service;
        _dataStorage = dataStorage;
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

        MovieEntity? movie = await _dataStorage.GetAsync(id);

        if (movie is not null)
        {
            return req.Conflict();
        }

        TMDbMovie? tmdbMovie = await _service.GetMovieAsync(id, string.Empty);

        if (tmdbMovie is null)
        {
            _log.LogError("Failed to fetch data for movie with id {id} from themoviedb.org.", id);
            return req.InternalServerError();
        }

        var posterUrl = await _service.GetImageUrlAsync(tmdbMovie.PosterPath);
        var stream = new MemoryStream();
        await _service.DownloadImageUrlToStreamAsync(posterUrl, stream);
        await _fileStorage.UploadAsync($"{id}.jpg", stream);

        movie = new MovieEntity()
        {
            Id = id,
            ImdbId = tmdbMovie.ImdbId ?? string.Empty,
            OriginalLanguage = tmdbMovie.OriginalLanguage,
            OriginalTitle = tmdbMovie.OriginalTitle,
            ReleaseDate = tmdbMovie.ReleaseDate,
            RunTime = tmdbMovie.RunTime,
            Title = tmdbMovie.Title,
            WatchedDate = null,
            ETag = tmdbMovie.ETag
        };

        try
        {
            await _dataStorage.AddAsync(movie);

            _log.LogInformation("Added movie with id {id} and title {title}.", movie.Id, movie.Title);

            return req.Ok();
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Something went wrong.");
            return req.InternalServerError();
        }
    }
}
