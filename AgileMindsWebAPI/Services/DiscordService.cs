using AgileMinds.Shared.Models;
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

        [SlashCommand("askgpt", "Ask ChatGPT something!")]
        public async System.Threading.Tasks.Task AskGptCommand(InteractionContext ctx, [Option("question", "Your question")] string question)
        {
            try
            {
                // Immediately defer the response to avoid timing out
                await ctx.DeferAsync();

                if (string.IsNullOrWhiteSpace(question))
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Please provide a valid question."));
                    return;
                }

                Console.WriteLine($"Received question: {question}");

                // Call DiscordBotService for ChatGPT response
                var response = await _discordBotService.GetChatGptResponse(question);

                if (!string.IsNullOrWhiteSpace(response))
                {
                    // Check if the response exceeds Discord's 2000-character limit
                    if (response.Length > 2000)
                    {
                        // Convert the response to a memory stream for the text file
                        var fileStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(response));
                        var fileName = "response.txt";

                        // Send the response as a file attachment
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                            .WithContent("The response is too long. Here's the output as a text file:")
                            .AddFile(fileName, fileStream));
                    }
                    else
                    {
                        // Send the response normally if it's under the limit
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(response));
                    }
                }
                else
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("No response from ChatGPT."));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AskGptCommand: {ex.Message}");
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Error: {ex.Message}"));
            }
        }


        [SlashCommand("tasks", "List All Project Tasks")]
        public async System.Threading.Tasks.Task TasksCommand(InteractionContext ctx)
        {
            var tasks = await _discordBotService.GetTasksForGuild(ctx.Guild.Id);

            if (!tasks.Any())
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("No tasks found for this project."));
                return;
            }

            var tasksList = string.Join("\n", tasks.Select(t => $"{t.Id}: {t.Name}")); // Assuming each task has a Title property
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"\n**All Tasks**:\n{tasksList}"));
        }
        [SlashCommand("sprint", "List Current Sprint and Tasks")]
        public async System.Threading.Tasks.Task SprintsCommand(InteractionContext ctx)
        {
            // Fetch the current open sprint using the project ID (guild ID here is mapped to a project in your system)
            var sprints = await _discordBotService.GetSprintsForGuild(ctx.Guild.Id, onlyOpen: true);

            // Check if there are any open sprints
            if (sprints == null || !sprints.Any())
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent("No open sprint found for this project."));
                return;
            }

            // Assuming there is only one open sprint at a time
            var currentSprint = sprints.First();
            var sprintName = currentSprint.Name;

            // Check if the sprint has tasks
            if (currentSprint.Tasks == null || !currentSprint.Tasks.Any())
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent($"**{sprintName}**\nNo tasks found for this sprint."));
                return;
            }

            // Format tasks into a Markdown list
            var tasksList = string.Join("\n", currentSprint.Tasks.Select(t => $"- **{t.Id}**: {t.Name}"));

            // Construct the response
            var response = $"**\nSprint: {sprintName}**\n**Start Date**: {currentSprint.StartDate:MMMM dd, yyyy}\n**End Date**: {currentSprint.EndDate:MMMM dd, yyyy}\nTasks:\n{tasksList}";

            // Send the response
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent(response));
        }

        [SlashCommand("backlog", "List Backlog Tasks (Tasks not assigned to any sprint).")]
        public async System.Threading.Tasks.Task BacklogCommand(InteractionContext ctx)
        {
            // Fetch backlog tasks using the guild ID
            var backlogTasks = await _discordBotService.GetBacklogTasksForGuild(ctx.Guild.Id);

            // Check if there are any backlog tasks
            if (!backlogTasks.Any())
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent("No backlog tasks found for this project."));
                return;
            }

            // Format backlog tasks into a Markdown list
            var backlogList = string.Join("\n", backlogTasks.Select(t => $"- **{t.Id}**: {t.Name}"));

            // Construct the response
            var response = $"**\nBacklog Tasks**\n{backlogList}";

            // Send the response
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent(response));
        }


        [SlashCommand("task", "Get details of a specific task by its ID.")]
        public async System.Threading.Tasks.Task TaskCommand(InteractionContext ctx, [Option("id", "The ID of the task")] long taskId)
        {
            await ctx.DeferAsync();

            try
            {
                // Convert long to int
                int taskIdInt = (int)taskId;

                // Fetch the task using the DiscordBotService
                var task = await _discordBotService.GetTaskById(ctx.Guild.Id, taskIdInt);

                if (task == null)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                        .WithContent($"Task with ID {taskIdInt} not found."));
                    return;
                }

                // Format task details
                var taskDetails = $@"
