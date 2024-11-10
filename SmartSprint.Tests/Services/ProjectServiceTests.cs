using AgileMindsUI.Client.Services;
using AgileMindsUI.Client.Models;
using Moq;
using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using System.Linq;
using Newtonsoft.Json;


using AgileMinds.Shared.Models;

using AgileMindsUI.Client.Services;

using Moq;

public class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, HttpResponseMessage> _responseGenerator;

    public MockHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responseGenerator)
    {
        _responseGenerator = responseGenerator;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Use the provided function to generate the response
        return System.Threading.Tasks.Task.FromResult(_responseGenerator(request));
    }
}

namespace SmartSprint.Tests.Services
{
    public class ProjectServiceTests
    {

        private ProjectService CreateService(Func<HttpRequestMessage, HttpResponseMessage> responseGenerator, out Mock<ILogger<ProjectService>> loggerMock)
        {
            var mockHandler = new MockHttpMessageHandler(responseGenerator);
            var httpClient = new HttpClient(mockHandler)
            {
                BaseAddress = new Uri("http://localhost/")
            };
            loggerMock = new Mock<ILogger<ProjectService>>();
            return new ProjectService(httpClient, loggerMock.Object);
        }

        [Fact]
        public void SetSelectedProject_WhenProjectIsNull_ShouldSetSelectedProjectToNull()
        {
            // Arrange
            var service = CreateService(request => new HttpResponseMessage(HttpStatusCode.OK), out _);
            Project? project = null;

            // Act
            service.SetSelectedProject(project);

            // Assert
            Assert.Null(service.SelectedProject);
        }

        [Fact]
        public void SetSelectedProject_WhenValidProjectProvided_ShouldSetSelectedProject()
        {
            // Arrange
            var service = CreateService(request => new HttpResponseMessage(HttpStatusCode.OK), out _);
            var project = new Project { Id = 1, Name = "Test Project" };

            // Act
            service.SetSelectedProject(project);

            // Assert
            Assert.Equal(project, service.SelectedProject);
        }

        [Fact]
        public async System.Threading.Tasks.Task SetSelectedProjectById_WhenProjectIdIsValid_ShouldSetSelectedProjectAndLogInfo()
        {
            // Arrange
            var projectId = 1;
            var expectedProject = new Project 
            {
                Id = projectId,
                Name = "Fetched Project",
                Description = null, // Default is null for strings
                GameifiedApp = false,
                DiscordIntegration = false,
                CanvasIntegration = false,
                CreatedAt = DateTime.MinValue, // Default for DateTime
                CreatedBy = 0,
                Members = new List<ProjectMember>(), // Default to an empty list
                Tasks = new List<AgileMindsUI.Client.Models.Task>() // Default to an empty list
            };

            var service = CreateService(request =>
            {
                if (request.RequestUri?.ToString() == $"http://localhost/api/projects/{projectId}")
                {
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = JsonContent.Create(expectedProject)
                    };
                }
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }, out var loggerMock);

            // Act
            var result = await service.SetSelectedProjectById(projectId);

            // Assert
            Assert.True(result);
            Assert.NotNull(service.SelectedProject);
            Assert.Equal(expectedProject.Id, service.SelectedProject!.Id);
            Assert.Equal(expectedProject.Name, service.SelectedProject.Name);
            Assert.Equal(expectedProject.Description, service.SelectedProject.Description);
            Assert.Equal(expectedProject.GameifiedApp, service.SelectedProject.GameifiedApp);
            Assert.Equal(expectedProject.DiscordIntegration, service.SelectedProject.DiscordIntegration);
            Assert.Equal(expectedProject.CanvasIntegration, service.SelectedProject.CanvasIntegration);
            Assert.Equal(expectedProject.CreatedAt, service.SelectedProject.CreatedAt);
            Assert.Equal(expectedProject.CreatedBy, service.SelectedProject.CreatedBy);
            Assert.Equal(expectedProject.Members, service.SelectedProject.Members);
            Assert.Equal(expectedProject.Tasks, service.SelectedProject.Tasks);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v != null && v.ToString()!.Contains("successfully fetched")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v != null && v.ToString()!.Contains("set as SelectedProject")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async System.Threading.Tasks.Task SetSelectedProjectById_WhenProjectIdIsInvalid_ShouldReturnFalseAndLogWarning()
        {
            // Arrange
            var projectId = 999;
            var service = CreateService(request => new HttpResponseMessage(HttpStatusCode.NotFound), out var loggerMock);

            // Act
            var result = await service.SetSelectedProjectById(projectId);

            // Assert
            Assert.False(result);
            Assert.Null(service.SelectedProject);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v != null && v.ToString()!.Contains("Failed to set SelectedProject")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async System.Threading.Tasks.Task FetchProjectById_WhenProjectIdIsValid_ShouldReturnProject()
        {
            // Arrange
            var projectId = 1;
            var expectedProject = new Project { Id = projectId, Name = "Fetched Project" };

            var service = CreateService(request =>
            {
                if (request.RequestUri?.ToString() == $"http://localhost/api/projects/{projectId}")
                {
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = JsonContent.Create(expectedProject)
                    };
                }
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }, out _);

            // Act
            var result = await service.FetchProjectById(projectId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedProject.Id, result.Id);
            Assert.Equal(expectedProject.Name, result.Name);
        }

        [Fact]
        public async System.Threading.Tasks.Task FetchProjectById_WhenProjectNotFound_ShouldReturnNullAndLogWarning()
        {
            // Arrange
            var projectId = 999;
            var service = CreateService(request => new HttpResponseMessage(HttpStatusCode.NotFound), out var loggerMock);

            // Act
            var result = await service.FetchProjectById(projectId);

            // Assert
            Assert.Null(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v != null && v.ToString()!.Contains("Project not found for ID:")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
