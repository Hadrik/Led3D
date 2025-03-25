using System.Numerics;
using ColorHelper;
using Led3D_2.Utility;

namespace Led3D_2.Core.Volume.VolumeTypes;

public class Gradient : IVolumeType
{
    private class MySettings
    {
        public Vector3 Position { get; set; } = new(0, 0, 0);
        public Vector3 Rotation { get; set; } = new(0, 0, 0);
        public Vector3 Scale { get; set; } = new(1, 1, 1);
        public HSV StartColor { get; set; } = new(0, 255, 255);
        public HSV EndColor { get; set; } = new(180, 255, 255);
    }
    private readonly MySettings _settings = new();
    private readonly SettingsProvider<MySettings> _settingsProvider;

    public Gradient()
    {
        _settingsProvider = new SettingsProvider<MySettings>(_settings);
    }

    public Dictionary<string, string> GetAvailableSettings() => _settingsProvider.GetAvailableSettings();
    public Dictionary<string, object> GetSettings() => _settingsProvider.GetSettings();
    public void UpdateSettings(Dictionary<string, object> newSettings) => _settingsProvider.UpdateSettings(newSettings);
    public void UpdateSetting(string key, object newValue) => _settingsProvider.UpdateSetting(key, newValue);
    
    public HSV GetColorAt(Vector3 position)
    {
        var local = position.ToLocal(_settings.Position, _settings.Rotation);
        
        var normalized = local.NormalizeByScale(_settings.Scale);

        return ColorHelpers.Lerp(_settings.StartColor, _settings.EndColor, normalized.X);
    }
}