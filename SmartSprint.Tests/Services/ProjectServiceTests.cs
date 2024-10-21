using AgileMinds.Shared.Models;

using AgileMindsUI.Client.Services;

using Moq;

namespace SmartSprint.Tests.Services
{
    public class ProjectServiceTests
    {

        private MockRepository mockRepository;
        private Mock<HttpClient> mockHttpClient;

        public ProjectServiceTests()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);
            this.mockHttpClient = this.mockRepository.Create<HttpClient>();
        }

        private ProjectService CreateService()
        {
            // Pass the mocked HttpClient to ProjectService
            return new ProjectService(this.mockHttpClient.Object);
        }

        [Fact]
        public void SetSelectedProject_WhenProjectIsNull_ShouldSetSelectedProjectToNull()
        {
            // Arrange
            var service = this.CreateService();
            Project project = null;

            // Act
            service.SetSelectedProject(
                project);

            // Assert
            Assert.Equal(project, service.GetSelectedProject());
            this.mockRepository.VerifyAll();
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
            this.mockRepository.VerifyAll();
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
            Assert.NotNull(result);  // Ensure the result is not null
            Assert.Equal(project.Id, result.Id);  // Check if the ProjectId matches
            Assert.Equal(project.Name, result.Name);  // Check if the ProjectName matches
            this.mockRepository.VerifyAll();
        }

        [Fact(Skip = "To be implemented")]
        public async System.Threading.Tasks.Task FetchProjectById_WhenProjectIdIsValid_ShouldReturnProject()
        {
            // Arrange
            var service = this.CreateService();
            int projectId = 0;

            // Act
            var result = await service.FetchProjectById(projectId);

            // Assert
            Assert.True(false); // Adjust with actual assertions later
            this.mockRepository.VerifyAll();
        }
    }
}
