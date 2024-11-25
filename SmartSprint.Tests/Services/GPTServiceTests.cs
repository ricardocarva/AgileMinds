using System.Net;
using AgileMindsUI.Client.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Task = System.Threading.Tasks.Task;

namespace SmartSprint.Tests.Services
{
    public class GPTServiceTests
    {
        private Mock<ILogger<GPTService>> _loggerMock;

        public GPTServiceTests()
        {
            _loggerMock = new Mock<ILogger<GPTService>>();
        }

        private GPTService CreateService(HttpMessageHandler handler)
        {
            var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
            return new GPTService(httpClient, _loggerMock.Object);
        }

        [Fact]
        public async Task AskGptAsync_WhenRequestIsSuccessful_ShouldReturnResponse()
        {
            // Arrange
            var question = "What is GPT?";
            var expectedResponse = "GPT is a language model.";

            var mockHandler = new MockHttpMessageHandler(request =>
            {
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(expectedResponse)
                };
            });

            var service = CreateService(mockHandler);

            // Act
            var result = await service.AskGptAsync(question);

            // Assert
            Assert.Equal(expectedResponse, result);
        }

        [Fact]
        public async Task AskGptAsync_WhenResponseIsError_ShouldLogErrorAndReturnErrorMessage()
        {
            // Arrange
            var question = "What is GPT?";
            var errorDetails = "Bad Request";

            var mockHandler = new MockHttpMessageHandler(request =>
            {
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent(errorDetails)
                };
            });

            var service = CreateService(mockHandler);

            // Act
            var result = await service.AskGptAsync(question);

            // Assert
            Assert.Contains("Error occurred", result);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error fetching GPT response")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task AskGptAsync_WhenHttpRequestFails_ShouldLogErrorAndReturnErrorMessage()
        {
            // Arrange
            var question = "What is GPT?";

            var mockHandler = new MockHttpMessageHandler(request =>
                throw new HttpRequestException("Simulated network error"));

            var service = CreateService(mockHandler);

            // Act
            var result = await service.AskGptAsync(question);

            // Assert
            Assert.Equal("Error occurred while communicating with GPT service.", result);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("HTTP request failed")),
                    It.IsAny<HttpRequestException>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void AskGptAsync_WhenQuestionIsNullOrEmpty_ShouldThrowArgumentException()
        {
            // Arrange
            var mockHandler = new MockHttpMessageHandler(request => new HttpResponseMessage(HttpStatusCode.OK));
            var service = CreateService(mockHandler);

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(() => service.AskGptAsync(null!));
            Assert.ThrowsAsync<ArgumentException>(() => service.AskGptAsync(""));
            Assert.ThrowsAsync<ArgumentException>(() => service.AskGptAsync("   "));
        }

        [Fact]
        public async Task AskGptAsync_WhenResponseHasNonSuccessStatusCode_ShouldReturnDetailedErrorMessage()
        {
            // Arrange
            var question = "What is GPT?";
            var errorMessage = "Internal Server Error";

            var mockHandler = new MockHttpMessageHandler(request =>
            {
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent(errorMessage),
                    ReasonPhrase = "Internal Server Error"
                };
            });

            var service = CreateService(mockHandler);

            // Act
            var result = await service.AskGptAsync(question);

            // Assert
            Assert.Contains("Error occurred", result);
            Assert.Contains("Internal Server Error", result);
        }
    }
}
