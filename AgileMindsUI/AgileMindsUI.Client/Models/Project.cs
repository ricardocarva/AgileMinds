namespace AgileMindsUI.Client.Models
{
    public class Project
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool GameifiedApp { get; set; }
        public bool DiscordIntegration { get; set; }
        public bool CanvasIntegration { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CreatedBy { get; set; }

        // members associated with the project
        public List<ProjectMember> Members { get; set; } = new List<ProjectMember>();

        // masks associated with the project
        public List<Task> Tasks { get; set; } = new List<Task>();
    }

}
