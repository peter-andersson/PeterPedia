using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using PeterPedia.Server.Data;
using PeterPedia.Shared;
using PeterPedia.Server.Data.Models;
using Microsoft.Extensions.Configuration;
using System.IO;
using PeterPedia.Server.Services;

namespace PeterPedia.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VideoController : Controller
    {
        private readonly ILogger<VideoController> _logger;
        private readonly PeterPediaContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly IFileService _fileService;

        public VideoController(ILogger<VideoController> logger, PeterPediaContext dbContext, IConfiguration configuration, IFileService fileService)
        {
            _logger = logger;
            _dbContext = dbContext;
            _configuration = configuration;
            _fileService = fileService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            _logger.LogDebug("Get videos");
            var items = await _dbContext.Videos.ToListAsync().ConfigureAwait(false);

            var result = new List<Video>(items.Count);
            foreach (var item in items)
            {
                result.Add(ConvertToItem(item));
            }

            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogDebug($"Delete id: {id}");

            var item = await _dbContext.Videos.Where(v => v.Id == id).AsTracking().SingleOrDefaultAsync().ConfigureAwait(false);

            if (item is null)
            {
                return NotFound();
            }

            try
            {
                _fileService.Delete(item.AbsolutePath);

                _dbContext.Videos.Remove(item);

                await _dbContext.SaveChangesAsync().ConfigureAwait(false);
            }
            catch (IOException ioe)
            {
                _logger.LogError(ioe, "Failed to delete video.");

                return StatusCode(500);
            }

            return Ok();
        }

        private Video ConvertToItem(VideoEF itemEF)
        {
            if (itemEF is null)
            {
                throw new ArgumentNullException(nameof(itemEF));
            }

            var video = new Video()
            {
                Id = itemEF.Id,
                Title = itemEF.Title,
                Duration = itemEF.Duration,
                Type = itemEF.Type,
            };

            var basePath = _configuration["VideoPath"] ?? "/video";
            var relativePath = Path.GetRelativePath(basePath, itemEF.AbsolutePath);

            video.Url = "/video/" + relativePath;

            return video;
        }
    }
}
