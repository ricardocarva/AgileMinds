namespace AgileMinds.Shared.Models
{
    public class AiResponse
    {
        public string Answer { get; set; } = string.Empty;
        public List<string> Tasks { get; set; } = new List<string>();
    }
    public class AiDetailedResponse
    {
        public string Answer { get; set; } = string.Empty;
        public List<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }

    public class TaskItem
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
