using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

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

        public async Task<Project?> FetchProjectById(int projectId)
        {
            try
            {
                // Make the request
                var response = await _httpClient.GetAsync($"api/projects/{projectId}");

                // Check if the status code is 404 (Not Found)
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return null; // Return null if the project is not found
                }

                // Deserialize the JSON content into a Project object
                var project = await response.Content.ReadFromJsonAsync<Project>();

                // Return the project or null if deserialization failed
                return project;
            }
            catch (HttpRequestException)
            {
                // Handle other exceptions if needed, or rethrow
                return null;
            }
            catch (JsonException)
            {
                // Handle JSON deserialization errors separately if necessary
                return null;
            }
        }
        //public async Task<Project> FetchProjectById(int projectId)
        //{
        //    if (SelectedProject == null || SelectedProject.Id != projectId)
        //    {
        //        SelectedProject = await _httpClient.GetFromJsonAsync<Project>($"api/projects/{projectId}");
        //    }

        //    return SelectedProject;
        //}
    }
}
