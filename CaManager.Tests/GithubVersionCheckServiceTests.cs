using System.Net;
using System.Reflection;
using CaManager.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace CaManager.Tests
{
    public class GithubVersionCheckServiceTests
    {
        private readonly Mock<HttpMessageHandler> _msgHandlerMock;
        private readonly HttpClient _httpClient;
        private readonly Mock<IMemoryCache> _cacheMock;
        private readonly Mock<ILogger<GithubVersionCheckService>> _loggerMock;

        public GithubVersionCheckServiceTests()
        {
            _msgHandlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_msgHandlerMock.Object);
            _cacheMock = new Mock<IMemoryCache>();
            _loggerMock = new Mock<ILogger<GithubVersionCheckService>>();

            // Setup cache to always miss (return null and false) for default behavior, 
            // or we can setup extensions. Simple way is ensure it accepts calls.
            var cacheEntryMock = new Mock<ICacheEntry>();
            _cacheMock.Setup(m => m.CreateEntry(It.IsAny<object>())).Returns(cacheEntryMock.Object);
        }

        // Subclass to override GetCurrentVersion
        private class TestableGithubVersionCheckService : GithubVersionCheckService
        {
            private readonly string _mockVersion;

            public TestableGithubVersionCheckService(HttpClient httpClient, IMemoryCache cache, ILogger<GithubVersionCheckService> logger, string mockVersion = "0.0.0") 
                : base(httpClient, cache, logger)
            {
                _mockVersion = mockVersion;
            }

            protected override string GetCurrentVersion()
            {
                return _mockVersion;
            }
        }

        [Fact]
        public async Task CheckForUpdateAsync_ReturnsUpdateAvailable_WhenNewerVersionExists()
        {
            // Arrange
            // Mock current version is 0.0.0
            // Response: 1.0.0
            SetupMockResponse(HttpStatusCode.OK, "{\"tag_name\": \"v1.0.0\", \"html_url\": \"http://url\"}");

            var service = new TestableGithubVersionCheckService(_httpClient, _cacheMock.Object, _loggerMock.Object, "0.0.0");

            // Act
            var result = await service.CheckForUpdateAsync();

            // Assert
            Assert.True(result.IsUpdateAvailable);
            Assert.Equal("v1.0.0", result.LatestVersion);
        }

        [Fact]
        public async Task CheckForUpdateAsync_ReturnsNoUpdate_WhenVersionsMatch()
        {
            // Arrange
            SetupMockResponse(HttpStatusCode.OK, "{\"tag_name\": \"v1.0.0\", \"html_url\": \"http://url\"}");

            // Set current version to match remote
            var service = new TestableGithubVersionCheckService(_httpClient, _cacheMock.Object, _loggerMock.Object, "1.0.0");

            // Act
            var result = await service.CheckForUpdateAsync();

            // Assert
            Assert.False(result.IsUpdateAvailable);
            Assert.Equal("v1.0.0", result.LatestVersion);
        }

        [Fact]
        public async Task CheckForUpdateAsync_Handles404_Gracefully()
        {
            // Arrange
            SetupMockResponse(HttpStatusCode.NotFound, "");

            var service = new TestableGithubVersionCheckService(_httpClient, _cacheMock.Object, _loggerMock.Object);

            // Act
            var result = await service.CheckForUpdateAsync();

            // Assert
            Assert.False(result.IsUpdateAvailable);
            // Logger should have logged Info
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No releases found")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        private void SetupMockResponse(HttpStatusCode statusCode, string content)
        {
            _msgHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = statusCode,
                    Content = new StringContent(content)
                });
        }
    }
}
