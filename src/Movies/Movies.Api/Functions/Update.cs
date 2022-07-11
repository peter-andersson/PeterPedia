using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;

namespace Movies.Api.Functions;

public class Update
{
    private readonly ILogger<Update> _log;
    private readonly ITheMovieDatabaseService _service;
    private readonly CosmosContext _dbContext;
    private readonly BlobStorage _blobStorage;

    public Update(ILogger<Update> log, ITheMovieDatabaseService service, CosmosContext dbContext, BlobStorage fileStorage)
    {
        _log = log;
        _service = service;
        _dbContext = dbContext;
        _blobStorage = fileStorage;
    }

    [FunctionName("Update")]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "update")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        Movie movie = JsonConvert.DeserializeObject<Movie>(requestBody);

        if (movie is null)
        {
            return new BadRequestObjectResult("Missing movie object");
        }

        MovieEntity existing = await _dbContext.GetAsync(movie.Id);

        if (existing is null)
        {
            return new NotFoundResult();
        }

        if (movie.Refresh)
        {
            TMDbMovie tmdbMovie = await _service.GetMovieAsync(movie.Id, string.Empty);

            if (tmdbMovie is null)
            {
                _log.LogError("Failed to fetch data for movie with id {id} from themoviedb.org", movie.Id);
                return new StatusCodeResult(500);
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return new StatusCodeResult(503);
            }

            var posterUrl = await _service.GetImageUrlAsync(tmdbMovie.PosterPath);
            await _blobStorage.DownloadPosterUrlAsync(movie.Id, posterUrl);

            existing.ImdbId = tmdbMovie.ImdbId ?? string.Empty;
            existing.OriginalLanguage = tmdbMovie.OriginalLanguage;
            existing.OriginalTitle = tmdbMovie.OriginalTitle;
            existing.ReleaseDate = tmdbMovie.ReleaseDate;
            existing.RunTime = tmdbMovie.RunTime;
            existing.ETag = tmdbMovie.ETag;
        }

        existing.WatchedDate = movie.WatchedDate;
        existing.Title = movie.Title;

        if (cancellationToken.IsCancellationRequested)
        {
            return new StatusCodeResult(503);
        }

        try
        {            
            await _dbContext.UpdateAsync(existing);
            _log.LogInformation("Updated movie with id {id}, title {title}, watchDate: {watchDate}", existing.Id, existing.Title, existing.WatchedDate);

            return new OkResult();
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Something went wrong.");
            return new StatusCodeResult(500);
        }
    }
}
