namespace PeterPedia.Services;

public class FileService : IFileService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<FileService> _logger;

    public FileService(ILogger<FileService> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    public void Delete(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    public bool FileExists(string filename) => File.Exists(filename);

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

        LogMessage.DownloadImage(_logger, url, filename);

        try
        {
            using HttpResponseMessage response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            using var fs = new FileStream(filename, FileMode.CreateNew);

            await response.Content.CopyToAsync(fs);
        }
        catch (Exception ex)
        {
            LogMessage.DownloadImageFailed(_logger, url, filename, ex);
        }
    }
}
