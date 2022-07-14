namespace PeterPedia.Data.Interface;

public interface IFileStorage
{
    Task UploadBlobAsync(string blob, Stream stream);

    Task<bool> DownloadBlobAsync(string blob, Stream stream);
}
