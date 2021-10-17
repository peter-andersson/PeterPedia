using System;
using Xunit;
using System.Threading.Tasks;
using PeterPedia.Client.Book.Services;
using PeterPedia.Shared;
using Moq;
using System.Net.Http;
using System.Threading;
using System.Net;
using Moq.Protected;
using Blazored.Toast;
using Blazored.Toast.Services;
using PeterPedia.Client.Bookmark.Services;

namespace PeterPedia.Tests.Client.Bookmark
{
    public class ReadListServiceTests
    {
        private readonly string baseAddress = "https://example.com/";

        private readonly IToastService _toastService;

        public ReadListServiceTests()
        {
            _toastService = new Mock<IToastService>().Object;
        }

        [Fact]
        public async Task FetchData_ShouldCallApi()
        {
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent("[{\"id\":1,\"url\":\"https://lpda.se\",\"added\":\"2021-01-01T00:00:00Z\"}]"),
               })
               .Verifiable();

            // use real http client with mocked handler here
            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri(baseAddress),
            };

            var subjectUnderTest = new ReadListService(httpClient, _toastService);

            // ACT
            await subjectUnderTest.FetchData();

            // ASSERT
            Assert.NotNull(subjectUnderTest.Items);
            Assert.Single(subjectUnderTest.Items);
            var item = subjectUnderTest.Items[0];
            Assert.NotNull(item);
            Assert.Equal(1, item.Id);
            Assert.Equal("https://lpda.se", item.Url);

            // also check the 'http' call was like we expected it
            var expectedUri = new Uri($"{baseAddress}api/ReadList");

            handlerMock.Protected().Verify(
               "SendAsync",
               Times.Exactly(1), // we expected a single external request
               ItExpr.Is<HttpRequestMessage>(req =>
                  req.Method == HttpMethod.Get  // we expected a GET request
                  && req.RequestUri == expectedUri // to this uri
               ),
               ItExpr.IsAny<CancellationToken>()
            );
        }
    }
}
