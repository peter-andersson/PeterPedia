using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;

namespace Episodes.Api.Functions;

public class Update
{
    private readonly ILogger<Update> _log;
    private readonly ITheMovieDatabaseService _service;
    private readonly IRepository _repository;
    private readonly IFileStorage _fileStorage;

    public Update(ILogger<Update> log, ITheMovieDatabaseService service, IRepository repository, IFileStorage fileStorage)
    {
        _log = log;
        _service = service;
        _repository = repository;
        _fileStorage = fileStorage;
    }

    [FunctionName("Update")]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "update")] HttpRequest req,
        CancellationToken _)
    {
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        TVShow show = JsonConvert.DeserializeObject<TVShow>(requestBody);

        if (show is null)
        {
            return req.BadRequest("Missing movie object");
        }

        TVShowEntity? existing = await _repository.GetAsync<TVShowEntity>(show.Id);

        if (existing is null)
        {
            return req.NotFound();
        }

        if (show.Refresh)
        {
            TMDbShow? tmdbShow = await _service.GetTvShowAsync(show.Id, string.Empty);

            if (tmdbShow is null)
            {
                _log.LogError("Failed to fetch data for tv show with id {id} from themoviedb.org", show.Id);
                return req.InternalServerError();
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

        try
        {
            await _repository.UpdateAsync(existing);
            _log.LogInformation("Updated tv show with id {id}, title {title}", existing.Id, existing.Title);

            return req.Ok();
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Something went wrong.");
            return req.InternalServerError();
        }
    }
}
