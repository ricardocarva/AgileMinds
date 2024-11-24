using System.Net.Http.Json;

using AgileMinds.Shared.Models;

using Microsoft.JSInterop;

namespace AgileMindsUI.Client.Services
{
    public class TaskStateContainer
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;

        public TaskStateContainer(HttpClient httpClient, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;

        }
        public List<AgileMinds.Shared.Models.Task> Tasks { get; private set; } = new List<AgileMinds.Shared.Models.Task>();
        public List<AgileMinds.Shared.Models.Task> TasksKanban { get; private set; } = new List<AgileMinds.Shared.Models.Task>();
        public event Action OnChange;

        private void NotifyStateChanged() => OnChange?.Invoke();

        public async System.Threading.Tasks.Task LoadTasks(int projectId, HttpClient http, Sprint? openSprint)
        {
            if (_jsRuntime is IJSInProcessRuntime)
            {
                try
                {
                    var response = await http.GetAsync($"api/projects/{projectId}/tasks");
                    if (response.IsSuccessStatusCode)
                    {
                        Tasks = await response.Content.ReadFromJsonAsync<List<AgileMinds.Shared.Models.Task>>() ?? new List<AgileMinds.Shared.Models.Task>();

                        // Always set TasksKanban
                        if (openSprint != null)
                        {
                            TasksKanban = Tasks.Where(t => t.SprintId == openSprint.Id).ToList();
                        }
                        else
                        {
                            // If no sprints open, tasks from kanban wil lbe the all tasks not on a sprint
                            TasksKanban = Tasks.Where(t => t.SprintId == null).ToList();
                        }
                        NotifyStateChanged();
                    }
                }
                catch
                {
                    Tasks = new List<AgileMinds.Shared.Models.Task>();
                    TasksKanban = new List<AgileMinds.Shared.Models.Task>();
                }
            }
            else
            {
                // Running on server during prerendering, skip loading tasks
            }
        }

        private async Task<Sprint?> GetOpenSprintKanban(int projectId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/projects/{projectId}/sprints/open");

                if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    // Handle the case where there is no open sprint
                    return null;
                }

                if (response.IsSuccessStatusCode)
                {
                    // Deserialize only if there is content for debugging
                    return await response.Content.ReadFromJsonAsync<Sprint>();
                }
                else
                {
                    // Add logging for non-success responses
                    Console.WriteLine($"Failed to fetch open sprint. Status code: {response.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                // Log exception details
                Console.WriteLine($"Error fetching open sprint: {ex.Message}");
                throw;
            }
        }

        public void AddOrUpdateTask(AgileMinds.Shared.Models.Task task)
        {
            var existingTask = Tasks.FirstOrDefault(t => t.Id == task.Id);
            if (existingTask != null)
            {
                Tasks.Remove(existingTask);
            }
            Tasks.Add(task);
            NotifyStateChanged();
        }

    }
}
