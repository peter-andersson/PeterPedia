using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using PeterPedia.Data.Interface;
using PeterPedia.Data.Models;

namespace PeterPedia.Data.Implementations;

public class BlobStorage : IFileStorage
{
    private readonly BlobServiceClient _serviceClient;
    private readonly BlobContainerClient _containerClient;


    public BlobStorage(IOptions<BlobStorageOptions> options)
    {
        if (string.IsNullOrEmpty(options.Value.ConnectionString))
        {
            throw new ArgumentException("Please specify a valid ConnectionString in the appSettings.json file or your Azure Functions Settings.");
        }

        if (string.IsNullOrEmpty(options.Value.Container))
        {
            throw new ArgumentException("Please specify a valid Container in the appSettings.json file or your Azure Functions Settings.");
        }

        _serviceClient = new BlobServiceClient(options.Value.ConnectionString);
        _containerClient = _serviceClient.GetBlobContainerClient(options.Value.Container);
    }

    public async Task UploadBlobAsync(string blob, Stream stream)
    {
        BlobClient blobClient = _containerClient.GetBlobClient(blob);

        await blobClient.DeleteIfExistsAsync();

        stream.Seek(0, SeekOrigin.Begin);
        await blobClient.UploadAsync(stream);
    }

    public async Task<bool> DownloadBlobAsync(string blob, Stream stream)
    {
        BlobClient blobClient = _containerClient.GetBlobClient(blob);

        if (await blobClient.ExistsAsync())
        {
            await blobClient.DownloadToAsync(stream);
            stream.Seek(0, SeekOrigin.Begin);

            return true;
        }
        else
        {
            return false;
        }     
    }
}