**Task Details:**
- **ID**: {task.Id}
- **Name**: {task.Name}
- **Description**: {task.Description}
- **Status**: {task.Status}
- **Priority**: {task.Priority}
- **Assigned To**: {(task.AssignedUser?.Username ?? "Unassigned")}
- **Due Date**: {(task.DueDate?.ToString("yyyy-MM-dd") ?? "Not set")}
- **Sprint ID**: {(task.SprintId?.ToString() ?? "No sprint assigned")}";

                // Send the response
                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .WithContent(taskDetails));
            }
            catch (Exception ex)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .WithContent($"Error fetching task details: {ex.Message}"));
            }
        }

        [SlashCommand("taskhelp", "Get AI advice for a specific task by its ID.")]
        public async System.Threading.Tasks.Task TaskHelpCommand(InteractionContext ctx, [Option("id", "The ID of the task")] long taskId)
        {
            await ctx.DeferAsync();

            try
            {
                // Convert long to int
                int taskIdInt = (int)taskId;

                // Fetch the task using the DiscordBotService
                var task = await _discordBotService.GetTaskById(ctx.Guild.Id, taskIdInt);

                if (task == null)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                        .WithContent($"Task with ID {taskIdInt} not found."));
                    return;
                }

                // Generate the AI question
                var question = $@"
I am working on a task for a project:
**Task Name**: {task.Name}
**Description**: {task.Description}
**Status**: {task.Status}
**Priority**: {task.Priority}
**Assigned To**: {(task.AssignedUser?.Username ?? "Unassigned")}
**Due Date**: {(task.DueDate?.ToString("yyyy-MM-dd") ?? "Not set")}
**Sprint ID**: {(task.SprintId?.ToString() ?? "No sprint assigned")}
Could you provide advice or suggestions to help complete this task effectively?";

                // Get AI advice using DiscordBotService
                var advice = await _discordBotService.GetChatGptResponse(question);

                if (!string.IsNullOrWhiteSpace(advice))
                {
                    if (advice.Length > 2000)
                    {
                        var fileStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(advice));
                        var fileName = $"Task_{taskIdInt}_Advice.txt";

                        await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                            .WithContent("The advice is too long. Here's the output as a text file:")
                            .AddFile(fileName, fileStream));
                    }
                    else
                    {
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(advice));
                    }
                }
                else
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("No advice received from ChatGPT."));
                }
            }
            catch (Exception ex)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Error fetching advice: {ex.Message}"));
            }
        }
        [SlashCommand("project", "Get details about the current project.")]
        public async System.Threading.Tasks.Task ProjectCommand(InteractionContext ctx)
        {
            await ctx.DeferAsync();

            try
            {
                // Fetch project details using DiscordBotService
                var project = await _discordBotService.GetProjectDetailsForGuild(ctx.Guild.Id);

                if (project == null)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                        .WithContent("No project linked to this Discord server."));
                    return;
                }

                // Format project details
                var projectDetails = $@"
**Project Details:**
- **ID**: {project.Id}
- **Name**: {project.Name ?? "Unnamed Project"}
- **Description**: {project.Description ?? "No description provided."}
- **Created At**: {project.CreatedAt:yyyy-MM-dd}
- **Created By**: {project.CreatedBy}
- **Discord Integration**: {(project.DiscordIntegration ? "Enabled" : "Disabled")}
- **Canvas Integration**: {(project.CanvasIntegration ? "Enabled" : "Disabled")}
- **Members**: {project.Members.Count}
- **Tasks**: {project.Tasks.Count}";

                // Send the response
                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .WithContent(projectDetails));
            }
            catch (Exception ex)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .WithContent($"Error fetching project details: {ex.Message}"));
            }
        }

        [SlashCommand("members", "List all members of the current project.")]
        public async System.Threading.Tasks.Task MembersCommand(InteractionContext ctx)
        {
            await ctx.DeferAsync();

            try
            {
                // Fetch project members using DiscordBotService
                var members = await _discordBotService.GetMembersForGuild(ctx.Guild.Id);

                if (members == null || !members.Any())
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                        .WithContent("No members found for this project."));
                    return;
                }

                // Format the member list
                var memberList = string.Join("\n", members.Select(m => $"- **{m.Username}** - Role: {m.Role}"));

                // Construct the response
                var response = $@"
