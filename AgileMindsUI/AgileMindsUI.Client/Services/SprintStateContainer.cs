using System.Net.Http.Json;

using AgileMinds.Shared.Models;

using Microsoft.JSInterop;

namespace AgileMindsUI.Client.Services
{
    public class SprintStateContainer
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;

        public SprintStateContainer(HttpClient httpClient, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
        }

        public Sprint OpenSprint { get; private set; }
        public List<Sprint> CompletedSprints { get; private set; } = new List<Sprint>();
        public List<Sprint> AllSprints { get; private set; } = new List<Sprint>();
        public event Action OnChange;

        private void NotifyStateChanged() => OnChange?.Invoke();

        public async System.Threading.Tasks.Task LoadSprints(int projectId)
        {
            if (_jsRuntime is IJSInProcessRuntime)
            {
                try
                {
                    var response = await _httpClient.GetAsync($"api/projects/{projectId}/sprints");
                    if (response.IsSuccessStatusCode)
                    {
                        AllSprints = await response.Content.ReadFromJsonAsync<List<Sprint>>() ?? new List<Sprint>();

                        OpenSprint = AllSprints.FirstOrDefault(s => !s.IsCompleted);
                        CompletedSprints = AllSprints.Where(s => s.IsCompleted).ToList();
                        NotifyStateChanged();
                    }
                }
                catch (Exception ex)
                {
                    AllSprints = new List<Sprint>();
                    OpenSprint = null;
                    CompletedSprints = new List<Sprint>();
                }
            }
        }

        public void UpdateSprint(Sprint sprint)
        {
            var existingSprint = AllSprints.FirstOrDefault(s => s.Id == sprint.Id);
            if (existingSprint != null)
            {
                AllSprints.Remove(existingSprint);
            }
            AllSprints.Add(sprint);
            OpenSprint = AllSprints.FirstOrDefault(s => !s.IsCompleted);
            CompletedSprints = AllSprints.Where(s => s.IsCompleted).ToList();
            NotifyStateChanged();
        }
    }
}
