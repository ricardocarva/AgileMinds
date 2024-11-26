using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace AgileMindsUI.Client.Services
{
    public class GPTService
    {
        private readonly HttpClient _http;
        private readonly ILogger<GPTService> _logger;

        public GPTService(HttpClient http, ILogger<GPTService> logger)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> AskGptAsync(string question)
        {
            if (string.IsNullOrWhiteSpace(question))
            {
                throw new ArgumentException("Question cannot be null or empty.", nameof(question));
            }

            var request = new GptRequest
            {
                Question = question
            };

            try
            {
                // Send the POST request to your Web API's GPT controller
                var response = await _http.PostAsJsonAsync("api/gpt/ask-gpt", request);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }

                var errorDetails = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Error fetching GPT response: {response.StatusCode}, Details: {errorDetails}");
                return $"Error occurred: {response.ReasonPhrase}";
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed while asking GPT.");
                return "Error occurred while communicating with GPT service.";
            }
        }
    }

    public class GptRequest
    {
        public string Question { get; set; } = string.Empty;
    }
}