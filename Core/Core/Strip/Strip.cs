using System.Numerics;
using ColorHelper;
using Led3D_2.Utility;

namespace Led3D_2.Core.Strip;

/// <summary>
/// Data structure that gets passed to a CommunicationTarget.
/// </summary>
public class StripColorData
{
    public required string Id { get; set; }
    public required List<HSV> Data { get; set; }
}

/// <summary>
/// Used only to set up visualizations.
/// </summary>
public class StripPositionData
{
    public required string Id { get; set; }
    public required List<Vector3> Data { get; set; }
}

public class Strip : ISettingsProvider, ICommandProvider, IDisposable
{
    private static int _idCounter = 0;
    public string Id { get; } = $"S{Interlocked.Increment(ref _idCounter):D3}";
    private readonly List<Pixel.Pixel> _pixels = [];
    public IReadOnlyList<Pixel.Pixel> Pixels => _pixels;

    private class MySettings : SettingsProvider
    {
        public Setting<IStripPixelLayout?> Layout { get; } = new()
        {
            Name = "Layout",
            Value = null
        };

        public Setting<Volume.Volume?> Volume { get; } = new()
        {
            Name = "Volume",
            Value = null,
            Converter = new VolumeConverter()
        };
    }

    private readonly MySettings _settings = new();
    private readonly CommandProvider _commandProvider = new();

    public Strip()
    {
        _settings.RegisterChangeHandler(_settings.Layout, OnLayoutChange);
        _commandProvider.RegisterCommand("Remove", Remove);
    }
    
    public void Dispose()
    {
        if (_settings.Layout.Value != null)
        {
            _settings.Layout.Value.PixelPositionsChanged -= OnPixelPositionsChanged;
        }
        
        if (_settings.Layout.Value is IDisposable disposable)
        {
            disposable.Dispose();
        }
        
        _settings.Layout.Value = null;
    }
    
    private object Remove()
    {
        foreach (var driver in Core.Instance.Drivers)
        {
            if (driver.Strips.All(s => s.Id != Id)) continue;
            driver.RemoveStrip(Id);
            break;
        }
        return new { };
    }

    public StripColorData? GetColors()
    {
        if (_settings.Volume.Value == null) return null;
        return new StripColorData()
        {
            Id = Id,
            Data = _pixels.Select(p => _settings.Volume.Value.GetColorAt(p.Position)).ToList()
        };
    }
    
    public StripPositionData? GetPositions()
    {
        if (_settings.Layout.Value == null) return null;
        return new StripPositionData()
        {
            Id = Id,
            Data = _pixels.Select(p => p.Position).ToList()
        };
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
        
        Core.Instance.SendAllVisualizationData();
    }

    private void ChangePixelCount(int difference)
    {
        if (difference > 0)
        {
            // Remove diff pixels
            _pixels.RemoveRange(_pixels.Count - difference, difference);
        }
        else
        {
            // Add diff pixels
            for (var i = 0; i < -difference; i++)
            {
                _pixels.Add(new Pixel.Pixel());
            }
        }
    }

    public List<string> GetCommands() => _commandProvider.GetCommands();
    public object? ExecuteCommand(string command) => _commandProvider.ExecuteCommand(command);
    public List<IDictionary<string, object?>> GetSettings() => _settings.GetSettings();
    public void UpdateSettings(Dictionary<string, object> newSettings) => _settings.UpdateSettings(newSettings);
    public void UpdateSetting(string key, object newValue) => _settings.UpdateSetting(key, newValue);

    private class VolumeConverter : ISettingsConverter<Volume.Volume?>
    {
        public object? ToObject(Volume.Volume? vol)
        {
            return vol?.Id;
        }

        public Volume.Volume? FromObject(object obj)
        {
            return obj is not string id ? null : Core.Instance.Volumes.FirstOrDefault(v => v.Id == id);
        }
    }
}