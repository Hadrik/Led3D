using System.Numerics;
using Led3D_2.Utility;
using NLog;

namespace Led3D_2.Core.Strip.PixelLayouts;

public class Linear : IStripPixelLayout
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
    private class MySettings : SettingsProvider
    {
        public Setting<int> Length { get; } = new()
        {
            Name = "Length",
            DefaultValue = 0
        };
        
        public Setting<Vector3> StartPosition { get; } = new()
        {
            Name = "StartPosition",
            DefaultValue = new Vector3(1, 1, 1)
        };
        
        public Setting<Vector3> EndPosition { get; } = new()
        {
            Name = "EndPosition",
            DefaultValue = new Vector3(-1, -1, -1)
        };
    }
    private readonly MySettings _settings = new();

    public event Action<List<Vector3>>? PixelPositionsChanged;
    
    public Linear()
    {
        _settings.RegisterChangeHandler(_settings.Length, (o, n) => UpdatePixelPositions());
        _settings.RegisterChangeHandler(_settings.StartPosition, (o, n) => UpdatePixelPositions());
        _settings.RegisterChangeHandler(_settings.EndPosition, (o, n) => UpdatePixelPositions());
    }

    public List<IDictionary<string, object?>> GetSettings() => _settings.GetSettings();
    public void UpdateSettings(Dictionary<string, object> newSettings) => _settings.UpdateSettings(newSettings);
    public void UpdateSetting(string key, object newValue) => _settings.UpdateSetting(key, newValue);
    
    private void UpdatePixelPositions()
    {
        var positions = new List<Vector3>(_settings.Length.Value);
        
        var start = _settings.StartPosition.Value;
        var end = _settings.EndPosition.Value;

        for (var i = 0; i < positions.Count; i++)
        {
            positions.Add(Vector3.Lerp(start, end, (float)i / _settings.Length.Value));
        }
        
        PixelPositionsChanged?.Invoke(positions);
    }
}