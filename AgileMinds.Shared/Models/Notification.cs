using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AgileMinds.Shared.Models
{
    [Table("Notifications")]
    public class Notification
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [ForeignKey("User")]
        [Column("user_id")]
        public int UserId { get; set; }

        [JsonIgnore]
        public virtual User User { get; set; }

        [Column("message")]
        [Required]
        [StringLength(255)]
        public string Message { get; set; }

        [Column("is_read")]
        public bool IsRead { get; set; } = false;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Optional reference to an Invitation
        [ForeignKey("Invitation")]
        [Column("invitation_id")]
        public int? InvitationId { get; set; }

        [JsonIgnore]
        public virtual Invitation Invitation { get; set; }
    }
}
