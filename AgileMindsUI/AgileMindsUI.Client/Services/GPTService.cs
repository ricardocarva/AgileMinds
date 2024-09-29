using OpenAI.Chat;

namespace AgileMindsUI.Client.Services
{
    public class GPTService
    {
        private readonly ChatClient _client;
        private readonly string _model;
        private readonly string _key;

        public GPTService()
        {
            _model = "gpt-4o-mini";
            _key = "YOUR_KEY_HERE";
        }

        public async Task<string> AskGptAsync(string question)
        {
            // not sure we need a new client every request, but it's working fine for now
            ChatClient client = new(model: _model, _key);

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
