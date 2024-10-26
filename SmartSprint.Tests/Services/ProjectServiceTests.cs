using AgileMindsUI.Client.Services;
using AgileMindsUI.Client.Models;
using Moq;
using System.Net;
using System.Net.Http.Json;


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

        private Mock<HttpMessageHandler> _httpMessageHandler;
        private HttpClient _httpClient;


        public ProjectServiceTests()
        {
            _httpMessageHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpMessageHandler.Object)
            {
                BaseAddress = new Uri("http://localhost/")
            };
        }

        private ProjectService CreateService()
        {
            return new ProjectService(_httpClient);
        }

        [Fact]
        public void SetSelectedProject_WhenProjectIsNull_ShouldSetSelectedProjectToNull()
        {
            // Arrange
            var service = this.CreateService();
            Project? project = null;

            // Act
            service.SetSelectedProject(project);

            // Assert
            Assert.Null(service.GetSelectedProject());
        }

        [Fact]
        public void SetSelectedProject_WhenValidProjectProvided_ShouldSetSelectedProject()
        {
            // Arrange
            var service = this.CreateService();
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
            var service = this.CreateService();
            var project = new Project { Id = 1, Name = "Test Project" };
            service.SetSelectedProject(project);  // Set a project first

            // Act
            var result = service.GetSelectedProject();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(project.Id, result.Id);
            Assert.Equal(project.Name, result.Name);
        }

        [Fact]
        public async System.Threading.Tasks.Task FetchProjectById_WhenProjectIdIsValid_ShouldReturnProject()
        {
            // Arrange
            var projectId = 1;
            var expectedProject = new Project
            {
                Id = projectId,
                Name = "Fetched Project"
            };

            // Set up the mock handler to return a successful response with the expected project as content
            var responseGenerator = new Func<HttpRequestMessage, HttpResponseMessage>((request) =>
            {
                // Check if the request is null
                if (request == null || request.RequestUri == null)
                {
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.BadRequest // Or any other status you prefer for null requests
                    };
                }

                // Verify the request URL is correct
                if (request.RequestUri.ToString() == $"http://localhost/api/projects/{projectId}")
                {
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = JsonContent.Create(expectedProject)
                    };
                }

                // Return a not found response if the request doesn't match
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound
                };
            });

            // Create an instance of the custom handler with the response generator
            var mockHandler = new MockHttpMessageHandler(responseGenerator);
            var httpClient = new HttpClient(mockHandler)
            {
                BaseAddress = new Uri("http://localhost/")
            };

            var service = new ProjectService(httpClient);

            // Act
            var result = await service.FetchProjectById(projectId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedProject.Id, result.Id);
            Assert.Equal(expectedProject.Name, result.Name);
        }
    

        [Fact]
        public async System.Threading.Tasks.Task FetchProjectById_WhenProjectNotFound_ShouldReturnNull()
        {
            // Arrange
            var responseGenerator = new Func<HttpRequestMessage, HttpResponseMessage>((request) =>
            {
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound
                };
            });

            var mockHandler = new MockHttpMessageHandler(responseGenerator);
            var httpClient = new HttpClient(mockHandler)
            {
                BaseAddress = new Uri("http://localhost/")
            };

            var service = new ProjectService(httpClient);
            int projectId = 999; // Change to a positive unexistent number in database. 
                                 // 999 is used instead of 0 because it represents a realistic scenario where a valid,
                                 // non-existent ("Today") project ID is passed to the method. This approach ensures that the test
                                 // accurately verifies the method's behavior when it cannot find a matching project.

            // Act
            var result = await service.FetchProjectById(projectId);

            // Assert
            Assert.Null(result);
        }
    }
}
