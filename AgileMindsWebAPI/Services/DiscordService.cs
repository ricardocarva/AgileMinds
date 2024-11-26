using AgileMindsWebAPI.Data;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

using Microsoft.EntityFrameworkCore;

using OpenAI.Chat;
namespace DiscordBot
{
    // SlashCommandModule for slash commands
    public class SlashCommandModule : ApplicationCommandModule
    {
        private readonly DiscordBotService _discordBotService;

        public SlashCommandModule(DiscordBotService discordBotService)
        {
            _discordBotService = discordBotService;
        }
        [SlashCommand("ping", "Responds with Pong!")]
        public async System.Threading.Tasks.Task PingCommand(InteractionContext ctx)
        {
            Console.WriteLine("pinged...now should pong...");
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Pong!"));
        }

        [SlashCommand("echo", "Repeats the text you provide.")]
        public async System.Threading.Tasks.Task EchoCommand(InteractionContext ctx, [Option("text", "Text to repeat")] string text)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(text));
        }

        [SlashCommand("askgpt", "Ask ChatGPT something.")]
        public async System.Threading.Tasks.Task AskGptCommand(InteractionContext ctx, [Option("question", "Your question")] string question)
        {
            var chatClient = new ChatClient(model: "gpt-4", "");
            var response = chatClient.CompleteChat(question);

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"{response.Value.Content[0].Text}"));
        }
        [SlashCommand("tasks", "List Project Tasks")]
        public async Task TasksCommand(InteractionContext ctx)
        {
            var tasks = await _discordBotService.GetTasksForGuild(ctx.Guild.Id);

            if (!tasks.Any())
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("No tasks found for this project."));
                return;
            }

            var tasksList = string.Join("\n", tasks.Select(t => $"{t.Id}: {t.Name}")); // Assuming each task has a Title property
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Tasks:\n{tasksList}"));
        }
    }

    // DiscordBotService that runs as a background service
    public class DiscordBotService : IHostedService
    {
        private readonly ILogger<DiscordBotService> _logger;
        private DiscordClient _discordClient;
        private SlashCommandsExtension _slashCommands;
        private readonly ChatClient _chatClient;
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly Dictionary<ulong, bool> _guildAsked = new();

        public DiscordBotService(ILogger<DiscordBotService> logger, IConfiguration configuration, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _configuration = configuration;
            _serviceScopeFactory = serviceScopeFactory;
            var _keyGPT = configuration["GPT:ApiKey"];  // API key loaded from configuration
            if (string.IsNullOrEmpty(_keyGPT))
            {
                throw new ArgumentNullException(nameof(_keyGPT), "OpenAI API key is not provided in the configuration.");
            }
            _chatClient = GetChatClient(_keyGPT);
            var _keyBot = configuration["DiscordBot:Token"];  // API key loaded from configuration
            if (string.IsNullOrEmpty(_keyBot))
            {
                throw new ArgumentNullException(nameof(_keyBot), "Discord Bot key is not provided in the configuration.");
            }
            _discordClient = GetDiscordClient(_keyBot);
        }


        public async System.Threading.Tasks.Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting Discord bot...");

            RegisterEventHandlers();
            RegisterSlashCommands();

            await _discordClient.ConnectAsync();
        }

        public async System.Threading.Tasks.Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping Discord bot...");
            await _discordClient.DisconnectAsync();
        }

        private void RegisterEventHandlers()
        {
            _discordClient.MessageCreated += async (s, e) =>
            {
                if (e.Author.IsBot) return; // Ignore messages from bots, including itself

                if (e.Guild != null && !_guildAsked.ContainsKey(e.Guild.Id))
                {
                    ulong guildId = e.Guild.Id;
                    var isLinked = await CheckIfGuildIsLinked(guildId);

                    if (!isLinked)
                    {
                        await e.Message.RespondAsync("Please enter the secret password to link this project.");
                        _guildAsked[guildId] = true; // Mark the guild as asked
                    }
                }
            };

            _discordClient.MessageCreated += async (s, e) =>
            {
                if (e.Author.IsBot) return; // Ignore messages from bots, including itself

                if (e.Guild != null && _guildAsked.ContainsKey(e.Guild.Id))
                {
                    ulong guildId = e.Guild.Id;
                    var result = await VerifySecretWithBackend(guildId, e.Message.Content);

                    if (result)
                    {
                        await e.Message.RespondAsync("Project successfully linked!");
                        _guildAsked.Remove(guildId); // Reset after linking
                    }
                    else
                    {
                        await e.Message.RespondAsync("Incorrect secret. Please try again.");
                    }
                }
            };
        }


        public async Task<List<AgileMinds.Shared.Models.Task>> GetTasksForGuild(ulong guildId)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // Find the linked project using the guildId
                var integration = await _context.DiscordIntegrations
                    .FirstOrDefaultAsync(d => d.DiscordServerId == guildId.ToString());

                if (integration == null)
                {
                    return new List<AgileMinds.Shared.Models.Task>(); // Return empty list if no project linked
                }

                // Fetch tasks associated with the project
                var tasks = await _context.Tasks
                    .Where(t => t.ProjectId == integration.ProjectId)
                    .ToListAsync();

                return tasks;
            }
        }

        private async Task<bool> CheckIfGuildIsLinked(ulong guildId)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var discordIntegration = await _context.DiscordIntegrations
                    .FirstOrDefaultAsync(d => d.DiscordServerId == guildId.ToString());
                return discordIntegration != null && discordIntegration.IsLinked;
            }
        }

        private async Task<bool> VerifySecretWithBackend(ulong guildId, string secret)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var discordIntegration = await _context.DiscordIntegrations
                    .FirstOrDefaultAsync(d => d.DiscordServerId == guildId.ToString());

                if (discordIntegration != null && discordIntegration.DiscordProjectSecret == secret)
                {
                    // Mark as linked
                    discordIntegration.IsLinked = true;
                    _context.DiscordIntegrations.Update(discordIntegration);
                    await _context.SaveChangesAsync();
                    return true;
                }

                return false;
            }
        }


        private void RegisterSlashCommands()
        {
            var commandsConfig = new SlashCommandsConfiguration { Services = _serviceScopeFactory.CreateScope().ServiceProvider };
            _slashCommands = _discordClient.UseSlashCommands(commandsConfig);
            _slashCommands.RegisterCommands<SlashCommandModule>();

            _slashCommands.SlashCommandExecuted += async (sender, e) =>
            {
                _logger.LogInformation($"Slash command executed: {e.Context.CommandName}");
            };

            _slashCommands.SlashCommandErrored += async (sender, e) =>
            {
                _logger.LogError($"Error executing slash command: {e.Exception.Message}");
            };
        }

        private static DiscordClient GetDiscordClient(string key)
        {
            return new DiscordClient(new DiscordConfiguration
            {
                Token = key,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.All
            });
        }

        private static ChatClient GetChatClient(string key)
        {
            return new ChatClient("gpt-4", key);
        }
    }
}