**Project Members**:
{memberList}";

                // Send the response
                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .WithContent(response));
            }
            catch (Exception ex)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .WithContent($"Error fetching members: {ex.Message}"));
            }
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
        public async Task<List<Sprint>> GetSprintsForGuild(ulong guildId, bool onlyOpen = false)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // Find the linked project using the guildId
                var integration = await _context.DiscordIntegrations
                    .FirstOrDefaultAsync(d => d.DiscordServerId == guildId.ToString());

                if (integration == null)
                {
                    return new List<Sprint>(); // Return empty list if no project is linked
                }

                // Fetch sprints associated with the project
                var sprintsQuery = _context.Sprints
                    .Include(s => s.Tasks) // Include tasks for each sprint
                    .Where(s => s.ProjectId == integration.ProjectId);

                // Filter to only open sprints if specified
                if (onlyOpen)
                {
                    sprintsQuery = sprintsQuery.Where(s => !s.IsCompleted);
                }

                var sprints = await sprintsQuery.ToListAsync();
                return sprints;
            }
        }

        public async Task<List<AgileMinds.Shared.Models.Task>> GetBacklogTasksForGuild(ulong guildId)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // Find the linked project using the guildId
                var integration = await _context.DiscordIntegrations
                    .FirstOrDefaultAsync(d => d.DiscordServerId == guildId.ToString());

                if (integration == null)
                {
                    return new List<AgileMinds.Shared.Models.Task>(); // Return an empty list if no project is linked
                }

                // Fetch tasks associated with the project that are not part of any sprint
                var backlogTasks = await _context.Tasks
                    .Where(t => t.ProjectId == integration.ProjectId && t.SprintId == null)
                    .ToListAsync();

                return backlogTasks;
            }
        }

        public async Task<string> GetChatGptResponse(string question)
        {
            try
            {
                var response = await _chatClient.CompleteChatAsync(question);
                Console.WriteLine(response);
                return response.Value.Content[0].Text ?? "ChatGPT did not provide a response.";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching ChatGPT response: {ex.Message}");
                throw;
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

        public async Task<AgileMinds.Shared.Models.Task?> GetTaskById(ulong guildId, long taskId)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // Find the linked project using the guildId
                var integration = await _context.DiscordIntegrations
                    .FirstOrDefaultAsync(d => d.DiscordServerId == guildId.ToString());

                if (integration == null)
                {
                    return null; // No project linked to the guild
                }
                // Safe casting from long to int
                if (taskId > int.MaxValue || taskId < int.MinValue)
                {
                    throw new ArgumentOutOfRangeException(nameof(taskId), $"Task ID {taskId} is out of range for an integer.");
                }

                int taskIdInt = (int)taskId;

                // Fetch the task by ID for the linked project
                var task = await _context.Tasks
                    .Include(t => t.AssignedUser) // Include related user data
                    .FirstOrDefaultAsync(t => t.ProjectId == integration.ProjectId && t.Id == taskId);

                return task;
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
        public async Task<Project?> GetProjectDetailsForGuild(ulong guildId)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // Find the linked project using the guildId
                var integration = await _context.DiscordIntegrations
                    .FirstOrDefaultAsync(d => d.DiscordServerId == guildId.ToString());

                if (integration == null)
                {
                    return null; // No project linked to the guild
                }

                // Fetch project details, including related data
                var project = await _context.Projects
                    .Include(p => p.Members)
                    .Include(p => p.Tasks)
                    .FirstOrDefaultAsync(p => p.Id == integration.ProjectId);

                return project;
            }
        }
        public class ProjectMemberDetails
        {
            public string Username { get; set; }
            public string Role { get; set; }
        }
        public async Task<List<ProjectMemberDetails>> GetMembersForGuild(ulong guildId)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // Find the linked project using the guildId
                var integration = await _context.DiscordIntegrations
                    .FirstOrDefaultAsync(d => d.DiscordServerId == guildId.ToString());

                if (integration == null)
                {
                    return new List<ProjectMemberDetails>(); // Return an empty list if no project is linked
                }

                // Fetch members of the linked project
                var members = await _context.ProjectMembers
                    .Where(pm => pm.ProjectId == integration.ProjectId)
                    .Join(
                        _context.Users,          // Join with the Users table
                        pm => pm.UserId,         // Match ProjectMember.UserId
                        u => u.Id,               // Match with User.Id
                        (pm, u) => new ProjectMemberDetails { Username = u.Username, Role = pm.Role == 0 ? "Owner" : "Member" } // Create DTO
                    )
                    .ToListAsync();

                return members;
            }
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

