using System.IO;

namespace PeterPedia.Server.Services
{
    public interface IFileService
    {
        void Delete(string path);
    }

    public class FileService : IFileService
    {
        public void Delete(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}