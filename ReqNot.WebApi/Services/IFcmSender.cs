namespace ReqNot.WebApi.Services;

public interface IFcmSender
{
    Task SendDeviceOffNotificationAsync();
}
