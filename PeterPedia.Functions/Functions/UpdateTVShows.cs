using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PeterPedia.Data.Implementations;
using PeterPedia.Data.Interface;
using PeterPedia.Data.Models;
using PeterPedia.Shared;
using TheMovieDatabase;
using TheMovieDatabase.Models;

namespace PeterPedia.Functions.Functions;

public class UpdateTVShows
{
    private readonly ILogger<UpdateTVShows> _log;
    private readonly IConfiguration _configuration;
    private readonly ITheMovieDatabaseService _service;
    private readonly IRepository _repository;
    private readonly string _containerId;

    private IFileStorage? _fileStorage;

    public UpdateTVShows(ILogger<UpdateTVShows> log, IConfiguration configuration, ITheMovieDatabaseService service, IRepository repository)
    {
        _log = log;
        _configuration = configuration;
        _service = service;
        _repository = repository;
        _containerId = configuration["Cosmos:EpisodesContainer"];
    }

    [FunctionName("UpdateTVShows")]
    public async Task RunAsync([TimerTrigger("0 0 * * * *")]TimerInfo myTimer)
    {
        _log.LogDebug(myTimer.FormatNextOccurrences(1));

        var blobStorageOptions = new BlobStorageOptions()
        {
            ConnectionString = _configuration["BlobStorage:ConnectionString"],
            Container = _configuration["BlobStorage:EpisodesContainer"]
        };
        _fileStorage = new BlobStorage(Options.Create(blobStorageOptions));

        var query = new QueryDefinition(query: "SELECT * FROM c WHERE c.NextUpdate < GetCurrentDateTime() ORDER BY c.NextUpdate OFFSET 0 LIMIT 20");

        List<TVShowEntity> entities = await _repository.QueryAsync<TVShowEntity>(_containerId, query);

        foreach (TVShowEntity show in entities)
        {
            await UpdateShowAsync(show);

            await _repository.UpdateAsync(_containerId, show);
        }
    }

    private async Task UpdateShowAsync(TVShowEntity show)
    {
        if (_service is null)
        {
            return;
        }

        if (_fileStorage is null)
        {
            return;
        }

        TMDbShow? tmdbShow = await _service.GetTvShowAsync(show.Id, show.ETag);

        if (tmdbShow is null)
        {
            show.NextUpdate = TVShowHelper.GetNextUpdate(show.Status);
            _log.LogInformation("Failed to fetch data for tv show with id {id} from themoviedb.org", show.Id);
            return;
        }

        var filename = $"{show.Id}.jpg";
        if (!await _fileStorage.ExistsAsync(filename))
        {
            var posterUrl = await _service.GetImageUrlAsync(tmdbShow.PosterPath);
            if (!string.IsNullOrWhiteSpace(posterUrl))
            {
                var stream = new MemoryStream();
                await _service.DownloadImageUrlToStreamAsync(posterUrl, stream);
                await _fileStorage.UploadAsync(filename, stream);
            }            
        }

        show.ETag = tmdbShow.ETag;
        show.OriginalTitle = tmdbShow.Title;
        show.Status = tmdbShow.Status;
        show.NextUpdate = TVShowHelper.GetNextUpdate(tmdbShow.Status);

        TVShowHelper.UpdateFromTheMovieDb(show, tmdbShow);
    }
}
