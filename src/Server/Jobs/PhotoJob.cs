using Microsoft.EntityFrameworkCore;
using Quartz;

namespace PeterPedia.Server.Jobs;

[DisallowConcurrentExecution]
public partial class PhotoJob : IJob
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "Should not suppress this.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Used by source generator [LoggerMessaage]")]
    private readonly ILogger<PhotoJob> _logger;
    private readonly PeterPediaContext _dbContext;
    private readonly string _basePath;

    private readonly List<string> photoFilesFound = new();

    public PhotoJob(ILogger<PhotoJob> logger, PeterPediaContext dbContext, IConfiguration configuration)
    {
        _logger = logger;
        _dbContext = dbContext;

        _basePath = configuration["PhotoPath"] ?? "/photos";
    }

    public async Task Execute(IJobExecutionContext context)
    {
        photoFilesFound.Clear();

        List<PhotoEF> photos = await _dbContext.Photos.AsTracking().ToListAsync();

        await ListFiles(_basePath);

        foreach (PhotoEF photo in photos)
        {
            if (photoFilesFound.IndexOf(photo.AbsolutePath) == -1)
            {
                LogRemovePohoto(photo.AbsolutePath);
                _dbContext.Remove(photo);
            }
        }

        await _dbContext.SaveChangesAsync();
    }

    private async Task ListFiles(string basePath)
    {
        string[] directories = Directory.GetDirectories(basePath);

        foreach (string dir in directories)
        {
            AlbumEF? album = await _dbContext.Albums.Where(a => a.Name == Path.GetFileName(dir)).SingleOrDefaultAsync();

            if (album is null)
            {
                album = new AlbumEF()
                {
                    Name = Path.GetFileName(dir),
                };

                LogNewAlbum(album.Name);
                _dbContext.Albums.Add(album);
            }

            string[] files = Directory.GetFiles(dir, "*.jpg");
            foreach (string file in files)
            {
                var photo = new PhotoEF()
                {
                    AbsolutePath = file,
                    FileName = Path.GetFileName(file),
                    Album = album
                };

                var existing = await _dbContext.Photos.Where(v => v.AbsolutePath == file).SingleOrDefaultAsync();
                if (existing is null)
                {
                    LogNewPhoto(photo.AbsolutePath);
                    _dbContext.Photos.Add(photo);
                }               

                photoFilesFound.Add(file);
            }
        }
    }
    
#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable CA1822 // Mark members as static
#pragma warning disable IDE0060 // Remove unused parameter
    [LoggerMessage(0, LogLevel.Information, "Video with path '{path}' no longer exists. Removing video.")]
    partial void LogRemovePohoto(string path);

    [LoggerMessage(1, LogLevel.Information, "Adding new album '{album}'")]
    partial void LogNewAlbum(string album);

    [LoggerMessage(2, LogLevel.Information, "Adding new photo '{file}'")]
    partial void LogNewPhoto(string file);  
#pragma warning restore CA1822 // Mark members as static
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore IDE0079 // Remove unnecessary suppression
}