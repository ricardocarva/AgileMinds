using System.Net.Http.Json;

using AgileMinds.Shared.Models;

namespace AgileMindsUI.Client.Services
{
    public class ProjectService
    {
        private readonly HttpClient _httpClient;

        public ProjectService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public Project SelectedProject { get; private set; }

        public void SetSelectedProject(Project project)
        {
            SelectedProject = project;
        }

        public Project GetSelectedProject()
        {
            return SelectedProject;
        }

        public async Task<Project> FetchProjectById(int projectId)
        {
            if (SelectedProject == null || SelectedProject.Id != projectId)
            {
                SelectedProject = await _httpClient.GetFromJsonAsync<Project>($"api/projects/{projectId}");
            }

            return SelectedProject;
        }
    }
}
