using System.Net;
using System.Net.Http.Json;
using AgileMinds.Shared.Models;
using AgileMindsUI.Client.Services;
using Moq;
using Microsoft.Extensions.Logging;
using Task = System.Threading.Tasks.Task;
using System.Text.Json;


public class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, HttpResponseMessage> _responseGenerator;

    public MockHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responseGenerator)
    {
        _responseGenerator = responseGenerator;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(_responseGenerator(request));
    }
}

namespace SmartSprint.Tests.Services
{
    public class ProjectServiceTests
    {
        private Mock<HttpMessageHandler> _httpMessageHandler;
        private HttpClient _httpClient;
        private Mock<ILogger<ProjectService>> _loggerMock;

        public ProjectServiceTests()
        {
            _httpMessageHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpMessageHandler.Object)
            {
                BaseAddress = new Uri("http://localhost/")
            };
            _loggerMock = new Mock<ILogger<ProjectService>>();
        }

        private ProjectService CreateService()
        {
            return new ProjectService(_httpClient, _loggerMock.Object);
        }

        [Fact]
        public void SetSelectedProject_WhenProjectIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            var service = CreateService();
            Project? project = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => service.SetSelectedProject(project));
        }

        [Fact]
        public void SetSelectedProject_WhenValidProjectProvided_ShouldSetSelectedProject()
        {
            // Arrange
            var service = CreateService();
            var project = new Project { Id = 1, Name = "Test Project" };

            // Act
            service.SetSelectedProject(project);

            // Assert
            var selectedProject = service.GetSelectedProject();
            Assert.Equal(project, selectedProject);
        }

        [Fact]
        public void GetSelectedProject_WhenProjectIsSet_ShouldReturnSameProject()
        {
            var service = CreateService();
            var project = new Project { Id = 1, Name = "Test Project" };
            service.SetSelectedProject(project);

            // Act
            var result = service.GetSelectedProject();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(project.Id, result.Id);
            Assert.Equal(project.Name, result.Name);
        }

        [Fact]
        public async Task FetchProjectById_WhenProjectIdIsValid_ShouldReturnProject()
        {
            // Arrange
            var projectId = 1;
            var expectedProject = new Project { Id = projectId, Name = "Fetched Project" };

            var responseGenerator = new Func<HttpRequestMessage, HttpResponseMessage>(request =>
            {
                if (request.RequestUri!.ToString() == $"http://localhost/api/projects/{projectId}")
                {
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = JsonContent.Create(expectedProject)
                    };
                }
                return new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound };
            });

            var mockHandler = new MockHttpMessageHandler(responseGenerator);
            var httpClient = new HttpClient(mockHandler) { BaseAddress = new Uri("http://localhost/") };
            var service = new ProjectService(httpClient, _loggerMock.Object);

            // Act
            var result = await service.FetchProjectById(projectId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedProject.Id, result.Id);
            Assert.Equal(expectedProject.Name, result.Name);
        }

        [Fact]
        public async Task FetchProjectById_WhenProjectNotFound_ShouldReturnNull()
        {
            // Arrange
            var responseGenerator = new Func<HttpRequestMessage, HttpResponseMessage>(request =>
                new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound });

            var mockHandler = new MockHttpMessageHandler(responseGenerator);
            var httpClient = new HttpClient(mockHandler) { BaseAddress = new Uri("http://localhost/") };
            var service = new ProjectService(httpClient, _loggerMock.Object);
            int projectId = 999;

            // Act
            var result = await service.FetchProjectById(projectId);

            // Assert
            Assert.Null(result);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("not found")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task FetchProjectById_WhenHttpRequestFails_ShouldReturnNullAndLogError()
        {
            // Arrange
            var responseGenerator = new Func<HttpRequestMessage, HttpResponseMessage>(request =>
                throw new HttpRequestException("Simulated network error"));

            var mockHandler = new MockHttpMessageHandler(responseGenerator);
            var httpClient = new HttpClient(mockHandler) { BaseAddress = new Uri("http://localhost/") };
            var service = new ProjectService(httpClient, _loggerMock.Object);
            int projectId = 1;

            // Act
            var result = await service.FetchProjectById(projectId);

            // Assert
            Assert.Null(result);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("HTTP request failed")),
                    It.IsAny<HttpRequestException>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task FetchProjectById_WhenResponseIsInvalidJson_ShouldReturnNullAndLogError()
        {
            // Arrange
            var responseGenerator = new Func<HttpRequestMessage, HttpResponseMessage>(request =>
                new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("Invalid JSON")
                });

            var mockHandler = new MockHttpMessageHandler(responseGenerator);
            var httpClient = new HttpClient(mockHandler) { BaseAddress = new Uri("http://localhost/") };
            var service = new ProjectService(httpClient, _loggerMock.Object);
            int projectId = 1;

            // Act
            var result = await service.FetchProjectById(projectId);

            // Assert
            Assert.Null(result);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v != null && v.ToString()!.Contains("Failed to deserialize response content to type")),
                    It.IsAny<JsonException>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
