using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PeterPedia.Server.Data;
using PeterPedia.Server.Data.Models;
using PeterPedia.Server.Services.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Diagnostics;
using PeterPedia.Shared.Services.Models;
using System.Collections.Generic;

namespace PeterPedia.Server.Services
{
    internal class VideoService
    {
        private readonly ILogger<VideoService> _logger;
        private readonly PeterPediaContext _dbContext;
        private readonly string _basePath;

        private List<string> videoFilesFound = new List<string>();

        public VideoService(ILogger<VideoService> logger, PeterPediaContext dbContext, IConfiguration configuration)
        {
            _logger = logger;
            _dbContext = dbContext;

            _basePath = configuration["VideoPath"] ?? "/video";
        }

        public async Task DoWork(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Execute();

                await Task.Delay(TimeSpan.FromMinutes(60), stoppingToken);
            }
        }

        private async Task Execute()
        {
            videoFilesFound.Clear();

            var videos = await _dbContext.Videos.AsNoTracking().ToListAsync();

            await ListFiles(_basePath);

            foreach (var video in videos)
            {
                if (videoFilesFound.IndexOf(video.AbsolutePath) == -1)
                {
                    _logger.LogDebug($"Removing video {video.AbsolutePath}");
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

            foreach (var subDir in Directory.GetDirectories(directory))
            {
                await ListFiles(subDir);
            }
        }

        private async Task ReadVideoInfo(string file)
        {
            var tmp = GetMediaInfo(file);

            if (tmp is null)
            {
                _logger.LogInformation($"Failed to load media info for {file}");
                return;
            }

            MediaInfo mediaInfo = tmp;

            var test = Path.GetRelativePath(_basePath, file);
            string dir = Path.GetDirectoryName(test) ?? string.Empty;

            try
            {
                VideoEF video = new VideoEF()
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
                        _logger.LogInformation($"Invalid format in file {file}");
                        return;
                }

                videoFilesFound.Add(file);

                var existing = await _dbContext.Videos.Where(v => v.AbsolutePath == file).SingleOrDefaultAsync();

                if (existing is null)
                {
                    _logger.LogInformation($"Adding new video {video.AbsolutePath}");
                    _dbContext.Videos.Add(video);
                }
                else
                {
                    if (video.Title != existing.Title)
                    {
                        _logger.LogInformation($"Updating title on video {video.AbsolutePath}");
                        existing.Title = video.Title;
                    }

                    if (video.Duration != existing.Duration)
                    {
                        _logger.LogInformation($"Updating duration on video {video.AbsolutePath}");
                        existing.Duration = video.Duration;
                    }

                    if (video.Type != existing.Type)
                    {
                        _logger.LogInformation($"Updating type on video {video.AbsolutePath}");
                        existing.Type = video.Type;
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "ReadVideoInfo");
            }
        }

        private MediaInfo? GetMediaInfo(string file)
        {
            try
            {
                Process p = new Process();
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.FileName = "/usr/bin/mediainfo";
                p.StartInfo.Arguments = $"--output=JSON \"{file}\"";

                p.Start();

                string output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();

                var mediaInfo = System.Text.Json.JsonSerializer.Deserialize<MediaData>(output);
                if (mediaInfo?.Media is not null)
                {
                    MediaInfo info = new MediaInfo();

                    foreach (var track in mediaInfo.Media.Track)
                    {
                        if (track.Type == "General")
                        {
                            info.Title = Path.GetFileNameWithoutExtension(file);
                            info.FileExtension = Path.GetExtension(file).Substring(1);
                            info.Duration = track.DurationTimeSpan;
                        }
                    }

                    return info;
                }
            }
            catch (Exception e)
            {
                _logger.LogInformation(e.ToString());
                _logger.LogError(e, "Failed to load info from file.");
            }

            return null;
        }
    }
}