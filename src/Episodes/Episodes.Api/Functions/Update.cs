using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;
using PeterPedia.Shared;

namespace Episodes.Api.Functions;

public class Update
{
    private readonly ILogger<Update> _log;
    private readonly ITheMovieDatabaseService _service;
    private readonly IDataStorage<TVShowEntity> _dataStorage;
    private readonly IFileStorage _fileStorage;

    public Update(ILogger<Update> log, ITheMovieDatabaseService service, IDataStorage<TVShowEntity> dbContext, IFileStorage fileStorage)
    {
        _log = log;
        _service = service;
        _dataStorage = dbContext;
        _fileStorage = fileStorage;
    }

    [FunctionName("Update")]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "update")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        TVShow show = JsonConvert.DeserializeObject<TVShow>(requestBody);

        if (show is null)
        {
            return new BadRequestObjectResult("Missing movie object");
        }

        TVShowEntity? existing = await _dataStorage.GetAsync(show.Id, show.Id);

        if (existing is null)
        {
            return new NotFoundResult();
        }

        if (show.Refresh)
        {
            TMDbShow? tmdbShow = await _service.GetTvShowAsync(show.Id, string.Empty);

            if (tmdbShow is null)
            {
                _log.LogError("Failed to fetch data for tv show with id {id} from themoviedb.org", show.Id);
                return new StatusCodeResult(500);
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return new StatusCodeResult(503);
            }

            var posterUrl = await _service.GetImageUrlAsync(tmdbShow.PosterPath);
            var stream = new MemoryStream();
            await _service.DownloadImageUrlToStreamAsync(posterUrl, stream);
            await _fileStorage.UploadAsync($"{existing.Id}.jpg", stream);
            
            existing.ETag = tmdbShow.ETag;
            existing.OriginalTitle = tmdbShow.Title;
            existing.Status = tmdbShow.Status;
            existing.NextUpdate = TVShowHelper.GetNextUpdate(tmdbShow.Status);

            TVShowHelper.UpdateFromTheMovieDb(existing, tmdbShow);
        }
        else
        {
            foreach (Season season in show.Seasons)
            {
                SeasonEntity? seasonEntity = existing.Seasons.Where(s => s.SeasonNumber == season.SeasonNumber).FirstOrDefault();

                if (seasonEntity is null)
                {
                    continue;
                }

                foreach (Episode episode in season.Episodes)
                {
                    EpisodeEntity? episodeEntity = seasonEntity.Episodes.Where(e => e.EpisodeNumber == episode.EpisodeNumber).FirstOrDefault();

                    if (episodeEntity is null)
                    {
                        continue;                        
                    }

                    episodeEntity.Watched = episode.Watched;
                }
            }
        }

        existing.Source = show.Source;
        existing.Title = show.Title;

        if (cancellationToken.IsCancellationRequested)
        {
            return new StatusCodeResult(503);
        }

        try
        {
            await _dataStorage.UpdateAsync(existing);
            _log.LogInformation("Updated tv show with id {id}, title {title}", existing.Id, existing.Title);

            return new OkResult();
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Something went wrong.");
            return new StatusCodeResult(500);
        }
    }
}
