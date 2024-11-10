namespace AgileMindsUI.Client.Models
{
    public class Invitation
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public int InvitorId { get; set; }
        public string InvitorUsername { get; set; }
        public int InviteeId { get; set; }
        public string InviteeUsername { get; set; }
        public bool IsAccepted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? AcceptedAt { get; set; }
    }
}
