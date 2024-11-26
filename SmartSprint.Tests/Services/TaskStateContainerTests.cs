using System.Net;

using AgileMindsUI.Client.Services;

using Microsoft.JSInterop;

using Moq;
using Moq.Protected;

using Task = System.Threading.Tasks.Task;

namespace SmartSprint.Tests.Services
{
    public class TaskStateContainerTests
    {

        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;
        private readonly SprintStateContainer _sprintState;

        [Fact(Skip = "Not tested atm")]
        public async Task LoadTasks_ShouldClearTasks_WhenApiResponseFails()
        {
            // Arrange
            var httpClient = CreateMockHttpClient(HttpStatusCode.InternalServerError, null);

            var taskStateContainer = new TaskStateContainer(_httpClient, _jsRuntime, _sprintState);
            bool wasNotified = false;

            taskStateContainer.OnChange += () => wasNotified = true;

            // Act
            await taskStateContainer.LoadTasks(1);

            // Assert
            Assert.Empty(taskStateContainer.Tasks);
            Assert.True(wasNotified);
        }

        [Fact(Skip = "Not tested atm")]
        public void AddOrUpdateTask_ShouldAddTask_WhenTaskDoesNotExist()
        {
            // Arrange
            var taskStateContainer = new TaskStateContainer(_httpClient, _jsRuntime, _sprintState);
            var newTask = new AgileMinds.Shared.Models.Task { Id = 1, Name = "New Task" };
            bool wasNotified = false;

            taskStateContainer.OnChange += () => wasNotified = true;

            // Act
            taskStateContainer.AddOrUpdateTask(newTask);

            // Assert
            Assert.Single(taskStateContainer.Tasks);
            Assert.Equal("New Task", taskStateContainer.Tasks[0].Name);
            Assert.True(wasNotified);
        }

        [Fact(Skip = "Not tested atm")]
        public void AddOrUpdateTask_ShouldUpdateTask_WhenTaskExists()
        {
            // Arrange
            var taskStateContainer = new TaskStateContainer(_httpClient, _jsRuntime, _sprintState);
            var existingTask = new AgileMinds.Shared.Models.Task { Id = 1, Name = "Old Task" };
            taskStateContainer.AddOrUpdateTask(existingTask);

            var updatedTask = new AgileMinds.Shared.Models.Task { Id = 1, Name = "Updated Task" };
            bool wasNotified = false;

            taskStateContainer.OnChange += () => wasNotified = true;

            // Act
            taskStateContainer.AddOrUpdateTask(updatedTask);

            // Assert
            Assert.Single(taskStateContainer.Tasks);
            Assert.Equal("Updated Task", taskStateContainer.Tasks[0].Name);
            Assert.True(wasNotified);
        }

        [Fact(Skip = "Not tested atm")]
        public void AddOrUpdateTask_ShouldAddMultipleTasks_WhenCalledWithDifferentIds()
        {
            // Arrange
            var taskStateContainer = new TaskStateContainer(_httpClient, _jsRuntime, _sprintState);

            var task1 = new AgileMinds.Shared.Models.Task { Id = 1, Name = "Task 1" };
            var task2 = new AgileMinds.Shared.Models.Task { Id = 2, Name = "Task 2" };

            // Act
            taskStateContainer.AddOrUpdateTask(task1);
            taskStateContainer.AddOrUpdateTask(task2);

            // Assert
            Assert.Equal(2, taskStateContainer.Tasks.Count);
            Assert.Contains(taskStateContainer.Tasks, t => t.Name == "Task 1");
            Assert.Contains(taskStateContainer.Tasks, t => t.Name == "Task 2");
        }
        private HttpClient CreateMockHttpClient(HttpStatusCode statusCode, List<AgileMinds.Shared.Models.Task>? responseContent)
        {
            var mockHandler = new Mock<HttpMessageHandler>();

            mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(() =>
                {
                    var responseMessage = new HttpResponseMessage
                    {
                        StatusCode = statusCode,
                    };

                    if (statusCode == HttpStatusCode.OK && responseContent != null)
                    {
                        var json = System.Text.Json.JsonSerializer.Serialize(responseContent, new System.Text.Json.JsonSerializerOptions
                        {
                            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
                        });
                        responseMessage.Content = new StringContent(json);
                        responseMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                        Console.WriteLine($"Mock Response JSON: {json}"); // Debugging log
                    }
                    else
                    {
                        responseMessage.Content = new StringContent("");
                    }

                    return responseMessage;
                });

            return new HttpClient(mockHandler.Object);
        }

    }
}
