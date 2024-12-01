using AgileMinds.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using OpenAI.Chat;
using System.Text.Json;


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
        private async Task<AgileMinds.Shared.Models.AiResponse?> AskGptAsync(string question)
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
                    return new AgileMinds.Shared.Models.AiResponse { Answer = "No response from AI.", Tasks = new List<string>() };
                }

                // Deserialize the response to AiResponse
                var aiResponse = JsonSerializer.Deserialize<AgileMinds.Shared.Models.AiResponse>(completion.ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return aiResponse ?? new AgileMinds.Shared.Models.AiResponse { Answer = "No response from AI.", Tasks = new List<string>() };
            }
            catch (Exception ex)
            {
                // Log the exception (logging is recommended here)
                return new AgileMinds.Shared.Models.AiResponse { Answer = "Error processing request.", Tasks = new List<string> { ex.Message } };
            }
        }

        [HttpPost("ask-gpt-detailed")]
        public async Task<IActionResult> AskGptDetailedAsync([FromBody] GptRequest request)
        {
            if (string.IsNullOrEmpty(request.Question))
            {
                return BadRequest("Question is required.");
            }

            // Call the GPT model and get the response
            var response = await AskGptDetailedSuggestionAsync(request.Question);

            if (response == null)
            {
                return StatusCode(500, new { Error = "Failed to process AI request." });
            }

            return Ok(response);  // Return the GPT response as JSON
        }

        public async Task<AgileMinds.Shared.Models.AiDetailedResponse?> AskGptDetailedSuggestionAsync(string question)
        {
            try
            {
                // Create a new ChatClient instance every request
                ChatClient client = new(_model, _key);

                var configuredQuestion = $"Please provide a brief, concise answer to the user's question, followed by 4 example tasks they can create while planning the project. Each task should be represented as an object with two properties: \"title\": The name of the task.\r\n\"description\": Individual lines with strings that includes the task's description, acceptance criteria, and definition of done. Separate these details with clear labels, except for the description string that MUST not have a label (Acceptance Criteria:, Definition of Done:) and double newlines before each label in the description.\r\nRespond in JSON format with two separate properties: \"answer\": A brief answer to the user's question. \"tasks\": An array of task objects, where each object contains a \"title\" and a \"description\" property. The question is: {question}";

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
                                        "type": "object",
                                        "properties": {
                                            "title": {
                                                "type": "string",
                                                "description": "A title for the task suggested for the project."
                                            },
                                            "description": {
                                                "type": "string",
                                                "description": "A single string containing a short description, acceptance criteria, and definition of done for each task, separated by labels and newlines."
                                            }
                                        },
                                        "required": ["title", "description"],
                                        "additionalProperties": false
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
                    return new AgileMinds.Shared.Models.AiDetailedResponse { Answer = "No response from AI.", Tasks = new List<TaskItem>() };
                }

                // Deserialize the response to AiResponse
                var aiResponse = JsonSerializer.Deserialize<AgileMinds.Shared.Models.AiDetailedResponse>(completion.ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return aiResponse ?? new AgileMinds.Shared.Models.AiDetailedResponse { Answer = "No response from AI.", Tasks = new List<TaskItem>() };
            }
            catch (Exception ex)
            {
                // Log the exception (logging is recommended here)
                return new AgileMinds.Shared.Models.AiDetailedResponse { Answer = "Error processing request.", Tasks = new List<TaskItem> { } };
            }
        }
    }
    // Model for the request body
    public class GptRequest
    {
        public string Question { get; set; }
    }
}
