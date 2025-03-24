using System.Numerics;
using Led3D_2.Utility;
using NLog;

namespace Led3D_2.Core.Strip.PixelLayouts;

public class Linear : IStripPixelLayout
{
    private class MySettings
    {
        public int Length { get; set; } = 0;
        public Vector3 StartPosition { get; set; } = new(1, 1, 1);
        public Vector3 EndPosition { get; set; } = new(-1, -1, -1);
    }
    private readonly MySettings _settings = new();
    
    public Dictionary<string, object> GetSettings() => _settings.AsDictionary();
    public Dictionary<string, string> GetAvailableSettings() => _settings.AsTypeDictionary();
    
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    public event Action<List<Vector3>>? PixelPositionsChanged;

    public void UpdateSettings(Dictionary<string, object> newSettings)
    {
        foreach (var (key, value) in newSettings)
        {
            UpdateSetting(key, value);
        }
    }
    
    public void UpdateSetting(string key, object newValue)
    {
        var property = _settings.GetProperty(key);
        if (!Verifier.VerifyProperty(property, key, newValue)) return;
        
        _settings.SetProperty(property!, newValue);
        
        // if (key is "Length" or "Start Position" or "End Position")
        // {
            UpdatePixelPositions();
        // }
    }

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