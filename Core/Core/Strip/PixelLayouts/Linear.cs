using System.Numerics;
using Led3D_2.Utility;
using NLog;

namespace Led3D_2.Core.Strip.PixelLayouts;

public class Linear : IStripPixelLayout
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
    private class MySettings
    {
        public int Length { get; set; } = 0;
        public Vector3 StartPosition { get; set; } = new(1, 1, 1);
        public Vector3 EndPosition { get; set; } = new(-1, -1, -1);
    }
    private readonly MySettings _settings = new();
    private readonly SettingsProvider<MySettings> _settingsProvider;

    public event Action<List<Vector3>>? PixelPositionsChanged;
    
    public Linear()
    {
        _settingsProvider = new SettingsProvider<MySettings>(_settings);
        _settingsProvider.RegisterChangeHandler(s => s.Length, (o, n) => UpdatePixelPositions());
        _settingsProvider.RegisterChangeHandler(s => s.StartPosition, (o, n) => UpdatePixelPositions());
        _settingsProvider.RegisterChangeHandler(s => s.EndPosition, (o, n) => UpdatePixelPositions());
    }

    public Dictionary<string, object> GetSettings() => _settingsProvider.GetSettings();
    public Dictionary<string, string> GetAvailableSettings() => _settingsProvider.GetAvailableSettings();
    public void UpdateSettings(Dictionary<string, object> newSettings) => _settingsProvider.UpdateSettings(newSettings);
    public void UpdateSetting(string key, object newValue) => _settingsProvider.UpdateSetting(key, newValue);
    
    private void UpdatePixelPositions()
    {
        var positions = new List<Vector3>(_settings.Length);
        
        var start = _settings.StartPosition;
        var end = _settings.EndPosition;

        for (var i = 0; i < positions.Count; i++)
        {
            positions.Add(Vector3.Lerp(start, end, (float)i / _settings.Length));
        }
        
        PixelPositionsChanged?.Invoke(positions);
    }
}