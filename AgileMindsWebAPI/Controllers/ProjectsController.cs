/*using AgileMindsWebAPI.Data;
using AgileMinds.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgileMindsWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProjectsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: api/projects
        [HttpPost]
        public async Task<IActionResult> CreateProject([FromBody] Project project)
        {
            if (project == null)
            {
                return BadRequest("Project data is null");
            }

            project.CreatedAt = DateTime.UtcNow;  // Set the created time to now
            _context.Projects.Add(project);       // Add the project to the database
            await _context.SaveChangesAsync();    // Save changes to the database

            return Ok(project);                   // Return the created project data
        }

        // GET: api/projects
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserProjects(int userId)
        {
            // Assuming CreatedBy is the user who owns the project
            var projects = await _context.Projects
                .Where(p => p.CreatedBy == userId)
                .ToListAsync();

            return Ok(projects);
        }

    }
}
*/

using System.Net.Http.Headers;
using System.Text.Json;

using AgileMinds.Shared.Models;

using AgileMindsWebAPI.Data;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgileMindsWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProjectsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateProject([FromBody] ProjectDto projectDto)
        {
            if (projectDto == null)
            {
                return BadRequest("Project data is null");
            }

            var project = new Project
            {
                Name = projectDto.Name,
                Description = projectDto.Description,
                GameifiedApp = projectDto.GameifiedApp,
                DiscordIntegration = projectDto.DiscordIntegration,
                CanvasIntegration = projectDto.CanvasIntegration,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = projectDto.CreatedBy
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            // add creator as owner
            var projectMember = new ProjectMember
            {
                ProjectId = project.Id,
                UserId = project.CreatedBy,
                Role = (int)ProjectRole.Owner
            };

            _context.ProjectMembers.Add(projectMember);
            await _context.SaveChangesAsync();

            // Return the newly created project DTO
            var createdProjectDto = new ProjectDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                GameifiedApp = project.GameifiedApp,
                DiscordIntegration = project.DiscordIntegration,
                CanvasIntegration = project.CanvasIntegration,
                CreatedAt = project.CreatedAt,
                CreatedBy = project.CreatedBy
            };

            return Ok(createdProjectDto);
        }


        // GET: api/projects/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserProjects(int userId)
        {
            var projects = await _context.Projects
                .Where(p => p.CreatedBy == userId || p.Members.Any(m => m.UserId == userId))
                .ToListAsync();

            return Ok(projects);
        }

        //// GET: api/projects/user/{userId}
        //[HttpGet("todos/user/{userId}")]
        //public async Task<IActionResult> GetUserTodos(int userId)
        //{
        //    var todos = await _context.Todos
        //        .Where(p => p.UserID == userId)
        //        .ToListAsync();

        //    return Ok(todos);
        //}

        [HttpGet("{projectId}")]
        public async Task<IActionResult> GetProjectById(int projectId)
        {
            // Fetch the project
            var project = await _context.Projects
                .Include(p => p.Members)
                .Include(p => p.Tasks)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
            {
                return NotFound();
            }

            // Fetch the Canvas integration
            var canvasIntegration = await _context.CanvasIntegrations
                .FirstOrDefaultAsync(ci => ci.ProjectId == projectId);

            // Fetch the Discord integration
            var discordIntegration = await _context.DiscordIntegrations
                .FirstOrDefaultAsync(di => di.ProjectId == projectId);

            // Create the DTO to return
            var projectDto = new ProjectDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                GameifiedApp = project.GameifiedApp,
                DiscordIntegration = project.DiscordIntegration,
                CanvasIntegration = project.CanvasIntegration,
                CreatedAt = project.CreatedAt,
                CreatedBy = project.CreatedBy,
                Members = project.Members?.Where(m => m.User != null)
            .Select(m => new MemberDto
            {
                UserId = m.UserId,
                Role = m.Role,
                Username = m.User.Username
            }).ToList() ?? new List<MemberDto>(),
                Tasks = project.Tasks?.Select(t => new TaskDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Description = t.Description,
                    DueDate = t.DueDate,
                    AssignedTo = t.AssignedTo,
                    Status = t.Status.ToString(),
                    SprintId = t.SprintId
                }).ToList() ?? new List<TaskDto>(),
                // Add the Canvas and Discord Integration objects
                CanvasIntegrationDetails = canvasIntegration != null ? new CanvasIntegrationDto
                {
                    CanvasApiKey = canvasIntegration.CanvasApiKey,
                    CanvasCourseId = canvasIntegration.CanvasCourseId
                } : null,
                DiscordIntegrationDetails = discordIntegration != null ? new DiscordIntegrationDto
                {
                    DiscordServerId = discordIntegration.DiscordServerId,
                    ProjectSecret = discordIntegration.DiscordProjectSecret
                } : null

            };
            return Ok(projectDto);
        }

        // GET: api/projects/{projectId}/members
        [HttpGet("{projectId}/members")]
        public async Task<IActionResult> GetMembersForProject(int projectId)
        {
            var members = await _context.ProjectMembers
               .Where(pm => pm.ProjectId == projectId)
               .Include(pm => pm.User)
               .Select(pm => pm.User)
               .ToListAsync();

            return Ok(members);
        }

        // GET: api/projects/{projectId}/members/detailed
        [HttpGet("{projectId}/members/detailed")]
        public async Task<IActionResult> GetMembersDetailedForProject(int projectId)
        {
            var members = await _context.ProjectMembers
               .Where(pm => pm.ProjectId == projectId)
               .Include(pm => pm.User)
               .ToListAsync();

            return Ok(members);
        }

        //// GET: api/projects/{projectId}/tasks
        //[HttpGet("{projectId}/tasks")]
        //public async Task<IActionResult> GetTasksForProject(int projectId)
        //{
        //    var tasks = await _context.Tasks
        // .Where(t => t.ProjectId == projectId)
        // .Include(t => t.Project)
        // .Include(t => t.AssignedUser)
        // .Include(t => t.Sprint)
        // .ToListAsync();

        //    if (tasks == null || !tasks.Any())
        //    {
        //        return NoContent();
        //    }

        //    var tasksDto = tasks.Select(t => new TaskDto
        //    {
        //        Id = t.Id,
        //        Name = t.Name,
        //        Description = t.Description,
        //        DueDate = t.DueDate,
        //        AssignedTo = t.AssignedTo,
        //        AssignedUser = t.AssignedUser != null ? new MemberDto
        //        {
        //            UserId = t.AssignedUser.Id,
        //            Username = t.AssignedUser.Username
        //        } : null,
        //        Status = t.Status.ToString(),
        //        SprintId = t.SprintId
        //    }).ToList();

        //    return Ok(tasksDto);
        //}

        // GET: api/projects/{projectId}/tasks
        [HttpGet("{projectId}/tasks")]
        public async Task<IActionResult> GetTasksForProject(int projectId)
        {
            var tasks = await _context.Tasks
                .Where(t => t.ProjectId == projectId)
                .Include(t => t.AssignedUser)
                .ToListAsync();

            var taskDtos = tasks.Select(t => new TaskDto
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                DueDate = t.DueDate,
                AssignedTo = t.AssignedTo,
                AssignedUser = t.AssignedUser != null ? new UserDto
                {
                    Id = t.AssignedUser.Id,
                    Username = t.AssignedUser.Username
                } : null,
                Status = t.Status.ToString(),
                Priority = t.Priority,
                Type = t.Type,
                Estimate = t.Estimate,
                SprintId = t.SprintId
            }).ToList();

            return Ok(taskDtos);
        }

        // POST: api/projects/{projectId}/tasks
        [HttpPost("{projectId}/tasks")]
        public async Task<IActionResult> CreateTaskForProject(int projectId, [FromBody] AgileMinds.Shared.Models.Task newTask)
        {
            if (newTask == null)
            {
                return BadRequest("Task data is null");
            }

            var project = await _context.Projects.FindAsync(projectId);
            if (project == null)
            {
                return NotFound("Project not found");
            }

            newTask.ProjectId = projectId;
            _context.Tasks.Add(newTask);
            await _context.SaveChangesAsync();

            // Fetch the task with the assigned user
            var task = await _context.Tasks
                .Include(t => t.AssignedUser)
                .FirstOrDefaultAsync(t => t.Id == newTask.Id);

            var taskDto = new TaskDto
            {
                Id = task.Id,
                Name = task.Name,
                Description = task.Description,
                DueDate = task.DueDate,
                AssignedTo = task.AssignedTo,
                AssignedUser = task.AssignedUser != null ? new UserDto
                {
                    Id = task.AssignedUser.Id,
                    Username = task.AssignedUser.Username
                } : null,
                Status = task.Status.ToString(),
                Priority = task.Priority,
                Type = task.Type,
                Estimate = task.Estimate,
                SprintId = task.SprintId
            };

            return Ok(taskDto);
        }
        // GET: api/projects/{projectId}/tasks/{taskId}
        [HttpGet("{projectId}/tasks/{taskId}")]
        public async Task<IActionResult> GetTaskById(int projectId, int taskId)
        {
            var task = await _context.Tasks
                .Include(t => t.AssignedUser)
                .FirstOrDefaultAsync(t => t.ProjectId == projectId && t.Id == taskId);

            if (task == null)
            {
                return NotFound("Task not found");
            }

            return Ok(task);
        }

        // PUT: api/projects/{projectId}/tasks/{taskId}
        [HttpPut("{projectId}/tasks/{taskId}")]
        public async Task<IActionResult> UpdateTaskForProject(int projectId, int taskId, [FromBody] AgileMinds.Shared.Models.Task updatedTask)
        {
            if (updatedTask == null || taskId != updatedTask.Id)
            {
                return BadRequest("Invalid task data");
            }

            var existingTask = await _context.Tasks
                .FirstOrDefaultAsync(t => t.Id == taskId && t.ProjectId == projectId);

            if (existingTask == null)
            {
                return NotFound("Task not found");
            }

            // Update task fields
            existingTask.Name = updatedTask.Name;
            existingTask.Description = updatedTask.Description;
            existingTask.Status = updatedTask.Status;
            existingTask.Priority = updatedTask.Priority;
            existingTask.DueDate = updatedTask.DueDate;
            existingTask.Type = updatedTask.Type;
            existingTask.Estimate = updatedTask.Estimate;
            existingTask.AssignedTo = updatedTask.AssignedTo;
            existingTask.SprintId = updatedTask.SprintId;

            await _context.SaveChangesAsync();

            // Fetch the updated task with the assigned user
            var task = await _context.Tasks
                .Include(t => t.AssignedUser)
                .FirstOrDefaultAsync(t => t.Id == updatedTask.Id);

            var taskDto = new TaskDto
            {
                Id = task.Id,
                Name = task.Name,
                Description = task.Description,
                DueDate = task.DueDate,
                AssignedTo = task.AssignedTo,
                AssignedUser = task.AssignedUser != null ? new UserDto
                {
                    Id = task.AssignedUser.Id,
                    Username = task.AssignedUser.Username
                } : null,
                Status = task.Status.ToString(),
                Priority = task.Priority,
                Type = task.Type,
                Estimate = task.Estimate,
                SprintId = task.SprintId
            };

            return Ok(taskDto);
        }

        // GET: api/projects/{projectId}/sprints
        [HttpGet("{projectId}/sprints")]
        public async Task<IActionResult> GetSprintsForProject(int projectId, [FromQuery] bool onlyOpen = false)
        {
            var sprintsQuery = _context.Sprints.Where(s => s.ProjectId == projectId);

            if (onlyOpen)
            {
                sprintsQuery = sprintsQuery.Where(s => !s.IsCompleted);
            }

            var sprints = await sprintsQuery.Include(s => s.Tasks).ToListAsync();

            if (sprints == null || !sprints.Any())
            {
                return NoContent();
            }

            var sprintsDto = sprints.Select(s => new SprintDto
            {
                Id = s.Id,
                Name = s.Name,
                StartDate = s.StartDate,
                EndDate = s.EndDate,
                IsStarted = s.IsStarted,
                IsCompleted = s.IsCompleted,
                ProjectId = s.ProjectId,
                Tasks = s.Tasks.Select(t => new TaskDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Description = t.Description,
                    DueDate = t.DueDate,
                    AssignedTo = t.AssignedTo,
                    Status = t.Status.ToString(),
                    SprintId = t.SprintId
                }).ToList()
            }).ToList();

            return Ok(sprintsDto);
        }


        // GET: api/projects/{projectId}/sprints/open
        [HttpGet("{projectId}/sprints/open")]
        public async Task<IActionResult> GetOpenSprint(int projectId)
        {
            var openSprintDto = await _context.Sprints
        .Where(s => s.ProjectId == projectId && !s.IsCompleted)
        .Select(openSprint => new SprintDto
        {
            Id = openSprint.Id,
            Name = openSprint.Name,
            StartDate = openSprint.StartDate,
            EndDate = openSprint.EndDate,
            IsStarted = openSprint.IsStarted,
            IsCompleted = openSprint.IsCompleted,
            ProjectId = openSprint.ProjectId,
            Tasks = openSprint.Tasks.Select(t => new TaskDto
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                DueDate = t.DueDate,
                AssignedTo = t.AssignedTo,
                Status = t.Status.ToString(),
                SprintId = t.SprintId
            }).ToList()
        })
        .SingleOrDefaultAsync();

            if (openSprintDto == null)
            {
                // Return a 204 No Content if no open sprint is found
                return NoContent();
            }
            return Ok(openSprintDto);
        }

        // GET: api/projects/{projectId}/sprints/completed
        [HttpGet("{projectId}/sprints/completed")]
        public async Task<IActionResult> GetCompletedSprints(int projectId)
        {
            var completedSprintsDto = await _context.Sprints
                .Where(s => s.ProjectId == projectId && s.IsCompleted)
                .Select(s => new SprintDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    IsStarted = s.IsStarted,
                    IsCompleted = s.IsCompleted,
                    ProjectId = s.ProjectId,
                    Tasks = s.Tasks.Select(t => new TaskDto
                    {
                        Id = t.Id,
                        Name = t.Name,
                        Description = t.Description,
                        DueDate = t.DueDate,
                        AssignedTo = t.AssignedTo,
                        AssignedUser = t.AssignedUser != null ? new UserDto
                        {
                            Id = t.AssignedUser.Id,
                            Username = t.AssignedUser.Username
                        } : null,
                        Status = t.Status.ToString(),
                        SprintId = t.SprintId
                    }).ToList()
                })
                .ToListAsync();

            if (completedSprintsDto == null || completedSprintsDto.Count == 0)
            {
                return NoContent();
            }

            return Ok(completedSprintsDto);
        }


        // POST: api/projects/{projectId}/sprints
        [HttpPost("{projectId}/sprints")]
        public async Task<IActionResult> CreateSprintForProject(int projectId, [FromBody] SprintDto sprint)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null)
            {
                return NotFound("Project not found");
            }

            // Check if an open sprint already exists
            var openSprint = await _context.Sprints
                .Where(s => s.ProjectId == projectId && !s.IsCompleted)
                    .Include(s => s.Tasks)
                .Include(s => s.Project)
                .SingleOrDefaultAsync();

            if (openSprint != null)
            {
                // Return conflict (HTTP 409) with a specific message indicating that a sprint is already open
                return Conflict("A sprint is already open for this project.");
            }

            var newSprint = new AgileMinds.Shared.Models.Sprint
            {
                Name = sprint.Name,
                StartDate = sprint.StartDate,
                EndDate = sprint.EndDate,
                ProjectId = projectId,
                Project = project
            };

            _context.Sprints.Add(newSprint);
            await _context.SaveChangesAsync();

            return Ok(sprint);
        }

        // PUT: api/projects/{sprintId}/start
        [HttpPut("{sprintId}/start")]
        public async Task<IActionResult> StartSprint(int sprintId)
        {
            var sprint = await _context.Sprints.FindAsync(sprintId);
            if (sprint == null)
            {
                return NotFound("Sprint not found");
            }

            if (sprint.IsStarted)
            {
                return BadRequest("Sprint has already been started.");
            }

            if (sprint.IsCompleted)
            {
                return BadRequest("Sprint has already been completed.");
            }

            sprint.IsStarted = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }
        // PUT: api/projects/{sprintId}/complete
        [HttpPut("{sprintId}/complete")]
        public async Task<IActionResult> CompleteSprint(int sprintId)
        {
            var sprint = await _context.Sprints.FindAsync(sprintId);
            if (sprint == null)
            {
                return NotFound("Sprint not found");
            }

            if (!sprint.IsStarted)
            {
                return BadRequest("Sprint has not been started yet.");
            }

            if (sprint.IsCompleted)
            {
                return BadRequest("Sprint has already been completed.");
            }

            sprint.IsCompleted = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        //// POST: api/projects/{projectId}/sprints
        //[HttpPost("{projectId}/sprints")]
        //public async Task<IActionResult> CreateSprintsForProject(int projectId, [FromBody] AgileMinds.Shared.Models.Sprint sprint)
        //{
        //    var project = await _context.Projects.FindAsync(projectId);
        //    if (project == null)
        //    {
        //        return NotFound("Project not found");
        //    }
        //    sprint.ProjectId = projectId;
        //    sprint.Project = project;
        //    _context.Sprints.Add(sprint);
        //    await _context.SaveChangesAsync();

        //    return Ok(sprint);
        //}

        [HttpPost("{projectId}/invitations")]
        public async Task<IActionResult> InviteUserToProject(int projectId, [FromBody] InvitationDto invitation)
        {
            var inv = new Invitation
            {
                ProjectId = projectId,
                InvitorId = invitation.InvitorId,
                InviteeId = invitation.InviteeId,
                CreatedAt = DateTime.UtcNow,

                IsAccepted = false
            };

            _context.Invitations.Add(inv);
            await _context.SaveChangesAsync();

            // create a notification for the invitee
            var notification = new Notification
            {
                UserId = inv.InviteeId,
                Message = $"You have been invited to join the project: {projectId}.",
                CreatedAt = DateTime.UtcNow,
                InvitationId = inv.Id
            };
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // PUT: api/projects/{projectId}/members/{userId}/role
        [HttpPut("{projectId}/members/{userId}/role")]
        public async Task<IActionResult> UpdateMemberRole(int projectId, int userId, [FromBody] UpdateMemberRoleDto dto)
        {
            var member = await _context.ProjectMembers
                .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);

            if (member == null)
            {
                return NotFound("Project member not found");
            }

            // Update the role
            member.Role = dto.Role;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/projects/{projectId}/discordintegration
        [HttpGet("{projectId}/discordintegration")]
        public async Task<IActionResult> GetDiscordIntegration(int projectId)
        {
            var discordIntegration = await _context.DiscordIntegrations
                .FirstOrDefaultAsync(d => d.ProjectId == projectId);

            if (discordIntegration == null)
            {
                return NotFound($"No Discord Integration Found For Project {projectId}");
            }

            return Ok(discordIntegration);
        }

        [HttpPost("{projectId}/discordintegration")]
        public async Task<IActionResult> SaveDiscordIntegration(int projectId, [FromBody] DiscordIntegration discordIntegration)
        {
            if (discordIntegration == null)
            {
                return BadRequest("Discord integration data is null");
            }

            var existingIntegration = await _context.DiscordIntegrations
                .FirstOrDefaultAsync(d => d.ProjectId == projectId);

            if (existingIntegration == null)
            {
                discordIntegration.ProjectId = projectId;
                discordIntegration.CreatedAt = DateTime.UtcNow;
                discordIntegration.UpdatedAt = DateTime.UtcNow;
                discordIntegration.IsLinked = false; // Initially not linked

                _context.DiscordIntegrations.Add(discordIntegration);
            }
            else
            {
                existingIntegration.DiscordServerId = discordIntegration.DiscordServerId;
                existingIntegration.DiscordBotToken = discordIntegration.DiscordBotToken;
                existingIntegration.DiscordChannelId = discordIntegration.DiscordChannelId;
                existingIntegration.UpdatedAt = DateTime.UtcNow;
                existingIntegration.DiscordProjectSecret = discordIntegration.DiscordProjectSecret;
                existingIntegration.IsLinked = discordIntegration.IsLinked;

                _context.DiscordIntegrations.Update(existingIntegration);
            }

            await _context.SaveChangesAsync();
            return Ok(discordIntegration);
        }

        [HttpGet("checkguild/{guildId}")]
        public async Task<IActionResult> CheckIfGuildIsLinked(string guildId)
        {
            var integration = await _context.DiscordIntegrations
                .FirstOrDefaultAsync(di => di.DiscordServerId == guildId);

            if (integration == null || !integration.IsLinked)
            {
                return NotFound();
            }

            return Ok();
        }

        [HttpPost("verifysecret")]
        public async Task<IActionResult> VerifySecret([FromBody] VerifySecretDto dto)
        {
            var integration = await _context.DiscordIntegrations
                .FirstOrDefaultAsync(di => di.DiscordServerId == dto.GuildId);

            if (integration == null || integration.DiscordProjectSecret != dto.Secret)
            {
                return BadRequest("Incorrect secret");
            }

            // Mark as linked
            integration.IsLinked = true;
            _context.DiscordIntegrations.Update(integration);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("{projectId}/canvascourses")]
        public async Task<IActionResult> GetCanvasCourses(int projectId)
        {
            var canvasIntegration = await _context.CanvasIntegrations
                .FirstOrDefaultAsync(ci => ci.ProjectId == projectId);

            if (canvasIntegration == null || string.IsNullOrEmpty(canvasIntegration.CanvasApiKey))
            {
                return BadRequest("Canvas API key not found for the project.");
            }

            var apiKey = canvasIntegration.CanvasApiKey;
            var coursesUrl = "https://ufl.instructure.com/api/v1/courses";

            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                var response = await client.GetAsync(coursesUrl);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var courses = JsonSerializer.Deserialize<List<CourseDto>>(jsonResponse, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    var filteredCourses = courses
                        .Select(c => new CourseDto
                        {
                            Id = c.Id,
                            Name = c.Name
                        })
                        .ToList();

                    return Ok(filteredCourses);
                }
                else
                {
                    Console.WriteLine("here we go\n\n");
                    return BadRequest("Error fetching courses from Canvas.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error+: {ex.Message}");
            }
        }



        [HttpGet("{projectId}/canvasintegration")]
        public async Task<IActionResult> GetCanvasIntegration(int projectId)
        {
            var canvasIntegration = await _context.CanvasIntegrations
                .FirstOrDefaultAsync(c => c.ProjectId == projectId);

            if (canvasIntegration == null)
            {
                return NotFound($"No Canvas Integration Found For Project {projectId}");
            }

            return Ok(canvasIntegration);
        }

        [HttpPost("{projectId}/canvasintegration")]
        public async Task<IActionResult> SaveCanvasIntegration(int projectId, [FromBody] CanvasIntegrationDto canvasIntegrationDto)
        {
            if (canvasIntegrationDto == null)
            {
                return BadRequest("Canvas integration data is null");
            }

            var existingIntegration = await _context.CanvasIntegrations
                .FirstOrDefaultAsync(c => c.ProjectId == projectId);

            if (existingIntegration == null)
            {
                var newCanvasIntegration = new CanvasIntegration
                {
                    ProjectId = projectId,
                    CanvasApiKey = canvasIntegrationDto.CanvasApiKey,
                    CanvasCourseId = canvasIntegrationDto.CanvasCourseId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.CanvasIntegrations.Add(newCanvasIntegration);
            }
            else
            {
                existingIntegration.CanvasApiKey = canvasIntegrationDto.CanvasApiKey;
                existingIntegration.CanvasCourseId = canvasIntegrationDto.CanvasCourseId;
                existingIntegration.UpdatedAt = DateTime.UtcNow;
                _context.CanvasIntegrations.Update(existingIntegration);
            }

            await _context.SaveChangesAsync();
            return Ok(canvasIntegrationDto);
        }

        [HttpGet("{projectId}/canvasassignments")]
        public async Task<IActionResult> GetCanvasAssignments(int projectId)
        {
            // Fetch the Canvas integration for the project
            var canvasIntegration = await _context.CanvasIntegrations
                .FirstOrDefaultAsync(ci => ci.ProjectId == projectId);

            if (canvasIntegration == null || string.IsNullOrEmpty(canvasIntegration.CanvasApiKey))
            {
                return BadRequest("Canvas API key not found for the project.");
            }

            var apiKey = canvasIntegration.CanvasApiKey;
            var courseId = canvasIntegration.CanvasCourseId; // Ensure the course ID is stored in CanvasIntegration
            var assignmentsUrl = $"https://ufl.instructure.com/api/v1/courses/{courseId}/assignments";

            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                var response = await client.GetAsync(assignmentsUrl);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var assignments = JsonSerializer.Deserialize<List<AssignmentDto>>(jsonResponse, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return Ok(assignments);
                }
                else
                {
                    return BadRequest("Error fetching assignments from Canvas.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
        [HttpGet("{projectId}/integrations")]
        public async Task<IActionResult> GetProjectIntegrations(int projectId)
        {
            var discordIntegration = await _context.DiscordIntegrations
                .FirstOrDefaultAsync(di => di.ProjectId == projectId);

            var canvasIntegration = await _context.CanvasIntegrations
                .FirstOrDefaultAsync(ci => ci.ProjectId == projectId);

            var integrationDto = new IntegrationDto
            {
                DiscordIntegration = discordIntegration,
                CanvasIntegration = canvasIntegration
            };

            return Ok(integrationDto);
        }
    }


    public class AddMemberModel
    {
        public string Username { get; set; }
    }


    public class InvitationDto
    {
        public int ProjectId { get; set; }
        public int InvitorId { get; set; }
        public int InviteeId { get; set; }

        //public int Id { get; set; }
    }

    public class VerifySecretDto
    {
        public string GuildId { get; set; }
        public string Secret { get; set; }
    }

    public class CanvasIntegrationDto
    {
        public int ProjectId { get; set; }       // Project to link with Canvas
        public string CanvasApiKey { get; set; }  // Canvas API key from user input
        public string CanvasCourseId { get; set; }   // Selected course ID
    }

    public class CanvasCourse
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class CourseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class AssignmentDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? DueAt { get; set; }
        public int PointsPossible { get; set; }
    }

    public class ProjectDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool GameifiedApp { get; set; }
        public bool DiscordIntegration { get; set; }
        public bool CanvasIntegration { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CreatedBy { get; set; }
        public List<MemberDto> Members { get; set; }
        public List<TaskDto>? Tasks { get; set; }

        // New fields for integration details
        public CanvasIntegrationDto? CanvasIntegrationDetails { get; set; }
        public DiscordIntegrationDto? DiscordIntegrationDetails { get; set; }
    }

    public class DiscordIntegrationDto
    {
        public string DiscordServerId { get; set; }
        public string ProjectSecret { get; set; }
    }
    public class IntegrationDto
    {
        public DiscordIntegration? DiscordIntegration { get; set; }
        public CanvasIntegration? CanvasIntegration { get; set; }
    }
}
