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

namespace PeterPedia.Client.Book.Tests
{
    public class BookServiceTests
    {
        private readonly IToastService _toastService;

        public BookServiceTests()
        {
            _toastService = new Mock<IToastService>().Object;
        }

        [Fact]
        public async Task FetchAuthor_ShouldCallApi()
        {
            // ARRANGE
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
               .Protected()
               // Setup the PROTECTED method to mock
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               // prepare the expected response of the mocked http call
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent("[{\"id\":1,\"name\": \"Peter Andersson\"}]"),
               })
               .Verifiable();

            // use real http client with mocked handler here
            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("https://api.local.lpda.se/"),
            };

            var subjectUnderTest = new BookService(httpClient, _toastService);

            // ACT
            await subjectUnderTest.FetchAuthor();

            // ASSERT
            Assert.NotNull(subjectUnderTest.Authors);
            Assert.Single(subjectUnderTest.Authors);
            var author = subjectUnderTest.Authors[0];
            Assert.NotNull(author);
            Assert.Equal(1, author.Id);
            Assert.Equal("Peter Andersson", author.Name);

            // also check the 'http' call was like we expected it
            var expectedUri = new Uri("https://api.local.lpda.se/api/Author");

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
