using System;
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
    private readonly IDataStorage<TVShowEntity> _dataStorage;

    private IFileStorage? _fileStorage;

    public UpdateTVShows(ILogger<UpdateTVShows> log, IConfiguration configuration, ITheMovieDatabaseService service, IDataStorage<TVShowEntity> dataStorage)
    {
        _log = log;
        _configuration = configuration;
        _service = service;
        _dataStorage = dataStorage;
    }

    [FunctionName("UpdateTVShows")]
#pragma warning disable IDE0060 // Remove unused parameter
    public async Task RunAsync([TimerTrigger("0 0 * * * *" )]TimerInfo myTimer, ILogger log)
#pragma warning restore IDE0060 // Remove unused parameter
    {
        var blobStorageOptions = new BlobStorageOptions()
        {
            ConnectionString = _configuration["BlobStorage:ConnectionString"],
            Container = _configuration["BlobStorage:EpisodesContainer"]
        };
        _fileStorage = new BlobStorage(Options.Create(blobStorageOptions));

        var query = new QueryDefinition(query: "SELECT * FROM c WHERE c.NextUpdate < GetCurrentDateTime() ORDER BY c.NextUpdate OFFSET 0 LIMIT 20");

        List<TVShowEntity> entities = await _dataStorage.QueryAsync(query);

        foreach (TVShowEntity show in entities)
        {
            await UpdateShowAsync(show);

            await _dataStorage.UpdateAsync(show);
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
       
        var posterUrl = await _service.GetImageUrlAsync(tmdbShow.PosterPath);
        var stream = new MemoryStream();
        await _service.DownloadImageUrlToStreamAsync(posterUrl, stream);
        await _fileStorage.UploadBlobAsync($"{show.Id}.jpg", stream);

        show.ETag = tmdbShow.ETag;
        show.OriginalTitle = tmdbShow.Title;
        show.Status = tmdbShow.Status;
        show.NextUpdate = TVShowHelper.GetNextUpdate(tmdbShow.Status);

        TVShowHelper.UpdateFromTheMovieDb(show, tmdbShow);
    }
}
