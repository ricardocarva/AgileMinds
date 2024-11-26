using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AgileMinds.Shared.Models
{
    public class Project
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public string? Name { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("gameified_app")]
        public bool GameifiedApp { get; set; }

        [Column("discord_integration")]
        public bool DiscordIntegration { get; set; }

        [Column("canvas_integration")]
        public bool CanvasIntegration { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [ForeignKey("CreatedBy")]
        [Column("created_by")]
        public int CreatedBy { get; set; }


        // Navigation property for members of the project

        public List<ProjectMember> Members { get; set; } = new List<ProjectMember>();

        // Navigation property for tasks in the project
        [JsonIgnore]
        public List<Task> Tasks { get; set; } = new List<Task>();
    }
}
