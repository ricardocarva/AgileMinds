using System.Net.Http.Json;

using Microsoft.JSInterop;

namespace AgileMindsUI.Client.Services
{
    public class TaskStateContainer
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;
        private readonly SprintStateContainer _sprintState;

        public TaskStateContainer(HttpClient httpClient, IJSRuntime jsRuntime, SprintStateContainer sprintState)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
            _sprintState = sprintState;

            _sprintState.OnChange += OnSprintStateChanged;
        }
        public List<AgileMinds.Shared.Models.Task> Tasks { get; private set; } = new List<AgileMinds.Shared.Models.Task>();
        public List<AgileMinds.Shared.Models.Task> TasksKanban { get; private set; } = new List<AgileMinds.Shared.Models.Task>();
        public event Action OnChange;

        private void NotifyStateChanged() => OnChange?.Invoke();

        public async System.Threading.Tasks.Task LoadTasks(int projectId)
        {
            if (_jsRuntime is IJSInProcessRuntime)
            {
                try
                {
                    var response = await _httpClient.GetAsync($"api/projects/{projectId}/tasks");
                    if (response.IsSuccessStatusCode)
                    {
                        Tasks = await response.Content.ReadFromJsonAsync<List<AgileMinds.Shared.Models.Task>>() ?? new List<AgileMinds.Shared.Models.Task>();

                        UpdateTasksKanban();

                        NotifyStateChanged();
                    }
                }
                catch
                {
                    Tasks = new List<AgileMinds.Shared.Models.Task>();
                    TasksKanban = new List<AgileMinds.Shared.Models.Task>();
                }
            }
        }

        private void UpdateTasksKanban()
        {
            if (_sprintState.OpenSprint != null)
            {
                TasksKanban = Tasks
                    .Where(t => t.SprintId.HasValue && t.SprintId.Value == _sprintState.OpenSprint.Id)
                    .ToList();
            }
            else
            {
                var completedSprintIds = _sprintState.CompletedSprints.Select(s => s.Id).ToHashSet();
                TasksKanban = Tasks
                    .Where(t => !t.SprintId.HasValue || !completedSprintIds.Contains(t.SprintId.Value))
                    .ToList();
            }
        }

        private void OnSprintStateChanged()
        {
            UpdateTasksKanban();
            NotifyStateChanged();
        }

        public void AddOrUpdateTask(AgileMinds.Shared.Models.Task task)
        {
            var existingTask = Tasks.FirstOrDefault(t => t.Id == task.Id);
            if (existingTask != null)
            {
                Tasks.Remove(existingTask);
            }
            Tasks.Add(task);
            UpdateTasksKanban();
            NotifyStateChanged();
        }

    }
}
