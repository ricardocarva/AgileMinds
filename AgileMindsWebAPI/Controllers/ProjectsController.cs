/*using AgileMindsWebAPI.Data;
using AgileMindsWebAPI.Models;
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

using AgileMindsWebAPI.Data;
using AgileMindsWebAPI.DTO;
using AgileMindsWebAPI.Models;

using Amazon.Runtime;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgileMindsWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IAmazonSimpleNotificationService _snsClient;
        private readonly string _snsTopicArn;
        private readonly IConfiguration _configuration;

        public ProjectsController(ApplicationDbContext context, IAmazonSimpleNotificationService snsClient, IConfiguration configuration)
        {
            _context = context;
            _snsClient = snsClient;
            _configuration = configuration;
            _snsTopicArn = _configuration["AWS:SnsTopicArn"];

            // Initialize AWS SNS client with explicit credentials
            var awsAccessKey = _configuration["AWS:AccessKey"];
            var awsSecretKey = _configuration["AWS:SecretKey"];
            var awsRegion = _configuration["AWS:Region"];

            var credentials = new BasicAWSCredentials(awsAccessKey, awsSecretKey);
            var config = new AmazonSimpleNotificationServiceConfig
            {
                RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(awsRegion)
            };
            _snsClient = new AmazonSimpleNotificationServiceClient(credentials, config);

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
                Members = project.Members.Select(m => new DTO.MemberDto
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
                .Select(pm => new { pm.User.Id, pm.User.Username })
                .ToListAsync();

            return Ok(members);
        }

        // GET: api/projects/{projectId}/tasks
        [HttpGet("{projectId}/tasks")]
        public async Task<IActionResult> GetTasksForProject(int projectId)
        {
            var tasks = await _context.Tasks
                .Where(t => t.ProjectId == projectId)
                .ToListAsync();

            return Ok(tasks);
        }

        // POST: api/projects/{projectId}/tasks
        [HttpPost("{projectId}/tasks")]
        public async Task<IActionResult> CreateTaskForProject(int projectId, [FromBody] Models.Task task)
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
        public async Task<IActionResult> UpdateTaskForProject(int projectId, int taskId, [FromBody] Models.Task updatedTask)
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
            var sprints = await _context.Sprints
                .Where(s => s.ProjectId == projectId)
                .ToListAsync();

            return Ok(sprints);
        }

        [HttpPost("{projectId}/invitations")]
        public async Task<IActionResult> InviteUserToProject(int projectId, [FromBody] InvitationDto invitation)
        {
            if (invitation == null)
            {
                return BadRequest("Invalid invitation data.");
            }
            // Check if the invitation already exists to avoid duplication
            //var existingInvitation = await _context.Invitations
            //    .FirstOrDefaultAsync(i => i.ProjectId == projectId && i.InviteeId == invitation.InviteeId);

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

            // Create a notification for the invitee
            var notification = new Notification
            {
                UserId = inv.InviteeId,
                Message = $"You have been invited to join the project: {projectId}.",
                CreatedAt = DateTime.UtcNow,
                InvitationId = inv.Id
            };
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // Send an email notification via SNS if the username is an email address
            var invitee = await _context.Users.FindAsync(inv.InviteeId);
            if (invitee != null && invitee.Username.Contains("@"))
            {
                if (string.IsNullOrEmpty(_snsTopicArn))
                {
                    Console.WriteLine("SNS Topic ARN is not configured correctly.");
                    return StatusCode(500, "SNS Topic ARN is not configured correctly.");
                }

                Console.WriteLine($"SNS Topic ARN: {_snsTopicArn}");

                var publishRequest = new PublishRequest
                {
                    TopicArn = _snsTopicArn,
                    Message = notification.Message,
                    Subject = "You have a new project invitation",
                    MessageAttributes = new Dictionary<string, MessageAttributeValue>
            {
                { "email", new MessageAttributeValue { DataType = "String", StringValue = invitee.Username } }
            }
                };

                try
                {
                    var response = await _snsClient.PublishAsync(publishRequest);
                    Console.WriteLine($"SNS Publish Response: MessageId - {response.MessageId}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send SNS notification: {ex.Message}");
                    return StatusCode(500, $"Failed to send SNS notification: {ex.Message}");
                }
            }

            return Ok(inv);
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
