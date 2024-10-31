namespace AgileMindsWebAPI.DTO
{
    public class ProjectDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool GameifiedApp { get; set; }
        public bool DiscordIntegration { get; set; }
        public bool CanvasIntegration { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CreatedBy { get; set; }

        // Only send basic details for members and tasks to avoid cyclic references
        public List<MemberDto> Members { get; set; } = new List<MemberDto>();
        public List<TaskDto> Tasks { get; set; } = new List<TaskDto>();
    }

    public class MemberDto
    {
        public int UserId { get; set; }
        public string Username { get; set; }
    }

    public class TaskDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? DueDate { get; set; }
        public int? AssignedTo { get; set; }
        public string Status { get; set; }
        public int? SprintId { get; set; }
    }

    public class SprintDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsStarted { get; set; }
        public bool IsCompleted { get; set; }
        public int ProjectId { get; set; }
        public ICollection<TaskDto> Tasks { get; set; }
    }
}
