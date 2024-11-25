using System.Net;
using System.Net.Http.Json;
using AgileMinds.Shared.Models;
using AgileMindsUI.Client.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Task = System.Threading.Tasks.Task;


namespace SmartSprint.Tests.Services
{
    public class SprintServiceTests
    {
        private readonly Mock<ILogger<SprintService>> _loggerMock;

        public SprintServiceTests()
        {
            _loggerMock = new Mock<ILogger<SprintService>>();
        }

        // Nested MockHttpMessageHandler class
        private class MockHttpMessageHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _sendAsync;

            public MockHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync)
            {
                _sendAsync = sendAsync ?? throw new ArgumentNullException(nameof(sendAsync));
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return _sendAsync(request, cancellationToken);
            }
        }

        [Fact]
        public async Task GetOpenSprint_WhenSprintExists_ShouldReturnSprint()
        {
            // Arrange
            var expectedSprint = new Sprint { Id = 1, Name = "Sprint 1" };
            var mockHandler = new MockHttpMessageHandler((request, cancellationToken) =>
            {
                return Task.FromResult(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = JsonContent.Create(expectedSprint)
                });
            });

            var httpClient = new HttpClient(mockHandler)
            {
                BaseAddress = new Uri("http://localhost/")
            };

            var loggerMock = new Mock<ILogger<SprintService>>();
            var sprintService = new SprintService(httpClient, loggerMock.Object);

            // Act
            var result = await sprintService.GetOpenSprint(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedSprint.Id, result.Id);
            Assert.Equal(expectedSprint.Name, result.Name);
        }

        [Fact]
        public async Task GetCompletedSprints_WhenNoSprints_ShouldReturnEmptyList()
        {
            // Arrange
            var mockHandler = new MockHttpMessageHandler((request, cancellationToken) =>
                Task.FromResult(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NoContent
                }));

            var httpClient = new HttpClient(mockHandler)
            {
                BaseAddress = new Uri("http://localhost/")
            };
            var sprintService = new SprintService(httpClient, _loggerMock.Object);

            // Act
            var result = await sprintService.GetCompletedSprints(1);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task CreateSprint_WhenConflict_ShouldReturnErrorMessage()
        {
            // Arrange
            var errorMessage = "Sprint already exists.";
            var mockHandler = new MockHttpMessageHandler((request, cancellationToken) =>
                Task.FromResult(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Conflict,
                    Content = new StringContent(errorMessage)
                }));

            var httpClient = new HttpClient(mockHandler)
            {
                BaseAddress = new Uri("http://localhost/")
            };
            var sprintService = new SprintService(httpClient, _loggerMock.Object);

            // Act
            var result = await sprintService.CreateSprint(1, new Sprint());

            // Assert
            Assert.False(result.Success);
            Assert.Equal(errorMessage, result.ErrorMessage);
        }

        [Fact]
        public async Task StartSprint_WhenSuccess_ShouldReturnTrue()
        {
            // Arrange
            var mockHandler = new MockHttpMessageHandler((request, cancellationToken) =>
                Task.FromResult(new HttpResponseMessage { StatusCode = HttpStatusCode.OK }));

            var httpClient = new HttpClient(mockHandler)
            {
                BaseAddress = new Uri("http://localhost/")
            };
            var sprintService = new SprintService(httpClient, _loggerMock.Object);

            // Act
            var result = await sprintService.StartSprint(1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task CompleteSprint_WhenFails_ShouldReturnFalse()
        {
            // Arrange
            var mockHandler = new MockHttpMessageHandler((request, cancellationToken) =>
                Task.FromResult(new HttpResponseMessage { StatusCode = HttpStatusCode.InternalServerError }));

            var httpClient = new HttpClient(mockHandler)
            {
                BaseAddress = new Uri("http://localhost/")
            };
            var sprintService = new SprintService(httpClient, _loggerMock.Object);

            // Act
            var result = await sprintService.CompleteSprint(1);

            // Assert
            Assert.False(result);
        }
    }
}