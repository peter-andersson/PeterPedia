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

namespace PeterPedia.Tests.Server
{
    public class ArticleControllerTests
    {
        private readonly ILogger<ArticleController> _logger;

        public ArticleControllerTests()
        {
            _logger = Mock.Of<ILogger<ArticleController>>();
        }

        [Fact]
        public async Task Get_WithoutArticlesShouldReturnEmptyList()
        {
            var options = new DbContextOptionsBuilder<PeterPediaContext>()
                .UseInMemoryDatabase(databaseName: $"{ nameof(ArticleControllerTests) }.{ nameof(Get_WithoutArticlesShouldReturnEmptyList) }")
                .Options;

            using (var context = new PeterPediaContext(options))
            {
                ArticleController controller = new ArticleController(_logger, context);

                var result = await controller.Get();
                var okResult = result as OkObjectResult;
                var subscriptions = okResult.Value as List<Subscription>;

                Assert.NotNull(okResult);
                Assert.Equal(200, okResult.StatusCode);
                Assert.NotNull(subscriptions);
                Assert.Empty(subscriptions);
            }
        }

        [Fact]
        public async Task Get_WithArticlesShouldReturnSubscriptionListWithArticles()
        {
            var options = new DbContextOptionsBuilder<PeterPediaContext>()
                .UseInMemoryDatabase(databaseName: $"{ nameof(ArticleControllerTests) }.{ nameof(Get_WithArticlesShouldReturnSubscriptionListWithArticles) }")
                .Options;

            using (var context = new PeterPediaContext(options))
            {
                context.Subscriptions.Add(new SubscriptionEF
                {
                    Id = 1,
                    Title = "Test",
                });

                context.Articles.Add(new ArticleEF
                {
                    Id = 1,
                    SubscriptionId = 1,
                    Title = "Test",
                    Content = "Testar"
                });

                context.Articles.Add(new ArticleEF
                {
                    Id = 2,
                    SubscriptionId = 1,
                    Title = "Test 2",
                    Content = "Testar 2"
                });

                context.SaveChanges();
            }

            using (var context = new PeterPediaContext(options))
            {
                ArticleController controller = new ArticleController(_logger, context);

                var result = await controller.Get();
                var okResult = result as OkObjectResult;
                var subscriptions = okResult.Value as List<Subscription>;

                Assert.NotNull(okResult);
                Assert.Equal(200, okResult.StatusCode);
                Assert.NotNull(subscriptions);
                Assert.Single(subscriptions);
                Assert.Equal(2, subscriptions[0].Articles.Count);
            }
        }

        [Fact]
        public async Task GetHistory_WithoutArticlesShouldReturnEmptyList()
        {
            var options = new DbContextOptionsBuilder<PeterPediaContext>()
                .UseInMemoryDatabase(databaseName: $"{ nameof(ArticleControllerTests) }.{ nameof(GetHistory_WithoutArticlesShouldReturnEmptyList) }")
                .Options;

            using (var context = new PeterPediaContext(options))
            {
                ArticleController controller = new ArticleController(_logger, context);

                var result = await controller.History();
                var okResult = result as OkObjectResult;
                var articles = okResult.Value as List<Article>;

                Assert.NotNull(okResult);
                Assert.Equal(200, okResult.StatusCode);
                Assert.NotNull(articles);
                Assert.Empty(articles);
            }
        }

        [Fact]
        public async Task GetHistory_WithArticlesShouldReturnArticleList()
        {
            var options = new DbContextOptionsBuilder<PeterPediaContext>()
                .UseInMemoryDatabase(databaseName: $"{ nameof(ArticleControllerTests) }.{ nameof(GetHistory_WithArticlesShouldReturnArticleList) }")
                .Options;

            using (var context = new PeterPediaContext(options))
            {
                context.Subscriptions.Add(new SubscriptionEF
                {
                    Id = 1,
                    Title = "Test",
                });

                context.Articles.Add(new ArticleEF
                {
                    Id = 1,
                    SubscriptionId = 1,
                    Title = "Test",
                    Content = "Testar",
                    ReadDate = DateTime.Now,
                });

                context.Articles.Add(new ArticleEF
                {
                    Id = 2,
                    SubscriptionId = 1,
                    Title = "Test 2",
                    Content = "Testar 2",
                    ReadDate = DateTime.Now,
                });

                context.SaveChanges();
            }

            using (var context = new PeterPediaContext(options))
            {
                ArticleController controller = new ArticleController(_logger, context);

                var result = await controller.History();
                var okResult = result as OkObjectResult;
                var articles = okResult.Value as List<Article>;

                Assert.NotNull(okResult);
                Assert.Equal(200, okResult.StatusCode);
                Assert.NotNull(articles);
                Assert.Equal(2, articles.Count);
            }
        }

        [Fact]
        public async Task GetStats_WithoutArticlesShouldReturn0()
        {
            var options = new DbContextOptionsBuilder<PeterPediaContext>()
                            .UseInMemoryDatabase(databaseName: $"{ nameof(ArticleControllerTests) }.{ nameof(GetStats_WithoutArticlesShouldReturn0) }")
                            .Options;

            using (var context = new PeterPediaContext(options))
            {
                ArticleController controller = new ArticleController(_logger, context);

                var result = await controller.Stats();
                var okResult = result as OkObjectResult;
                var articleCount = okResult.Value as int?;

                Assert.NotNull(okResult);
                Assert.Equal(200, okResult.StatusCode);
                Assert.NotNull(articleCount);
                Assert.Equal(0, articleCount);
            }
        }

