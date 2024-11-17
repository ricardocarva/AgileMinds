using System.Net.Http.Json;

namespace AgileMindsUI.Client.Services
{
    public class TaskStateContainer
    {
        public List<AgileMinds.Shared.Models.Task> Tasks { get; private set; } = new List<AgileMinds.Shared.Models.Task>();

        public event Action OnChange;

        private void NotifyStateChanged() => OnChange?.Invoke();

        public async Task LoadTasks(int projectId, HttpClient http)
        {
            try
            {
                var response = await http.GetAsync($"api/projects/{projectId}/tasks");
                if (response.IsSuccessStatusCode)
                {
                    Tasks = await response.Content.ReadFromJsonAsync<List<AgileMinds.Shared.Models.Task>>() ?? new List<AgileMinds.Shared.Models.Task>();
                    NotifyStateChanged();
                }
                else
                {
                    Tasks = new List<AgileMinds.Shared.Models.Task>();
                }
            }
            catch
            {
                Tasks = new List<AgileMinds.Shared.Models.Task>();
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
