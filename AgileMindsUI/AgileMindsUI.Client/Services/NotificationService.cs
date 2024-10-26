namespace AgileMindsUI.Client.Services
{
    public class NotificationService
    {
        public event Action OnNotificationUpdated;

        public void NotifyNotificationsChanged()
        {
            OnNotificationUpdated?.Invoke();
        }
    }
}
