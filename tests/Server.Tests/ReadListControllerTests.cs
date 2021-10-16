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

namespace PeterPedia.Server.Tests
{
    public class ReadListControllerTests
    {
        private readonly ILogger<ReadListController> _logger;

        public ReadListControllerTests()
        {
            _logger = Mock.Of<ILogger<ReadListController>>();
        }

        [Fact]
        public async Task Get_WithoutItemsShouldReturnEmptyList()
        {
            var options = new DbContextOptionsBuilder<PeterPediaContext>()
                .UseInMemoryDatabase(databaseName: $"{ nameof(ReadListControllerTests) }.{ nameof(Get_WithoutItemsShouldReturnEmptyList) }")
                .Options;

            using (var context = new PeterPediaContext(options))
            {
                ReadListController controller = new ReadListController(_logger, context);

                var result = await controller.Get();
                var okResult = result as OkObjectResult;
                var items = okResult.Value as List<ReadListItem>;

                Assert.NotNull(okResult);
                Assert.Equal(200, okResult.StatusCode);
                Assert.NotNull(items);
                Assert.Empty(items);
            }
        }

        [Fact]
        public async Task Get_WithItemsShouldReturnListWithItems()
        {
            var options = new DbContextOptionsBuilder<PeterPediaContext>()
                .UseInMemoryDatabase(databaseName: $"{ nameof(ReadListControllerTests) }.{ nameof(Get_WithItemsShouldReturnListWithItems) }")
                .Options;

            using (var context = new PeterPediaContext(options))
            {
                context.ReadListItems.Add(new ReadListEF
                {
                    Id = 1,
                    Url = "https://google.se",
                    Added = DateTime.Now,
                });

                context.ReadListItems.Add(new ReadListEF
                {
                    Id = 2,
                    Url = "https://norran.se",
                    Added = DateTime.Now,
                });

                context.SaveChanges();
            }

            using (var context = new PeterPediaContext(options))
            {
                ReadListController controller = new ReadListController(_logger, context);

                var result = await controller.Get();
                var okResult = result as OkObjectResult;
                var items = okResult.Value as List<ReadListItem>;

                Assert.NotNull(okResult);
                Assert.Equal(200, okResult.StatusCode);
                Assert.NotNull(items);
                Assert.Equal(2, items.Count);
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("test")]
        public async Task Add_WithInvalidDataShouldReturnBadRequest(string url)
        {
            var options = new DbContextOptionsBuilder<PeterPediaContext>()
                .UseInMemoryDatabase(databaseName: $"{ nameof(ReadListControllerTests) }.{ nameof(Add_WithInvalidDataShouldReturnBadRequest) }")
                .Options;

            using (var context = new PeterPediaContext(options))
            {
                ReadListController controller = new ReadListController(_logger, context);

                ReadListItem item = new ReadListItem()
                {
                    Id = 0,
                    Added = DateTime.Now,
                    Url = url
                };

                var result = await controller.Add(item);
                var badRequestResult = result as BadRequestResult;

                Assert.NotNull(badRequestResult);
                Assert.Equal(400, badRequestResult.StatusCode);
            }
        }

        [Theory]
        [InlineData("http://google.com")]
        [InlineData("https://norran.se")]
        public async Task Add_WithValidDataShouldAddItem(string url)
        {
            var options = new DbContextOptionsBuilder<PeterPediaContext>()
                .UseInMemoryDatabase(databaseName: $"{ nameof(ReadListControllerTests) }.{ nameof(Add_WithValidDataShouldAddItem) }")
                .Options;

            using (var context = new PeterPediaContext(options))
            {
                ReadListController controller = new ReadListController(_logger, context);

                ReadListItem item = new ReadListItem()
                {
                    Id = 0,
                    Added = DateTime.Now,
                    Url = url
                };

                var result = await controller.Add(item);
                var okResult = result as OkObjectResult;
                var addedItem = okResult.Value as ReadListItem;

                Assert.NotNull(okResult);
                Assert.Equal(200, okResult.StatusCode);

                Assert.NotNull(addedItem);
                Assert.Equal(item.Url, addedItem.Url);

                var dbItem = await context.ReadListItems.SingleOrDefaultAsync(r => r.Url == item.Url);
                Assert.NotNull(dbItem);
                Assert.Equal(addedItem.Id, dbItem.Id);
            }
        }

        [Fact]
        public async Task Add_SameItemTwiceShouldReturnConfliceted()
        {
            var options = new DbContextOptionsBuilder<PeterPediaContext>()
                .UseInMemoryDatabase(databaseName: $"{ nameof(ReadListControllerTests) }.{ nameof(Add_SameItemTwiceShouldReturnConfliceted) }")
                .Options;

            using (var context = new PeterPediaContext(options))
            {
                context.ReadListItems.Add(new ReadListEF
                {
                    Id = 1,
                    Url = "https://google.se",
                    Added = DateTime.Now,
                });

                context.ReadListItems.Add(new ReadListEF
                {
                    Id = 2,
                    Url = "https://norran.se",
                    Added = DateTime.Now,
                });

                context.SaveChanges();
            }

            using (var context = new PeterPediaContext(options))
            {
                ReadListController controller = new ReadListController(_logger, context);

                ReadListItem item = new ReadListItem()
                {
                    Id = 0,
                    Added = DateTime.Now,
                    Url = "https://norran.se"
                };

                var result = await controller.Add(item);
                var conflictResult = result is ConflictResult;
                Assert.True(conflictResult);
            }
        }

        [Fact]
        public async Task GetOpen_NotFoundShouldReturn404()
        {
            var options = new DbContextOptionsBuilder<PeterPediaContext>()
                                        .UseInMemoryDatabase(databaseName: $"{ nameof(ReadListControllerTests) }.{ nameof(GetOpen_NotFoundShouldReturn404) }")
                                        .Options;

            using (var context = new PeterPediaContext(options))
            {
                ReadListController controller = new ReadListController(_logger, context);

                var result = await controller.Open(0);
                var notFoundResult = result as NotFoundResult;

                Assert.NotNull(notFoundResult);
                Assert.Equal(404, notFoundResult.StatusCode);
            }
        }

        [Fact]
        public async Task GetOpen_ItemFoundShouldRemoveItemAndRedirectToUrl()
        {
            var options = new DbContextOptionsBuilder<PeterPediaContext>()
                                        .UseInMemoryDatabase(databaseName: $"{ nameof(ReadListControllerTests) }.{ nameof(GetOpen_ItemFoundShouldRemoveItemAndRedirectToUrl) }")
                                        .Options;

            using (var context = new PeterPediaContext(options))
            {
                context.ReadListItems.Add(new ReadListEF
                {
                    Id = 1,
                    Url = "https://google.se",
                    Added = DateTime.Now,
                });

                context.ReadListItems.Add(new ReadListEF
                {
                    Id = 2,
                    Url = "https://norran.se",
                    Added = DateTime.Now,
                });

                context.SaveChanges();
            }

            using (var context = new PeterPediaContext(options))
            {
                ReadListController controller = new ReadListController(_logger, context);

                var result = await controller.Open(1);
                var redirectResult = result as RedirectResult;

                Assert.NotNull(redirectResult);
                Assert.False(redirectResult.Permanent);
                Assert.Equal("https://google.se", redirectResult.Url);

                var item = context.ReadListItems.Where(r => r.Id == 1).SingleOrDefault();
                Assert.Null(item);
            }
        }

        [Fact]
        public async Task Delete_NotFoundShouldReturn404()
        {
            var options = new DbContextOptionsBuilder<PeterPediaContext>()
                                        .UseInMemoryDatabase(databaseName: $"{ nameof(ReadListControllerTests) }.{ nameof(Delete_NotFoundShouldReturn404) }")
                                        .Options;

            using (var context = new PeterPediaContext(options))
            {
                ReadListController controller = new ReadListController(_logger, context);

                var result = await controller.Delete(0);
                var notFoundResult = result as NotFoundResult;

                Assert.NotNull(notFoundResult);
                Assert.Equal(404, notFoundResult.StatusCode);
            }
        }

        [Fact]
        public async Task Delete_ItemFoundShouldRemoveItem()
        {
            var options = new DbContextOptionsBuilder<PeterPediaContext>()
                                        .UseInMemoryDatabase(databaseName: $"{ nameof(ReadListControllerTests) }.{ nameof(Delete_ItemFoundShouldRemoveItem) }")
                                        .Options;

            using (var context = new PeterPediaContext(options))
            {
                context.ReadListItems.Add(new ReadListEF
                {
                    Id = 1,
                    Url = "https://google.se",
                    Added = DateTime.Now,
                });

                context.ReadListItems.Add(new ReadListEF
                {
                    Id = 2,
                    Url = "https://norran.se",
                    Added = DateTime.Now,
                });

                context.SaveChanges();
            }

            using (var context = new PeterPediaContext(options))
            {
                ReadListController controller = new ReadListController(_logger, context);

                var result = await controller.Delete(1);
                var okResult = result as OkResult;

                Assert.NotNull(okResult);
                Assert.Equal(200, okResult.StatusCode);

                var item = context.ReadListItems.Where(r => r.Id == 1).SingleOrDefault();
                Assert.Null(item);
            }
        }
    }
}
