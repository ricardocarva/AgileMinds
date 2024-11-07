using AgileMindsUI.Client.Models;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using AgileMinds.Shared.Models;


namespace AgileMindsUI.Client.Services
{
    public interface IProjectService
    {
        Project? SelectedProject { get; }
        void SetSelectedProject(Project project);
        Task<Project?> FetchProjectById(int projectId);
    }

    public class ProjectService : IProjectService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ProjectService> _logger;

        public ProjectService(HttpClient httpClient, ILogger<ProjectService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public Project? SelectedProject { get; private set; }

        public void SetSelectedProject(Project? project)
        {
            SelectedProject = project;
        }

        public async System.Threading.Tasks.Task<bool> SetSelectedProjectById(int projectId)
        {
            var project = await FetchProjectById(projectId);
            if (project != null)
            {
                SelectedProject = project;
                _logger.LogInformation("Project with ID {ProjectId} set as SelectedProject", projectId);
                return true;
            }
            _logger.LogWarning("Failed to set SelectedProject. Project ID {ProjectId} not found.", projectId);
            return false;
        }


        public async Task<Project?> FetchProjectById(int projectId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/projects/{projectId}");

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Project not found for ID: {ProjectId}", projectId);
                    return null;
                }

                response.EnsureSuccessStatusCode(); // Ensure response status is successful

                var project = await response.Content.ReadFromJsonAsync<Project>();

                if (project == null)
                {
                    _logger.LogWarning("Deserialization of Project returned null for ID: {ProjectId}", projectId);
                }
                else
                {
                    _logger.LogInformation("Project with ID {ProjectId} successfully fetched", projectId);
                }

                return project;

            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed for Project ID: {ProjectId}", projectId);
                return null;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON deserialization error for Project ID: {ProjectId}", projectId);
                return null;
            }
        }
    }
}
