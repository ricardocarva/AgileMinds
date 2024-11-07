using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AgileMinds.Shared.Models
{
    public class Tag
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        // Navigation property for the tasks this tag is associated with
        [JsonIgnore]
        public List<TaskTag> TaskTags { get; set; } = new List<TaskTag>();
    }

    // join to tags as many to many
    public class TaskTag
    {
        [ForeignKey("Task")]
        [Column("task_id")]
        public int TaskId { get; set; }

        [JsonIgnore]
        public Task Task { get; set; }

        [ForeignKey("Tag")]
        [Column("tag_id")]
        public int TagId { get; set; }

        [JsonIgnore]
        public Tag Tag { get; set; }
    }
}
