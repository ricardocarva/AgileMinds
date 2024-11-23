using System.Net.Http.Json;

using AgileMinds.Shared.Models;

namespace AgileMindsUI.Client.Services
{
    public class SprintService
    {
        private readonly HttpClient _httpClient;

        public SprintService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Get open sprint for the project
        public async Task<Sprint?> GetOpenSprint(int projectId)
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

        // Get completed sprints for the project
        public async Task<List<Sprint?>> GetCompletedSprints(int projectId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/projects/{projectId}/sprints/completed");

                if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    // Handle the case where there is no open sprint
                    return null;
                }

                if (response.IsSuccessStatusCode)
                {
                    // Deserialize only if there is content for debugging
                    return await response.Content.ReadFromJsonAsync<List<Sprint?>>();
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
                Console.WriteLine($"Error fetching completed sprint: {ex.Message}");
                throw;
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> CreateSprint(int projectId, Sprint sprint)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"api/projects/{projectId}/sprints", sprint);

                if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    // Return the error message from the response
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    return (false, errorMessage);
                }

                if (response.IsSuccessStatusCode)
                {
                    return (true, null);
                }
                else
                {
                    return (false, "Failed to create sprint due to an unknown error.");
                }
            }
            catch (Exception ex)
            {
                // Log exception details
                return (false, $"Error creating sprint: {ex.Message}");
            }
        }



        // Start an existing sprint
        public async Task<bool> StartSprint(int sprintId)
        {
            var response = await _httpClient.PutAsync($"api/projects/{sprintId}/start", null);
            return response.IsSuccessStatusCode;
        }

        // Complete an existing sprint
        public async Task<bool> CompleteSprint(int sprintId)
        {
            var response = await _httpClient.PutAsync($"api/projects/{sprintId}/complete", null);
            return response.IsSuccessStatusCode;
        }
    }
}
