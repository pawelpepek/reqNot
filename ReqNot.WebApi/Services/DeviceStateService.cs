using ReqNot.WebApi.Models;

namespace ReqNot.WebApi.Services;

public class DeviceStateService
{
    private readonly object _lock = new();
    private DeviceStatus _status = DeviceStatus.Unknown;
    private DateTime _lastChecked = DateTime.MinValue;

    public void Update(DeviceStatus status)
    {
        lock (_lock)
        {
            _status = status;
            _lastChecked = DateTime.UtcNow;
        }
    }

    public (DeviceStatus Status, DateTime LastChecked) GetState()
    {
        lock (_lock) return (_status, _lastChecked);
    }
}
