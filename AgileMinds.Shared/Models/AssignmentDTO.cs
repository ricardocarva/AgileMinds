namespace AgileMinds.Shared.Models
{
    public class AssignmentDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? DueAt { get; set; }
        public int PointsPossible { get; set; }
    }
}

