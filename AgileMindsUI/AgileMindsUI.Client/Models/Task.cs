namespace AgileMindsUI.Client.Models
{
    public class Task
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int ProjectId { get; set; }
        public int? AssignedTo { get; set; }
        public int? SprintId { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DueDate { get; set; }
        public TaskStatus Status { get; set; } = TaskStatus.Pending;

        public int Priority { get; set; } = 0;
        public string Type { get; set; } = "Feature";
        public int? Estimate { get; set; }

        public List<string>? Tags { get; set; }
    }

    public enum TaskStatus
    {
        Pending = 0,
        InProgress = 1,
        Completed = 2
    }
}
