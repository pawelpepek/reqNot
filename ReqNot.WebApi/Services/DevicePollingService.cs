using ReqNot.WebApi.Models;

namespace ReqNot.WebApi.Services;

public class DevicePollingService : BackgroundService
{
    private readonly DeviceChecker _deviceChecker;
    private readonly DeviceStateService _deviceStateService;
    private readonly IFcmSender _fcmSender;
    private readonly FirestoreService _firestoreService;
    private readonly TimeSpan _interval;
    private readonly ILogger<DevicePollingService> _logger;

    public DevicePollingService(
        DeviceChecker deviceChecker,
        DeviceStateService deviceStateService,
        IFcmSender fcmSender,
        FirestoreService firestoreService,
        IConfiguration configuration,
        ILogger<DevicePollingService> logger)
    {
        _deviceChecker = deviceChecker;
        _deviceStateService = deviceStateService;
        _fcmSender = fcmSender;
        _firestoreService = firestoreService;
        _logger = logger;

        var minutes = configuration.GetValue<int>("PollingIntervalMinutes", 30);
        _interval = TimeSpan.FromMinutes(minutes);
        _logger.LogInformation("Polling interval set to {Interval}", _interval);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var listener = _firestoreService.ListenForCheck(async () =>
        {
            try
            {
                _logger.LogInformation("Check requested via Firestore.");
                var isOn = await _deviceChecker.CheckAsync();
                var status = isOn ? DeviceStatus.On : DeviceStatus.Off;
                _deviceStateService.Update(status);
                await _firestoreService.UpdateWorksAsync(status);
                await _firestoreService.ResetCheckAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling Firestore check request.");
                _deviceStateService.Update(DeviceStatus.Unknown);
                try { await _firestoreService.UpdateWorksAsync(DeviceStatus.Unknown); } catch { }
                try { await _firestoreService.ResetCheckAsync(); } catch { }
            }
        });

        try
        {
            do
            {
                await PollAndNotifyAsync();
                await Task.Delay(_interval, stoppingToken);
            }
            while (!stoppingToken.IsCancellationRequested);
        }
        finally
        {
            await listener.StopAsync();
        }
    }

    private async Task PollAndNotifyAsync()
    {
        try
        {
            var isOn = await _deviceChecker.CheckAsync();
            var status = isOn ? DeviceStatus.On : DeviceStatus.Off;
            _deviceStateService.Update(status);
            await _firestoreService.UpdateWorksAsync(status);

            if (status == DeviceStatus.Off)
            {
                _logger.LogWarning("Device is off, sending push notification.");
                await _fcmSender.SendDeviceOffNotificationAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error polling device.");
            _deviceStateService.Update(DeviceStatus.Unknown);
            try { await _firestoreService.UpdateWorksAsync(DeviceStatus.Unknown); } catch { }
        }
    }
}
