using FirebaseAdmin.Messaging;

namespace ReqNot.WebApi.Services;

public class FcmSender : IFcmSender
{
    private readonly ILogger<FcmSender> _logger;

    public FcmSender(ILogger<FcmSender> logger)
    {
        _logger = logger;
    }

    public async Task SendDeviceOffNotificationAsync()
    {
        var message = new Message
        {
            Topic = "rekuperator",
            Notification = new Notification
            {
                Title = "Rekuperator",
                Body = "Rekuperator nie pracuje!"
            }
        };

        var result = await FirebaseMessaging.DefaultInstance.SendAsync(message);
        _logger.LogInformation("FCM notification sent, message ID: {MessageId}", result);
    }
}
