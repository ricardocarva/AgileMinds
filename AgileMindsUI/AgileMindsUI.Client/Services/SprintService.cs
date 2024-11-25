using System.Net.Http.Json;
using AgileMinds.Shared.Models;
using Microsoft.Extensions.Logging;

namespace AgileMindsUI.Client.Services
{
    public class SprintService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<SprintService> _logger;

        public SprintService(HttpClient httpClient, ILogger<SprintService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Get the open sprint for the project
        public async Task<Sprint?> GetOpenSprint(int projectId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/projects/{projectId}/sprints/open");

                if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    _logger.LogInformation($"No open sprint found for project ID: {projectId}");
                    return null;
                }

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<Sprint>();
                }

                _logger.LogWarning($"Failed to fetch open sprint. Status code: {response.StatusCode}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching open sprint for project ID: {projectId}");
                throw;
            }
        }

        // Get completed sprints for the project
        public async Task<List<Sprint>> GetCompletedSprints(int projectId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/projects/{projectId}/sprints/completed");

                if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    _logger.LogInformation($"No completed sprints found for project ID: {projectId}");
                    return new List<Sprint>();
                }

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<Sprint>>() ?? new List<Sprint>();
                }

                _logger.LogWarning($"Failed to fetch completed sprints. Status code: {response.StatusCode}");
                return new List<Sprint>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching completed sprints for project ID: {projectId}");
                throw;
            }
        }

        // Create a new sprint
        public async Task<(bool Success, string? ErrorMessage)> CreateSprint(int projectId, Sprint sprint)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"api/projects/{projectId}/sprints", sprint);

                if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Conflict when creating sprint: {errorMessage}");
                    return (false, errorMessage);
                }

                if (response.IsSuccessStatusCode)
                {
                    return (true, null);
                }

                _logger.LogWarning($"Failed to create sprint. Status code: {response.StatusCode}");
                return (false, "Unknown error occurred while creating sprint.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating sprint for project ID: {projectId}");
                return (false, ex.Message);
            }
        }

        // Start an existing sprint
        public async Task<bool> StartSprint(int sprintId)
        {
            try
            {
                var response = await _httpClient.PutAsync($"api/sprints/{sprintId}/start", null);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Failed to start sprint ID: {sprintId}. Status code: {response.StatusCode}");
                }
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error starting sprint ID: {sprintId}");
                throw;
            }
        }

        // Complete an existing sprint
        public async Task<bool> CompleteSprint(int sprintId)
        {
            try
            {
                var response = await _httpClient.PutAsync($"api/sprints/{sprintId}/complete", null);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Failed to complete sprint ID: {sprintId}. Status code: {response.StatusCode}");
                }
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error completing sprint ID: {sprintId}");
                throw;
            }
        }
    }
}
