using System.Numerics;
using ColorHelper;
using Led3D_2.Utility;

namespace Led3D_2.Core.Volume.VolumeTypes;

public class Gradient : IVolumeType
{
    private class MySettings : SettingsProvider
    {
        public Setting<Vector3> Position { get; } = new()
        {
            Name = "Position",
            DefaultValue = new Vector3(0, 0, 0)
        };
        
        public Setting<Vector3> Rotation { get; } = new()
        {
            Name = "Rotation",
            DefaultValue = new Vector3(0, 0, 0)
        };
        
        public Setting<Vector3> Scale { get; } = new()
        {
            Name = "Scale",
            DefaultValue = new Vector3(1, 1, 1)
        };
        
        public Setting<HSV> StartColor { get; } = new()
        {
            Name = "StartColor",
            DefaultValue = new HSV(0, 255, 255)
        };
        
        public Setting<HSV> EndColor { get; } = new()
        {
            Name = "EndColor",
            DefaultValue = new HSV(180, 255, 255)
        };
    }
    private readonly MySettings _settings = new();

    public List<IDictionary<string, object?>> GetSettings() => _settings.GetSettings();
    public void UpdateSettings(Dictionary<string, object> newSettings) => _settings.UpdateSettings(newSettings);
    public void UpdateSetting(string key, object newValue) => _settings.UpdateSetting(key, newValue);
    
    public HSV GetColorAt(Vector3 position)
    {
        var local = position.ToLocal(_settings.Position.Value, _settings.Rotation.Value);
        
        var normalized = local.NormalizeByScale(_settings.Scale.Value);

        return ColorHelpers.Lerp(_settings.StartColor.Value, _settings.EndColor.Value, normalized.X);
    }
}