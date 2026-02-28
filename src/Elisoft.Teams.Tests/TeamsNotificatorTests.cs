using System.Net;
using System.Text;
using AutoFixture;
using Elisoft.Teams.Services;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Shouldly;

namespace Elisoft.Teams.Tests
{
    [TestFixture]
    public class TeamsNotificatorTests
    {
        private Fixture _fixture;
        private ILogger<TeamsNotificator> _logger;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _logger = A.Fake<ILogger<TeamsNotificator>>();
        }

        [Test]
        public async Task SendMessageAsync_WebhookUrlIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            var httpClient = CreateHttpClient(HttpStatusCode.OK);
            var sut = new TeamsNotificator(httpClient, _logger);

            // Act & Assert
            await Should.ThrowAsync<ArgumentException>(async () =>
                await sut.SendMessageAsync(null!, "Test Title", "msg"));
        }

        [Test]
        public async Task SendMessageAsync_WebhookUrlIsInvalid_ThrowsArgumentException()
        {
            // Arrange
            var httpClient = CreateHttpClient(HttpStatusCode.OK);
            var sut = new TeamsNotificator(httpClient, _logger);

            // Act & Assert
            await Should.ThrowAsync<ArgumentException>(async () =>
                await sut.SendMessageAsync("not-a-url", "Test Title", "msg"));
        }

        [Test]
        public async Task SendMessageAsync_MessageTextIsEmpty_ThrowsArgumentException()
        {
            // Arrange
            var httpClient = CreateHttpClient(HttpStatusCode.OK);
            var sut = new TeamsNotificator(httpClient, _logger);

            // Act & Assert
            await Should.ThrowAsync<ArgumentException>(async () =>
                await sut.SendMessageAsync("https://example.com", "Test Title", ""));
        }

        [Test]
        public async Task SendMessageAsync_ResponseIsSuccess_ReturnsTrue()
        {
            // Arrange
            var httpClient = CreateHttpClient(HttpStatusCode.OK);
            var sut = new TeamsNotificator(httpClient, _logger);
            var url = "https://example.com";
            var title = _fixture.Create<string>();
            var msg = _fixture.Create<string>();

            // Act
            var result = await sut.SendMessageAsync(url, title, msg);

            // Assert
            result.ShouldBeTrue();
        }

        [Test]
        public async Task SendMessageAsync_ResponseIsFailure_ReturnsFalse()
        {
            // Arrange
            var httpClient = CreateHttpClient(HttpStatusCode.BadRequest);
            var sut = new TeamsNotificator(httpClient, _logger);
            var url = "https://example.com";
            var title = _fixture.Create<string>();
            var msg = _fixture.Create<string>();

            // Act
            var result = await sut.SendMessageAsync(url, title, msg);

            // Assert
            result.ShouldBeFalse();
        }

        [Test]
        public async Task SendMessageAsync_HttpClientThrowsException_ReturnsFalse()
        {
            // Arrange
            var handler = A.Fake<HttpMessageHandler>();
            A.CallTo(handler)
                .Where(call => call.Method.Name == "SendAsync")
                .WithReturnType<Task<HttpResponseMessage>>()
                .ThrowsAsync(new HttpRequestException());

            var httpClient = new HttpClient(handler);
            var sut = new TeamsNotificator(httpClient, _logger);
            var url = "https://example.com";
            var title = _fixture.Create<string>();
            var msg = _fixture.Create<string>();

            // Act
            var result = await sut.SendMessageAsync(url, title, msg);

            // Assert
            result.ShouldBeFalse();
        }

        private static HttpClient CreateHttpClient(HttpStatusCode statusCode)
        {
            var handler = A.Fake<HttpMessageHandler>();

            A.CallTo(handler)
                .Where(call => call.Method.Name == "SendAsync")
                .WithReturnType<Task<HttpResponseMessage>>()
                .Returns(Task.FromResult(new HttpResponseMessage
                {
                    StatusCode = statusCode,
                    Content = new StringContent("response", Encoding.UTF8)
                }));

            return new HttpClient(handler);
        }
    }
}
