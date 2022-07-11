using Azure;
using Azure.Storage.Blobs;

namespace Movies.Api.Data;

public class BlobStorage
{
    private readonly IHttpClientFactory _httpClientFactory;

    private readonly BlobContainerClient _containerClient;

    public BlobStorage(BlobServiceClient serviceClient, IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        _containerClient = serviceClient.GetBlobContainerClient("movies");
    }

    public async Task DownloadPosterUrlAsync(string id, string posterUrl)
    {
        HttpClient httpClient = _httpClientFactory.CreateClient();

        using HttpResponseMessage response = await httpClient.GetAsync(posterUrl);
        response.EnsureSuccessStatusCode();

        BlobClient blobClient = _containerClient.GetBlobClient(id);

        await blobClient.DeleteIfExistsAsync();

        using var stream = new MemoryStream();
        await response.Content.CopyToAsync(stream);

        stream.Seek(0, SeekOrigin.Begin);
        await blobClient.UploadAsync(stream);
    }

    public async Task GetPosterAsync(string id, MemoryStream stream)
    {
        BlobClient blobClient = _containerClient.GetBlobClient(id);

        try
        {
            await blobClient.DownloadToAsync(stream);
            stream.Seek(0, SeekOrigin.Begin);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            throw new FileNotFoundException("Blob file not found", id, ex);
        }
    }
}
