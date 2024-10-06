using System.Text.Json;

using Microsoft.AspNetCore.Mvc;

using OpenAI.Chat;

namespace AgileMindsWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GptController : ControllerBase
    {
        private readonly string _model;
        private readonly string _key;

        public GptController(IConfiguration configuration)
        {
            _model = "gpt-4o-mini";  // Set your GPT model here
            _key = configuration["GPT:ApiKey"];  // API key loaded from configuration

            if (string.IsNullOrEmpty(_key))
            {
                throw new ArgumentNullException(nameof(_key), "OpenAI API key is not provided in the configuration.");
            }
        }

        [HttpPost("ask-gpt")]
        public async Task<IActionResult> AskGpt([FromBody] GptRequest request)
        {
            if (string.IsNullOrEmpty(request.Question))
            {
                return BadRequest("Question is required.");
            }

            // Call the GPT model and get the response
            var response = await AskGptAsync(request.Question);

            if (response == null)
            {
                return StatusCode(500, new { Error = "Failed to process AI request." });
            }

            return Ok(response);  // Return the GPT response as JSON
        }

        // GPT logic moved from the service into the controller
        private async Task<AiResponse?> AskGptAsync(string question)
        {
            try
            {
                // Create a new ChatClient instance every request
                ChatClient client = new(_model, _key);

                var configuredQuestion = $"please give a brief concise answer and then 4 example tasks the user can create while planning the project. Please make sure the tasks are separate by newlines, respond in json format with the answer and tasks as separate json properties for easy parsing. The question is: {question}";

                ChatCompletionOptions options = new()
                {
                    ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                        "task_generation",
                        jsonSchema: BinaryData.FromString("""
                        {
                            "type": "object",
                            "properties": {
                                "answer": {
                                    "type": "string",
                                    "description": "A brief answer to the user's question."
                                },
                                "tasks": {
                                    "type": "array",
                                    "items": {
                                        "type": "string",
                                        "description": "A list of tasks based on the user's project description."
                                    }
                                }
                            },
                            "required": ["answer", "tasks"],
                            "additionalProperties": false
                        }
                        """)
                    )
                };

                // Use a simple string array instead of List<ChatMessage>
                ChatCompletion completion = await client.CompleteChatAsync([configuredQuestion], options);

                if (completion == null)
                {
                    return new AiResponse { Answer = "No response from AI.", Tasks = new List<string>() };
                }

                // Deserialize the response to AiResponse
                var aiResponse = JsonSerializer.Deserialize<AiResponse>(completion.ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return aiResponse ?? new AiResponse { Answer = "No response from AI.", Tasks = new List<string>() };
            }
            catch (Exception ex)
            {
                // Log the exception (logging is recommended here)
                return new AiResponse { Answer = "Error processing request.", Tasks = new List<string> { ex.Message } };
            }
        }
    }

    // Model for the request body
    public class GptRequest
    {
        public string Question { get; set; }
    }

    // Model for the AI response
    public class AiResponse
    {
        public string Answer { get; set; }
        public List<string> Tasks { get; set; }
    }
}
