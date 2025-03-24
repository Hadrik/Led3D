using System.Numerics;
using ColorHelper;
using Led3D_2.Utility;

namespace Led3D_2.Core.Volume;

public class Volume : ISettingsProvider
{
    private static int _idCounter = 0;
    public string Id { get; } = $"V{Interlocked.Increment(ref _idCounter):D3}";
    private class MySettings
    {
        public IVolumeType? VolumeType { get; set; } = null;
    }
    private readonly MySettings _settings = new();
    private readonly SettingsProvider<MySettings> _settingsProvider;
    
    public Volume()
    {
        _settingsProvider = new SettingsProvider<MySettings>(_settings);
    }
    
    public Dictionary<string, string> GetAvailableSettings() => _settingsProvider.GetAvailableSettings();
    public Dictionary<string, object> GetSettings() => _settingsProvider.GetSettings();
    public void UpdateSettings(Dictionary<string, object> newSettings) => _settingsProvider.UpdateSettings(newSettings);
    public void UpdateSetting(string key, object newValue) => _settingsProvider.UpdateSetting(key, newValue);
    
    public HSV GetColorAt(Vector3 position)
    {
        return _settings.VolumeType?.GetColorAt(position) ?? new HSV(0, 0, 0);
    }
}