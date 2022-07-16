using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;

namespace Movies.Api.Functions;

public class Update
{
    private readonly ILogger<Update> _log;
    private readonly ITheMovieDatabaseService _service;
    private readonly IDataStorage<MovieEntity> _dataStorage;
    private readonly IFileStorage _fileStorage;

    public Update(ILogger<Update> log, ITheMovieDatabaseService service, IDataStorage<MovieEntity> dbContext, IFileStorage fileStorage)
    {
        _log = log;
        _service = service;
        _dataStorage = dbContext;
        _fileStorage = fileStorage;
    }

    [FunctionName("Update")]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "update")] HttpRequest req,
        CancellationToken _)
    {
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        Movie movie = JsonConvert.DeserializeObject<Movie>(requestBody);

        if (movie is null)
        {
            return req.BadRequest("Missing movie object");
        }

        MovieEntity? existing = await _dataStorage.GetAsync(movie.Id, movie.Id);

        if (existing is null)
        {
            return req.NotFound();
        }

        if (movie.Refresh)
        {
            TMDbMovie? tmdbMovie = await _service.GetMovieAsync(movie.Id, string.Empty);

            if (tmdbMovie is null)
            {
                _log.LogError("Failed to fetch data for movie with id {id} from themoviedb.org", movie.Id);
                return req.InternalServerError();
            }

            var posterUrl = await _service.GetImageUrlAsync(tmdbMovie.PosterPath);
            var stream = new MemoryStream();
            await _service.DownloadImageUrlToStreamAsync(posterUrl, stream);
            await _fileStorage.UploadAsync($"{existing.Id}.jpg", stream);

            existing.ImdbId = tmdbMovie.ImdbId ?? string.Empty;
            existing.OriginalLanguage = tmdbMovie.OriginalLanguage;
            existing.OriginalTitle = tmdbMovie.OriginalTitle;
            existing.ReleaseDate = tmdbMovie.ReleaseDate;
            existing.RunTime = tmdbMovie.RunTime;
            existing.ETag = tmdbMovie.ETag;
        }

        existing.WatchedDate = movie.WatchedDate;
        existing.Title = movie.Title;

        try
        {            
            await _dataStorage.UpdateAsync(existing);
            _log.LogInformation("Updated movie with id {id}, title {title}, watchDate: {watchDate}", existing.Id, existing.Title, existing.WatchedDate);

            return req.Ok();
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Something went wrong.");
            return req.InternalServerError();
        }
    }
}
