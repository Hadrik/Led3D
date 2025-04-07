using System.Numerics;
using ColorHelper;
using Led3D_2.Utility;
using Led3D_2.Utility.Converters;

namespace Led3D_2.Core.Volume.VolumeTypes;

public class Gradient : IVolumeType
{
    private class MySettings : SettingsProvider
    {
        public Setting<Vector3> Position { get; } = new()
        {
            Name = "Position",
            Value = new Vector3(0, 0, 0)
        };
        
        public Setting<Vector3> Rotation { get; } = new()
        {
            Name = "Rotation",
            Value = new Vector3(0, 0, 0)
        };
        
        public Setting<Vector3> Scale { get; } = new()
        {
            Name = "Scale",
            Value = new Vector3(2, 2, 2)
        };
        
        public Setting<HSV> StartColor { get; } = new()
        {
            Name = "StartColor",
            Value = new HSV(0, 255, 255)
        };
        
        public Setting<HSV> EndColor { get; } = new()
        {
            Name = "EndColor",
            Value = new HSV(180, 255, 255)
        };
    }

    private readonly MySettings _settings = new()
    {
        Converters = [ new HsvSettingsConverter(), new Vector3SettingsConverter() ]
    };

    public event Action? RedrawVisualization;

    public Gradient()
    {
        _settings.RegisterChangeHandler(_settings.Position, (_, _) => Redraw());
        _settings.RegisterChangeHandler(_settings.Rotation, (_, _) => Redraw());
        _settings.RegisterChangeHandler(_settings.Scale, (_, _) => Redraw());
    }
    
    public HSV GetColorAt(Vector3 position)
    {
        var local = position.ToLocal(_settings.Position.Value, _settings.Rotation.Value);
        
        var normalized = local.NormalizeByScale(_settings.Scale.Value);

        return ColorHelpers.Lerp(_settings.StartColor.Value, _settings.EndColor.Value, normalized.X);
    }

    public VolumePositionData GetVolumePositionData()
    {
        return new VolumePositionData
            { Position = _settings.Position.Value, Rotation = _settings.Rotation.Value, Scale = _settings.Scale.Value };
    }

    private void Redraw()
    {
        RedrawVisualization?.Invoke();
    }
    
    public List<IDictionary<string, object?>> GetSettings() => _settings.GetSettings();
    public void UpdateSettings(Dictionary<string, object> newSettings) => _settings.UpdateSettings(newSettings);
    public void UpdateSetting(string key, object newValue) => _settings.UpdateSetting(key, newValue);
}