        [Fact]
        public async Task GetStats_WithArticlesShouldReturnTheArticleCount()
        {
            var options = new DbContextOptionsBuilder<PeterPediaContext>()
                            .UseInMemoryDatabase(databaseName: $"{ nameof(ArticleControllerTests) }.{ nameof(GetStats_WithArticlesShouldReturnTheArticleCount) }")
                            .Options;

            using (var context = new PeterPediaContext(options))
            {
                context.Subscriptions.Add(new SubscriptionEF
                {
                    Id = 1,
                    Title = "Test",
                });

                context.Articles.Add(new ArticleEF
                {
                    Id = 1,
                    SubscriptionId = 1,
                    Title = "Test",
                    Content = "Testar"
                });

                context.Articles.Add(new ArticleEF
                {
                    Id = 2,
                    SubscriptionId = 1,
                    Title = "Test 2",
                    Content = "Testar 2"
                });

                context.SaveChanges();
            }

            using (var context = new PeterPediaContext(options))
            {
                ArticleController controller = new ArticleController(_logger, context);

                var result = await controller.Stats();
                var okResult = result as OkObjectResult;
                var articleCount = okResult.Value as int?;

                Assert.NotNull(okResult);
                Assert.Equal(200, okResult.StatusCode);
                Assert.NotNull(articleCount);
                Assert.Equal(2, articleCount);
            }
        }

        [Fact]
        public async Task GetRead_ArticleNotFoundShouldReturn404()
        {
            var options = new DbContextOptionsBuilder<PeterPediaContext>()
                                        .UseInMemoryDatabase(databaseName: $"{ nameof(ArticleControllerTests) }.{ nameof(GetRead_ArticleNotFoundShouldReturn404) }")
                                        .Options;

            using (var context = new PeterPediaContext(options))
            {
                ArticleController controller = new ArticleController(_logger, context);

                var result = await controller.Read(0);
                var notFoundResult = result as NotFoundResult;

                Assert.NotNull(notFoundResult);
                Assert.Equal(404, notFoundResult.StatusCode);
            }
        }

        [Fact]
        public async Task GetRead_ArticleFoundShouldMarkArticleAsReadAndReturnOk()
        {
            var options = new DbContextOptionsBuilder<PeterPediaContext>()
                                        .UseInMemoryDatabase(databaseName: $"{ nameof(ArticleControllerTests) }.{ nameof(GetRead_ArticleFoundShouldMarkArticleAsReadAndReturnOk) }")
                                        .Options;

            using (var context = new PeterPediaContext(options))
            {
                context.Subscriptions.Add(new SubscriptionEF
                {
                    Id = 1,
                    Title = "Test",
                });

                context.Articles.Add(new ArticleEF
                {
                    Id = 1,
                    SubscriptionId = 1,
                    Title = "Test",
                    Content = "Testar"
                });

                context.SaveChanges();
            }

            using (var context = new PeterPediaContext(options))
            {
                ArticleController controller = new ArticleController(_logger, context);

                var result = await controller.Read(1);
                var okResult = result as OkResult;

                Assert.NotNull(okResult);
                Assert.Equal(200, okResult.StatusCode);

                var article = context.Articles.Where(a => a.Id == 1).SingleOrDefault();
                Assert.NotNull(article);
                Assert.NotNull(article.ReadDate);
            }
        }

        [Fact]
        public async Task GetOpen_ArticleNotFoundShouldReturn404()
        {
            var options = new DbContextOptionsBuilder<PeterPediaContext>()
                                        .UseInMemoryDatabase(databaseName: $"{ nameof(ArticleControllerTests) }.{ nameof(GetOpen_ArticleNotFoundShouldReturn404) }")
                                        .Options;

            using (var context = new PeterPediaContext(options))
            {
                ArticleController controller = new ArticleController(_logger, context);

                var result = await controller.Open(0);
                var notFoundResult = result as NotFoundResult;

                Assert.NotNull(notFoundResult);
                Assert.Equal(404, notFoundResult.StatusCode);
            }
        }

        [Fact]
        public async Task GetOpen_ArticleFoundShouldMarkArticleAsReadAndRedirectToUrl()
        {
            var options = new DbContextOptionsBuilder<PeterPediaContext>()
                                        .UseInMemoryDatabase(databaseName: $"{ nameof(ArticleControllerTests) }.{ nameof(GetOpen_ArticleFoundShouldMarkArticleAsReadAndRedirectToUrl) }")
                                        .Options;

            using (var context = new PeterPediaContext(options))
            {
                context.Subscriptions.Add(new SubscriptionEF
                {
                    Id = 1,
                    Title = "Test",
                });

                context.Articles.Add(new ArticleEF
                {
                    Id = 1,
                    SubscriptionId = 1,
                    Title = "Test",
                    Content = "Testar",
                    Url = "https://google.com"
                });

                context.SaveChanges();
            }

            using (var context = new PeterPediaContext(options))
            {
                ArticleController controller = new ArticleController(_logger, context);

                var result = await controller.Open(1);
                var redirectResult = result as RedirectResult;

                Assert.NotNull(redirectResult);
                Assert.False(redirectResult.Permanent);
                Assert.Equal("https://google.com", redirectResult.Url);

                var article = context.Articles.Where(a => a.Id == 1).SingleOrDefault();
                Assert.NotNull(article);
                Assert.NotNull(article.ReadDate);
            }
        }
    }
}
