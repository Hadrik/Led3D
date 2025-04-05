using System.Numerics;
using ColorHelper;
using Led3D_2.Communication;
using Led3D_2.Utility;

namespace Led3D_2.Core.Volume;

public class Volume : ISettingsProvider, ICommandProvider, IDisposable
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
    private readonly CommandProvider _commandProvider = new();

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
        _commandProvider.RegisterCommand("Remove", Remove);
    }
    
    public void Dispose()
    {
        if (_settings.VolumeType.Value != null)
        {
            _settings.VolumeType.Value.RedrawVisualization -= Redraw;
        }
        
        if (_settings.VolumeType.Value is IDisposable disposable)
        {
            disposable.Dispose();
        }
        
        _settings.VolumeType.Value = null;
    }

    private object Remove()
    {
        Core.Instance.RemoveVolume(Id);
        return new { };
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

    public List<string> GetCommands() => _commandProvider.GetCommands();
    public object? ExecuteCommand(string command) => _commandProvider.ExecuteCommand(command);
    public List<IDictionary<string, object?>> GetSettings() => _settings.GetSettings();
    public void UpdateSettings(Dictionary<string, object> newSettings) => _settings.UpdateSettings(newSettings);
    public void UpdateSetting(string key, object newValue) => _settings.UpdateSetting(key, newValue);
}
