namespace PeterPedia.Data.Interface;

public interface IFileStorage
{
    Task UploadAsync(string blob, Stream stream);

    Task<bool> DownloadAsync(string blob, Stream stream);

    Task<bool> ExistsAsync(string blob);
}
