using System;
using Xunit;
using PeterPedia.Server.Controllers;
using PeterPedia.Server.Data;
using System.Threading.Tasks;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using PeterPedia.Server.Data.Models;
using System.Collections.Generic;
using PeterPedia.Shared;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;
using PeterPedia.Server.Services;

namespace PeterPedia.Tests.Server
{
    public class VideoControllerTests
    {
        private readonly ILogger<VideoController> _logger;

        private readonly Mock<IFileService> _fileServiceMock;

        public VideoControllerTests()
        {
            _logger = Mock.Of<ILogger<VideoController>>();
            _fileServiceMock = new Mock<IFileService>();
        }

        [Fact]
        public async Task Get_WithoutItemsShouldReturnEmptyList()
        {
            var options = GetDbContextOptions();

            using var context = new PeterPediaContext(options);

            var controller = CreateController(context);

            var result = await controller.Get();
            var okResult = result as OkObjectResult;
            var items = okResult.Value as List<Video>;

            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);
            Assert.NotNull(items);
            Assert.Empty(items);
        }

        [Fact]
        public async Task Get_WithItemsShouldReturnListWithItems()
        {
            var options = GetDbContextOptions();

            AddTestData(options);

            using var context = new PeterPediaContext(options);

            var controller = CreateController(context);

            var result = await controller.Get();
            var okResult = result as OkObjectResult;
            var items = okResult.Value as List<Video>;

            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);
            Assert.NotNull(items);
            Assert.Single(items);

            var video = items[0];
            Assert.Equal(1, video.Id);
            Assert.Equal("/video/test/file.mp4", video.Url);
            Assert.Equal("file", video.Title);
            Assert.Equal(TimeSpan.FromMinutes(30), video.Duration);
            Assert.Equal("video/mp4", video.Type);
        }

        [Fact]
        public async Task Delete_NotFoundShouldReturn404()
        {
            var options = GetDbContextOptions();

            using var context = new PeterPediaContext(options);

            var controller = CreateController(context);

            var result = await controller.Delete(0);
            var notFoundResult = result as NotFoundResult;

            Assert.NotNull(notFoundResult);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task Delete_ItemFoundShouldRemoveItem()
        {
            var options = GetDbContextOptions();

            AddTestData(options);

            using var context = new PeterPediaContext(options);

            var controller = CreateController(context);

            var result = await controller.Delete(1);
            var okResult = result as OkResult;

            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);

            var item = context.Videos.Where(r => r.Id == 1).SingleOrDefault();
            Assert.Null(item);

            _fileServiceMock.Verify(m => m.Delete(It.IsAny<string>()));
        }

        private DbContextOptions<PeterPediaContext> GetDbContextOptions([CallerMemberName] string callingMember = null)
        {
            return new DbContextOptionsBuilder<PeterPediaContext>()
                .UseInMemoryDatabase(databaseName: $"{ nameof(VideoControllerTests) }.{callingMember}")
                .Options;
        }

        private VideoController CreateController(PeterPediaContext context)
        {
            var inMemorySettings = new Dictionary<string, string> {
                {"VideoPath", "/"},
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            return new VideoController(_logger, context, configuration, _fileServiceMock.Object);
        }

        private static void AddTestData(DbContextOptions<PeterPediaContext> options)
        {
            using var context = new PeterPediaContext(options);

            context.Videos.Add(new VideoEF
            {
                Id = 1,
                AbsolutePath = "/test/file.mp4",
                Directory = "/test",
                Duration = TimeSpan.FromMinutes(30),
                FileName = "file.mp4",
                Title = "file",
                Type = "video/mp4",
            });

            context.SaveChanges();
        }
    }
}
