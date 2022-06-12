namespace PeterPedia.Services;

public interface IFileService
{
    void Delete(string path);

    Task DownloadImageAsync(string url, string filename);

    bool FileExists(string filename);
}
