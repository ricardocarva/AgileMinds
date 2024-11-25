using System.Net.Http.Json;

namespace AgileMindsUI.Client.Services
{
    public class TaskStateContainer
    {
        private readonly object _lock = new object();

        // List to store tasks
        public List<AgileMinds.Shared.Models.Task> Tasks { get; private set; } = new List<AgileMinds.Shared.Models.Task>();

        // Event to notify subscribers of state changes
        public event Action? OnChange;

        // Notify all listeners about state changes
        private void NotifyStateChanged() => OnChange?.Invoke();

        // Load tasks for a specific project from the API
        public async Task LoadTasks(int projectId, HttpClient http)
        {
            try
            {
                // Fetch tasks from the API
                var tasks = await FetchTasksFromApi(projectId, http);

                lock (_lock)
                {
                    Tasks = tasks ?? new List<AgileMinds.Shared.Models.Task>();
                }

                Console.WriteLine($"Loaded {Tasks.Count} tasks."); // Debugging log
                NotifyStateChanged();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading tasks: {ex.Message}");

                lock (_lock)
                {
                    Tasks = new List<AgileMinds.Shared.Models.Task>();
                }

                NotifyStateChanged(); // Notify listeners even in error state
            }
        }

        // Add or update a task in the state container
        public void AddOrUpdateTask(AgileMinds.Shared.Models.Task task)
        {
            lock (_lock)
            {
                var existingTask = Tasks.FirstOrDefault(t => t.Id == task.Id);
                if (existingTask != null)
                {
                    Tasks.Remove(existingTask);
                }
                Tasks.Add(task);
            }
            NotifyStateChanged();
        }

        // Fetch tasks from the API
        private async Task<List<AgileMinds.Shared.Models.Task>> FetchTasksFromApi(int projectId, HttpClient http)
        {
            var response = await http.GetAsync($"api/projects/{projectId}/tasks");

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var tasks = await response.Content.ReadFromJsonAsync<List<AgileMinds.Shared.Models.Task>>(new System.Text.Json.JsonSerializerOptions
                    {
                        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
                    });
                    if (tasks == null || tasks.Count == 0)
                    {
                        Console.WriteLine("No tasks deserialized.");
                    }
                    else
                    {
                        Console.WriteLine($"Deserialized {tasks.Count} tasks.");
                    }
                    return tasks ?? new List<AgileMinds.Shared.Models.Task>();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Deserialization error: {ex.Message}");
                    return new List<AgileMinds.Shared.Models.Task>();
                }
            }
            else
            {
                Console.WriteLine($"Failed to fetch tasks. HTTP Status: {response.StatusCode}");
                return new List<AgileMinds.Shared.Models.Task>();
            }
        }
    }
}
