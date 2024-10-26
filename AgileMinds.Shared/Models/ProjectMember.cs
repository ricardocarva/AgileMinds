using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgileMinds.Shared.Models
{
    public class ProjectMember
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [ForeignKey("Project")]
        [Column("project_id")]
        public int ProjectId { get; set; }

        [ForeignKey("User")]
        [Column("user_id")]
        public int UserId { get; set; }

        [Column("role")]
        public int Role { get; set; } = (int)ProjectRole.Member;

        // Navigation properties
        public virtual Project Project { get; set; }
        public virtual User User { get; set; }
    }

    public enum ProjectRole
    {
        Owner = 0,
        Member = 1
    }
}
