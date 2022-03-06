namespace PeterPedia.Server.Services;

public interface IFileService
{
    void Delete(string path);

    Task DownloadImageAsync(string url, string filename);
}

public class FileService : IFileService
{
    private readonly HttpClient _httpClient;

    public FileService(IHttpClientFactory httpClientFactory) => _httpClient = httpClientFactory.CreateClient();

    public void Delete(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    public async Task DownloadImageAsync(string url, string filename)
    {
        if (File.Exists(filename))
        {
            File.Delete(filename);
        }

        var dir = Path.GetDirectoryName(filename);
        if (string.IsNullOrEmpty(dir))
        {
            return;
        }

        Directory.CreateDirectory(dir);

        using HttpResponseMessage response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        using var fs = new FileStream(filename, FileMode.CreateNew);
        
        await response.Content.CopyToAsync(fs);        
    }
}
