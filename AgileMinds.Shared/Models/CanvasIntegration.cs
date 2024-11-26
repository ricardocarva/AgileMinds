using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AgileMinds.Shared.Models
{
    [Table("CanvasIntegrations")]  // Explicitly define the table name
    public class CanvasIntegration
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [ForeignKey("Project")]
        [Column("project_id")]
        public int ProjectId { get; set; }

        [Column("canvas_api_key")]
        public string CanvasApiKey { get; set; }

        [Column("canvas_course_id")]
        public string CanvasCourseId { get; set; }

        [Column("is_linked")]
        public bool IsLinked { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        // Navigation property to the Project
        [JsonIgnore]
        public virtual Project? Project { get; set; }
    }
}
