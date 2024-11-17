using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AgileMinds.Shared.Models;
using Microsoft.Extensions.Logging;

namespace AgileMindsUI.Client.Services
{
    public class ProjectService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ProjectService> _logger;

        public ProjectService(HttpClient httpClient, ILogger<ProjectService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Project? SelectedProject { get; private set; }

        /// <summary>
        /// Sets the currently selected project.
        /// </summary>
        public void SetSelectedProject(Project? project)
        {
            SelectedProject = project ?? throw new ArgumentNullException(nameof(project));
        }

        /// <summary>
        /// Gets the currently selected project.
        /// </summary>
        public Project? GetSelectedProject()
        {
            return SelectedProject;
        }

        /// <summary>
        /// Fetches a project by its ID.
        /// </summary>
        /// <param name="projectId">The ID of the project to fetch.</param>
        /// <returns>The project if found, otherwise null.</returns>
        public async Task<Project?> FetchProjectById(int projectId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/projects/{projectId}").ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        _logger.LogWarning($"Project with ID {projectId} not found.");
                        return null;
                    }

                    _logger.LogError($"Failed to fetch project. Status code: {response.StatusCode}");
                    return null;
                }

                var project = await DeserializeResponse<Project>(response).ConfigureAwait(false);
                if (project == null)
                {
                    _logger.LogError($"Deserialization of project with ID {projectId} failed.");
                }

                return project;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"HTTP request failed while fetching project with ID {projectId}.");
                return null;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, $"JSON deserialization failed for project with ID {projectId}.");
                return null;
            }
        }

        /// <summary>
        /// Deserializes the HTTP response content.
        /// </summary>
        /// <typeparam name="T">The type to deserialize to.</typeparam>
        /// <param name="response">The HTTP response message.</param>
        /// <returns>The deserialized object, or null if deserialization fails.</returns>
        private async Task<T?> DeserializeResponse<T>(HttpResponseMessage response) where T : class
        {
            try
            {
                return await response.Content.ReadFromJsonAsync<T>().ConfigureAwait(false);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, $"Failed to deserialize response content to type {typeof(T).Name}.");
                return null;
            }
        }
    }
}
