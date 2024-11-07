namespace AgileMindsUI.Client.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }

        // not all notifications will be invites
        public int? InvitationId { get; set; }
        public Invitation Invitation { get; set; }
    }
}
