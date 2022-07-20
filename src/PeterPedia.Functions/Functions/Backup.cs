using Azure.Storage.Blobs;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PeterPedia.Data.Interface;
using PeterPedia.Data.Models;

namespace PeterPedia.Functions.Functions;

public class Backup
{
    private readonly ILogger<Backup> _log;
    private readonly IConfiguration _configuration;
    private readonly IRepository _repository;

    public Backup(ILogger<Backup> log, IConfiguration configuration, IRepository repository)
    {
        _log = log;
        _configuration = configuration;
        _repository = repository;
    }

    [FunctionName("Backup")]
    public async Task RunAsync([TimerTrigger("0 0 0 * * SUN")]TimerInfo myTimer, ILogger log)
    {
        log.LogDebug(myTimer.FormatNextOccurrences(1));

        try
        {
            var query = new QueryDefinition(query: "SELECT * FROM c");

            var data = new BackupData
            {
                Shows = await _repository.QueryAsync<TVShowEntity>(_configuration["Cosmos:EpisodesContainer"], query),
                Movies = await _repository.QueryAsync<MovieEntity>(_configuration["Cosmos:MoviesContainer"], query),
                Books = await _repository.QueryAsync<BookEntity>(_configuration["Cosmos:BooksContainer"], query)                
            };

            query = new QueryDefinition(query: "SELECT * FROM c WHERE c.Type = 'subscription'");
            data.Subscriptions = await _repository.QueryAsync<SubscriptionEntity>(_configuration["Cosmos:ReaderContainer"], query);

            var client = new BlobServiceClient(_configuration["BlobStorage:ConnectionString"]);
            BlobContainerClient container = client.GetBlobContainerClient(_configuration["BlobStorage:BackupContainer"]);
            BlobClient blob = container.GetBlobClient($"{DateTime.UtcNow:yyyy-MM-dd}.json");

            await blob.UploadAsync(BinaryData.FromString(JsonConvert.SerializeObject(data)), overwrite: true);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Backup task failed");
        }
    }

    public class BackupData
    {
        public List<TVShowEntity> Shows { get; set; } = null!;

        public List<MovieEntity> Movies { get; set; } = null!;

        public List<BookEntity> Books { get; set; } = null!;

        public List<SubscriptionEntity> Subscriptions { get; set; } = null!;
    }
}
