using OpenAI.Chat;

namespace AgileMindsUI.Client.Services
{
    public class GPTService
    {
        private readonly ChatClient _client;

        public GPTService(string apiKey = "")
        {
            //_client = new OpenAIClient(new OpenAIAuthentication(apiKey));
        }

        public async Task<string> AskGptAsync(string question)
        {
            ChatClient client = new(model: "gpt-4o-mini", "");

            var configuredQuestion = $"please give a brief concise answer and then 4 example tasks the user can create while planning the project please make sure the tasks are separate by newlines, respond in json format with the answer and tasks as separate json properties for easy parsing. the question is: {question}";
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


            ChatCompletion completion = await client.CompleteChatAsync([configuredQuestion], options);

            if (completion != null) { return completion.ToString(); }

            return "No response from AI.";
        }
    }
}
