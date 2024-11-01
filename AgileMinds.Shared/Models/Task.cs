using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AgileMinds.Shared.Models
{
    public class Task
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [ForeignKey("Project")]
        [Column("project_id")]
        public int ProjectId { get; set; }

        [ForeignKey("AssignedUser")]
        [Column("assigned_to")]
        public int? AssignedTo { get; set; }

        [ForeignKey("Sprint")]
        [Column("sprint_id")]
        public int? SprintId { get; set; }

        [ForeignKey("Creator")]
        [Column("created_by")]
        public int CreatedBy { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("due_date")]
        public DateTime? DueDate { get; set; }

        [Column("status")]
        [Required]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TaskStatus Status { get; set; } = TaskStatus.Pending;

        [Column("priority")]
        public int Priority { get; set; } = 0;  // Default to "Low"

        [Column("type")]
        [StringLength(50)]
        public string Type { get; set; } = "Feature";

        [Column("estimate")]
        public int? Estimate { get; set; }

        // Navigation properties
        [JsonIgnore]
        public virtual Project? Project { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public virtual User? AssignedUser { get; set; }

        public virtual User? Creator { get; set; }

        public virtual Sprint? Sprint { get; set; }

        [JsonIgnore]
        public List<Tag>? Tags { get; set; }
        [JsonIgnore]
        public List<TaskTag> TaskTags { get; set; } = new List<TaskTag>();
    }

    public enum TaskStatus
    {
        Pending = 0,
        InProgress = 1,
        Completed = 2
    }
}