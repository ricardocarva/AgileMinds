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

        [HttpGet("{projectId}")]
        public async Task<IActionResult> GetProjectById(int projectId)
        {
            var project = await _context.Projects
                .Include(p => p.Members)
                .Include(p => p.Tasks)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
            {
                return NotFound();
            }

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
                }).ToList() ?? new List<TaskDto>()
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
        public async Task<IActionResult> CreateSprintForProject(int projectId, [FromBody] Sprint sprint)
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

            sprint.ProjectId = projectId;
            sprint.Project = project;
            _context.Sprints.Add(sprint);
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
}
