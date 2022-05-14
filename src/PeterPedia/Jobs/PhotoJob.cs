using Microsoft.EntityFrameworkCore;
using Quartz;

namespace PeterPedia.Jobs;

[DisallowConcurrentExecution]
public partial class PhotoJob : IJob
{
    private readonly ILogger<PhotoJob> _logger;
    private readonly PeterPediaContext _dbContext;
    private readonly string _basePath;

    private readonly List<string> _photoFilesFound = new();

    public PhotoJob(ILogger<PhotoJob> logger, PeterPediaContext dbContext, IConfiguration configuration)
    {
        _logger = logger;
        _dbContext = dbContext;

        _basePath = configuration["PhotoPath"] ?? "/photos";
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _photoFilesFound.Clear();

        List<PhotoEF> photos = await _dbContext.Photos.AsTracking().ToListAsync();

        await ListFilesAsync(_basePath);

        foreach (PhotoEF photo in photos)
        {
            if (_photoFilesFound.IndexOf(photo.AbsolutePath) == -1)
            {
                LogMessage.PhotoRemove(_logger, photo.AbsolutePath);
                _dbContext.Remove(photo);
            }
        }

        await _dbContext.SaveChangesAsync();
    }

    private async Task ListFilesAsync(string basePath)
    {
        var directories = Directory.GetDirectories(basePath);

        foreach (var dir in directories)
        {
            AlbumEF? album = await _dbContext.Albums.AsTracking().Where(a => a.Name == Path.GetFileName(dir)).SingleOrDefaultAsync();

            if (album is null)
            {
                album = new AlbumEF()
                {
                    Name = Path.GetFileName(dir),
                };

                LogMessage.PhotoNewAlbum(_logger, album.Name);
                _dbContext.Albums.Add(album);
            }

            var files = Directory.GetFiles(dir, "*.jpg");
            foreach (var file in files)
            {
                var photo = new PhotoEF()
                {
                    AbsolutePath = file,
                    FileName = Path.GetFileName(file),
                    Album = album
                };

                PhotoEF? existing = await _dbContext.Photos.Where(v => v.AbsolutePath == file).SingleOrDefaultAsync();
                if (existing is null)
                {
                    LogMessage.PhotoNew(_logger, photo.AbsolutePath);
                    _dbContext.Photos.Add(photo);
                }               

                _photoFilesFound.Add(file);
            }
        }
    }
}
