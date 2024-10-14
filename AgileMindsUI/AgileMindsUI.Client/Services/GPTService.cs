using System.Net.Http.Json;

namespace AgileMindsUI.Client.Services
{
    public class GPTService
    {
        private readonly HttpClient _http;

        public GPTService(HttpClient http)
        {
            _http = http;
        }

        public async Task<string> AskGptAsync(string question)
        {
            var request = new GptRequest
            {
                Question = question
            };

            // Send the POST request to your Web API's GPT controller
            var response = await _http.PostAsJsonAsync(requestUri: "api/gpt/ask-gpt", request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }

            return "Error occurred while fetching GPT response.";
        }
    }
    public class GptRequest
    {
        public string Question { get; set; }
    }
}
