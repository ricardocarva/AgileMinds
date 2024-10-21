namespace AgileMindsUI.Client.Models
{
    public class Sprint
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int ProjectId { get; set; }

        // Navigation property to the related Project
        public virtual Project Project { get; set; }

        // tasks associated with the sprint
        public ICollection<Task> Tasks { get; set; } = new List<Task>();
    }
}
