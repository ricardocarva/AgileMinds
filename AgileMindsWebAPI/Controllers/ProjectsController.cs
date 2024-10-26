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
using AgileMindsWebAPI.DTO;

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

            var projectDto = new DTO.ProjectDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                GameifiedApp = project.GameifiedApp,
                DiscordIntegration = project.DiscordIntegration,
                CanvasIntegration = project.CanvasIntegration,
                CreatedAt = project.CreatedAt,
                CreatedBy = project.CreatedBy,
                Members = project.Members
                .Where(m => m.User != null)
                .Select(m => new DTO.MemberDto
                {
                    UserId = m.UserId,
                    Username = m.User.Username
                }).ToList(),
                Tasks = project.Tasks.Select(t => new DTO.TaskDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Description = t.Description,
                    DueDate = t.DueDate,
                    AssignedTo = t.AssignedTo,
                    Status = t.Status.ToString()
                }).ToList()
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

        // GET: api/projects/{projectId}/tasks
        [HttpGet("{projectId}/tasks")]
        public async Task<IActionResult> GetTasksForProject(int projectId)
        {
            var tasks = await _context.Tasks
                .Where(t => t.ProjectId == projectId)
                .Include(t => t.AssignedUser)
                .ToListAsync();

            return Ok(tasks);
        }

        // POST: api/projects/{projectId}/tasks
        [HttpPost("{projectId}/tasks")]
        public async Task<IActionResult> CreateTaskForProject(int projectId, [FromBody] AgileMinds.Shared.Models.Task task)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null)
            {
                return NotFound("Project not found");
            }

            task.ProjectId = projectId;
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            return Ok(task);
        }

        // PUT: api/projects/{projectId}/tasks/{taskId}
        [HttpPut("{projectId}/tasks/{taskId}")]
        public async Task<IActionResult> UpdateTaskForProject(int projectId, int taskId, [FromBody] AgileMinds.Shared.Models.Task updatedTask)
        {
            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == taskId && t.ProjectId == projectId);

            if (task == null)
            {
                return NotFound("Task not found");
            }

            // update task fields
            task.Name = updatedTask.Name;
            task.Description = updatedTask.Description;
            task.Status = updatedTask.Status;
            task.Priority = updatedTask.Priority;
            task.DueDate = updatedTask.DueDate;
            task.Type = updatedTask.Type;
            task.Estimate = updatedTask.Estimate;
            task.AssignedTo = updatedTask.AssignedTo;
            task.SprintId = updatedTask.SprintId;

            await _context.SaveChangesAsync();

            return Ok(task);
        }

        // GET: api/projects/{projectId}/sprints
        [HttpGet("{projectId}/sprints")]
        public async Task<IActionResult> GetSprintsForProject(int projectId)
        {
            List<Sprint>? sprints = await _context.Sprints
                .Where(s => s.ProjectId == projectId)
                .ToListAsync();

            return Ok(sprints);
        }

        // GET: api/projects/{projectId}/sprints/open
        [HttpGet("{projectId}/sprints/open")]
        public async Task<IActionResult> GetOpenSprint(int projectId)
        {
            var openSprint = await _context.Sprints
                .Where(s => s.ProjectId == projectId && !s.IsCompleted)
                .SingleOrDefaultAsync();

            if (openSprint == null)
            {
                // Return a 204 No Content if no open sprint is found
                return NoContent();
            }

            return Ok(openSprint);
        }




        // GET: api/projects/{projectId}/sprints/completed
        [HttpGet("{projectId}/sprints/completed")]
        public async Task<IActionResult> GetCompletedSprints(int projectId)
        {
            var completedSprints = await _context.Sprints
                .Where(s => s.ProjectId == projectId && s.IsCompleted)
                .ToListAsync();

            return Ok(completedSprints);
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
