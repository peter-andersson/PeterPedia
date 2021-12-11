using PeterPedia.Server.Data;
using PeterPedia.Server.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using PeterPedia.Shared.Services.Models;
using System.Runtime.CompilerServices;

namespace PeterPedia.Server.Services;

internal partial class VideoService
{
    private readonly ILogger<VideoService> _logger;
    private readonly PeterPediaContext _dbContext;
    private readonly string _basePath;
    private readonly string _mediaInfo;

    private readonly List<string> videoFilesFound = new();

    public VideoService(ILogger<VideoService> logger, PeterPediaContext dbContext, IConfiguration configuration)
    {
        _logger = logger;
        _dbContext = dbContext;

        _basePath = configuration["VideoPath"] ?? "/video";
        _mediaInfo = configuration["MediaInfo"] ?? "/usr/bin/mediainfo";
    }

    public async Task DoWork(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Execute();

            // await Task.Delay(TimeSpan.FromMinutes(60), stoppingToken);
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }

    private async Task Execute()
    {
        videoFilesFound.Clear();

        var videos = await _dbContext.Videos.AsTracking().ToListAsync();

        await ListFiles(_basePath);

        foreach (var video in videos)
        {
            if (videoFilesFound.IndexOf(video.AbsolutePath) == -1)
            {
                LogRemoveVideo(video.AbsolutePath);
                _dbContext.Remove(video);
            }
        }

        await _dbContext.SaveChangesAsync();
    }

    private async Task ListFiles(string directory)
    {
        foreach (var file in Directory.GetFiles(directory))
        {
            await ReadVideoInfo(file);
        }
    }

    private async Task ReadVideoInfo(string file)
    {
        var tmp = GetMediaInfo(file);

        if (tmp is null)
        {
            LogMediaInfoNotLoaded(file);
            return;
        }

        MediaInfo mediaInfo = tmp;

        var test = Path.GetRelativePath(_basePath, file);
        string dir = Path.GetDirectoryName(test) ?? string.Empty;

        try
        {
            VideoEF video = new()
            {
                Directory = dir,
                FileName = Path.GetFileName(file),
                AbsolutePath = file,
                Title = mediaInfo.Title,
                Duration = mediaInfo.Duration
            };

            switch (mediaInfo.FileExtension.ToLowerInvariant())
            {
                case "mp4":
                    video.Type = "video/mp4";
                    break;
                case "m4v":
                    video.Type = "video/mp4";
                    break;
                case "ogg":
                    video.Type = "video/ogg";
                    break;
                case "  webm":
                    video.Type = "video/webm";
                    break;
                default:
                    LogInvalidVideoFormat(file);
                    return;
            }

            videoFilesFound.Add(file);

            var existing = await _dbContext.Videos.Where(v => v.AbsolutePath == file).AsTracking().SingleOrDefaultAsync();

            if (existing is null)
            {
                LogNewVideo(video.AbsolutePath);
                _dbContext.Videos.Add(video);
            }
            else
            {
                if (video.Title != existing.Title)
                {
                    LogUpdateTitle(video.Title, video.AbsolutePath);
                    existing.Title = video.Title;
                }

                if (video.Duration != existing.Duration)
                {
                    LogUpdateDuration(video.Duration, video.AbsolutePath);
                    existing.Duration = video.Duration;
                }

                if (video.Type != existing.Type)
                {
                    LogUpdateType(video.Type, video.AbsolutePath);
                    existing.Type = video.Type;
                }
            }
        }
        catch (Exception e)
        {
            LogException(e);
        }
    }

    private MediaInfo? GetMediaInfo(string file)
    {
        try
        {
            Process p = new();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = _mediaInfo;
            p.StartInfo.Arguments = $"--output=JSON \"{file}\"";

            p.Start();

            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            var mediaInfo = System.Text.Json.JsonSerializer.Deserialize<MediaData>(output);
            if (mediaInfo?.Media is not null)
            {
                MediaInfo info = new();

                foreach (var track in mediaInfo.Media.Track)
                {
                    if (track.Type == "General")
                    {
                        info.Title = Path.GetFileNameWithoutExtension(file);
                        info.FileExtension = Path.GetExtension(file)[1..];
                        info.Duration = track.DurationTimeSpan;
                    }
                }

                return info;
            }
        }
        catch (Exception e)
        {
            LogException(e);
        }

        return null;
    }

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable CA1822 // Mark members as static
#pragma warning disable IDE0060 // Remove unused parameter
    [LoggerMessage(0, LogLevel.Information, "Video with path '{path}' no longer exists. Removing video.")]
    partial void LogRemoveVideo(string path);

    [LoggerMessage(1, LogLevel.Information, "Failed to load media info for '{file}'")]
    partial void LogMediaInfoNotLoaded(string file);

    [LoggerMessage(2, LogLevel.Information, "Invalid format in file '{file}'")]
    partial void LogInvalidVideoFormat(string file);

    [LoggerMessage(3, LogLevel.Information, "Adding new video '{file}'")]
    partial void LogNewVideo(string file);

    [LoggerMessage(4, LogLevel.Debug, "Updating title to '{title}' on video '{file}'")]
    partial void LogUpdateTitle(string title, string file);

    [LoggerMessage(5, LogLevel.Debug, "Updating duration to '{duration}' on video '{file}'")]
    partial void LogUpdateDuration(TimeSpan duration, string file);

    [LoggerMessage(6, LogLevel.Debug, "Updating type to '{type}' on video '{file}'")]
    partial void LogUpdateType(string type, string file);

    [LoggerMessage(7, LogLevel.Error, "Exception in VideoService")]
    partial void LogException(Exception ex);
#pragma warning restore CA1822 // Mark members as static
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore IDE0079 // Remove unnecessary suppression
}