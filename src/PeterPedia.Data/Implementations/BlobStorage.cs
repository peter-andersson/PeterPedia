using Azure;
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

    public async Task<bool> ExistsAsync(string blob)
    {
        BlobClient blobClient = _containerClient.GetBlobClient(blob);

        return await blobClient.ExistsAsync();
    }

    public async Task UploadAsync(string blob, Stream stream)
    {
        BlobClient blobClient = _containerClient.GetBlobClient(blob);

        stream.Seek(0, SeekOrigin.Begin);
        await blobClient.UploadAsync(stream, overwrite: true);
    }

    public async Task<bool> DownloadAsync(string blob, Stream stream)
    {
        BlobClient blobClient = _containerClient.GetBlobClient(blob);

        try
        {
            await blobClient.DownloadToAsync(stream);
            stream.Seek(0, SeekOrigin.Begin);

            return true;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return false;
        }        
    }    
}
