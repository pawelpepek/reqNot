namespace ReqNot.WebApi.Services;

public class DeviceStateService
{
    private readonly object _lock = new();
    private bool _isOn;
    private DateTime _lastChecked = DateTime.MinValue;

    public void Update(bool isOn)
    {
        lock (_lock)
        {
            _isOn = isOn;
            _lastChecked = DateTime.UtcNow;
        }
    }

    public (bool IsOn, DateTime LastChecked) GetState()
    {
        lock (_lock) return (_isOn, _lastChecked);
    }
}
