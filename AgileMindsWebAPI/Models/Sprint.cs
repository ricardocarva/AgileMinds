using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgileMindsWebAPI.Models
{
    public class Sprint
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [Column("start_date")]
        public DateTime StartDate { get; set; }

        [Column("end_date")]
        public DateTime EndDate { get; set; }

        [ForeignKey("Project")]
        [Column("project_id")]
        public int ProjectId { get; set; }

        // Navigation property to the related Project
        public virtual Project Project { get; set; }

        // Navigation property to the tasks associated with this sprint
        public virtual ICollection<Task> Tasks { get; set; }
    }
}
