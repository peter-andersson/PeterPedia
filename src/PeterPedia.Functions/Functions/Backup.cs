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
    private readonly IDataStorage<TVShowEntity> _tvStorage;
    private readonly IDataStorage<MovieEntity> _movieStorage;
    private readonly IDataStorage<BookEntity> _bookStorage;
    private readonly IDataStorage<SubscriptionEntity> _subscriptionStorage;

    public Backup(ILogger<Backup> log, IConfiguration configuration, IDataStorage<TVShowEntity> tvStorage, IDataStorage<MovieEntity> movieStorage, IDataStorage<BookEntity> bookStorage, IDataStorage<SubscriptionEntity> subscriptionStorage)
    {
        _log = log;
        _configuration = configuration;
        _tvStorage = tvStorage;
        _movieStorage = movieStorage;
        _bookStorage = bookStorage;
        _subscriptionStorage = subscriptionStorage;
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
                Shows = await _tvStorage.QueryAsync(query),
                Movies = await _movieStorage.QueryAsync(query),
                Books = await _bookStorage.QueryAsync(query)                
            };

            query = new QueryDefinition(query: "SELECT * FROM c WHERE c.Type = \"subscription\"");
            data.Subscriptions = await _subscriptionStorage.QueryAsync(query);

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
