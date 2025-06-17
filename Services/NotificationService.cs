using Microsoft.Toolkit.Uwp.Notifications;
using Windows.UI.Notifications;


namespace wpfChat.Services
{
    public static class NotificationService
    {
        public static void sendToast(string title,string message) {
            new ToastContentBuilder()
            .AddText(title)
            .AddText(message)
            .Show();
        }
    }
}
