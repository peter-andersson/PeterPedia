using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;

namespace Movies.Api.Functions;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Required by Azure function runtime.")]
public class Add
{
    private readonly ILogger<Add> _log;
    private readonly ITheMovieDatabaseService _service;
    private readonly CosmosContext _dbContext;
    private readonly BlobStorage _blobStorage;

    public Add(ILogger<Add> log, ITheMovieDatabaseService service, CosmosContext dbContext, BlobStorage fileStorage)
    {
        _log = log;
        _service = service;
        _dbContext = dbContext;
        _blobStorage = fileStorage;
    }

    [FunctionName("Add")]    
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "add/{id}")] HttpRequest req,
        string id,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return new BadRequestObjectResult("Missing query parameter id");
        }

        MovieEntity? movie = await _dbContext.GetAsync(id);

        if (movie is not null)
        {
            return new ConflictResult();
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return new StatusCodeResult(503);
        }

        TMDbMovie? tmdbMovie = await _service.GetMovieAsync(id, string.Empty);

        if (tmdbMovie is null)
        {
            _log.LogError("Failed to fetch data for movie with id {id} from themoviedb.org.", id);
            return new StatusCodeResult(500);
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return new StatusCodeResult(503);
        }

        var posterUrl = await _service.GetImageUrlAsync(tmdbMovie.PosterPath);
        await _blobStorage.DownloadPosterUrlAsync(id, posterUrl);

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

        if (cancellationToken.IsCancellationRequested)
        {
            return new StatusCodeResult(503);
        }

        try
        {
            await _dbContext.AddAsync(movie);

            _log.LogInformation("Added movie with id {id} and title {title}.", movie.Id, movie.Title);

            return new OkResult();
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Something went wrong.");
            return new StatusCodeResult(500);
        }
    }
}
