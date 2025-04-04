using System.Numerics;
using ColorHelper;
using Led3D_2.Communication;
using Led3D_2.Utility;

namespace Led3D_2.Core.Volume;

public class Volume : ISettingsProvider
{
    private static int _idCounter = 0;
    public string Id { get; } = $"V{Interlocked.Increment(ref _idCounter):D3}";
    private class MySettings : SettingsProvider
    {
        public Setting<IVolumeType?> VolumeType { get; } = new()
        {
            Name = "VolumeType",
            Value = null
        };
    }
    private readonly MySettings _settings = new();
    
    public List<IDictionary<string, object?>> GetSettings() => _settings.GetSettings();
    public void UpdateSettings(Dictionary<string, object> newSettings) => _settings.UpdateSettings(newSettings);
    public void UpdateSetting(string key, object newValue) => _settings.UpdateSetting(key, newValue);

    public Volume()
    {
        _settings.RegisterChangeHandler(_settings.VolumeType, (f, t) =>
        {
            if (f != null)
            {
                f.RedrawVisualization -= Redraw;
            }
            
            if (t != null)
            {
                t.RedrawVisualization += Redraw;
            }
        });
    }
    
    public HSV GetColorAt(Vector3 position)
    {
        return _settings.VolumeType.Value?.GetColorAt(position) ?? new HSV(0, 0, 0);
    }

    public VolumePositionData GetVolumePositionData()
    {
        var data = _settings.VolumeType.Value?.GetVolumePositionData();
        data.Id = Id;
        return data;
    }
    
    private void Redraw()
    {
        VisualizationProvider.Instance.SendVolume(GetVolumePositionData());
    }
}