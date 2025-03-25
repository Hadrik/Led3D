using System.Numerics;
using ColorHelper;
using Led3D_2.Core.Volume;
using Led3D_2.Utility;

namespace Led3D_2.Core.Strip;

public class Strip : ISettingsProvider
{
    private static int _idCounter = 0;
    public string Id { get; } = $"S{Interlocked.Increment(ref _idCounter):D3}";
    private readonly List<Pixel.Pixel> _pixels = [];
    public IReadOnlyList<Pixel.Pixel> Pixels => _pixels;

    private class MySettings
    {
        public IStripPixelLayout? Layout { get; set; } = null;
        public IVolumeType? Volume { get; set; } = null;
    }
    private readonly MySettings _settings = new();
    private readonly SettingsProvider<MySettings> _settingsProvider;
    
    public Strip()
    {
        _settingsProvider = new SettingsProvider<MySettings>(_settings);
        _settingsProvider.RegisterChangeHandler(s => s.Layout, OnLayoutChange);
    }

    public Dictionary<string, string> GetAvailableSettings() => _settingsProvider.GetAvailableSettings();
    public Dictionary<string, object> GetSettings() => _settingsProvider.GetSettings();
    public void UpdateSettings(Dictionary<string, object> newSettings) => _settingsProvider.UpdateSettings(newSettings);
    public void UpdateSetting(string key, object newValue) => _settingsProvider.UpdateSetting(key, newValue);
    
    public List<HSV>? GetColors()
    {
        if (_settings.Volume == null) return null;
        return _pixels.Select(p => _settings.Volume.GetColorAt(p.Position)).ToList();
    }

    private void OnLayoutChange(IStripPixelLayout? oldLayout, IStripPixelLayout? newLayout)
    {
        if (oldLayout != null) oldLayout.PixelPositionsChanged -= OnPixelPositionsChanged;
        if (newLayout != null) newLayout.PixelPositionsChanged += OnPixelPositionsChanged;
    }
    
    private void OnPixelPositionsChanged(List<Vector3> positions)
    {
        var diff = _pixels.Count - positions.Count;
        if (diff != 0)
        {
            ChangePixelCount(diff);
        }
        
        for (var i = 0; i < positions.Count; i++)
        {
            _pixels[i].Position = positions[i];
        }
    }

    private void ChangePixelCount(int difference)
    {
        if (difference < 0)
        {
            // Remove diff pixels
            _pixels.RemoveRange(_pixels.Count + difference, -difference);
        }
        else
        {
            // Add diff pixels
            for (var i = 0; i < difference; i++)
            {
                _pixels.Add(new Pixel.Pixel());
            }
        }
    }
}