using AgileMinds.Shared.Models;

using AgileMindsWebAPI.Data;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgileMindsWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvitationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public InvitationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: api/invitations
        [HttpPost]
        public async Task<IActionResult> CreateInvitation([FromBody] Invitation invitation)
        {
            if (invitation == null)
            {
                return BadRequest("Invalid invitation data.");
            }

            invitation.CreatedAt = DateTime.UtcNow;
            invitation.IsAccepted = false;
            Console.WriteLine(invitation.ToString());
            _context.Invitations.Add(invitation);
            await _context.SaveChangesAsync();

            // create a notification for the invitee
            var notification = new Notification
            {
                UserId = invitation.InviteeId,
                Message = $"You have been invited to join the project with ID: {invitation.ProjectId}.",
                CreatedAt = DateTime.UtcNow,
                InvitationId = invitation.Id
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return Ok(invitation);
        }

        // GET: api/invitations/{inviteeId}
        [HttpGet("{inviteeId}")]
        public async Task<IActionResult> GetInvitationsForUser(int inviteeId)
        {
            var invitations = await _context.Invitations
                .Where(i => i.InviteeId == inviteeId && !i.IsAccepted)
                .Include(i => i.Project)
                .Include(i => i.Invitor)
                .ToListAsync();

            return Ok(invitations);
        }

        // PUT: api/invitations/{invitationId}/accept
        [HttpPut("{invitationId}/accept")]
        public async Task<IActionResult> AcceptInvitation(int invitationId)
        {
            // find the invitation
            var invitation = await _context.Invitations.FindAsync(invitationId);
            if (invitation == null)
            {
                return NotFound("Invitation not found.");
            }

            // mark the invitation as accepted
            invitation.IsAccepted = true;
            await _context.SaveChangesAsync();

            // add the invitee to the project as a member
            var projectMember = new ProjectMember
            {
                ProjectId = invitation.ProjectId,
                UserId = invitation.InviteeId,
                Role = (int)ProjectRole.Member
            };
            _context.ProjectMembers.Add(projectMember);

            // mark the related notification as read
            var notification = await _context.Notifications.FirstOrDefaultAsync(n => n.InvitationId == invitationId);
            if (notification != null)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();

            return Ok();
        }

    }
}
