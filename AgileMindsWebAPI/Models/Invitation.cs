using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AgileMindsWebAPI.Models
{
    [Table("Invitations")]
    public class Invitation
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [ForeignKey("Project")]
        [Column("project_id")]
        public int ProjectId { get; set; }

        [JsonIgnore]
        public virtual Project Project { get; set; }

        [ForeignKey("Invitor")]
        [Column("invitor_id")]
        public int InvitorId { get; set; }

        [JsonIgnore]
        public virtual User Invitor { get; set; }

        [ForeignKey("Invitee")]
        [Column("invitee_id")]
        public int InviteeId { get; set; }

        [JsonIgnore]
        public virtual User Invitee { get; set; }

        [Column("is_accepted")]
        public bool IsAccepted { get; set; } = false;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("accepted_at")]
        public DateTime? AcceptedAt { get; set; }
    }
}
