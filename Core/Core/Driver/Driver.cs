using System.Net;
using System.Timers;
using Led3D_2.Communication;
using Led3D_2.Core.Strip;
using Led3D_2.Utility;
using Timer = System.Timers.Timer;

namespace Led3D_2.Core.Driver;

public class DriverColorData
{
    public required string Id { get; set; }
    public required List<StripColorData> Data { get; set; }
}

public class DriverPositionData
{
    public required string Id { get; set; }
    public required List<StripPositionData> Data { get; set; }
}

public class Driver : ICommandProvider, ISettingsProvider, IDisposable
{
    private static int _idCounter = 0;
    public string Id { get; } = $"D{Interlocked.Increment(ref _idCounter):D3}";
    private readonly List<Strip.Strip> _strips = [];
    public IReadOnlyList<Strip.Strip> Strips => _strips;

    private class MySettings : SettingsProvider
    {
        public Setting<int> FrameRate { get; } = new()
        {
            Name = "FrameRate",
            Value = 30
        };
        
        public Setting<ICommTarget?> Target { get; } = new()
        {
            Name = "Target",
            Value = null
        };
    }
    private readonly MySettings _settings = new();
    private readonly CommandProvider _commandProvider = new();
    
    public List<string> GetCommands() => _commandProvider.GetCommands();
    public object? ExecuteCommand(string command) => _commandProvider.ExecuteCommand(command);
    public List<IDictionary<string, object?>> GetSettings() => _settings.GetSettings();
    public void UpdateSettings(Dictionary<string, object> newSettings) => _settings.UpdateSettings(newSettings);
    public void UpdateSetting(string key, object newValue) => _settings.UpdateSetting(key, newValue);

    private readonly Timer _timer;
    
    public Driver()
    {
        _settings.RegisterChangeHandler(_settings.FrameRate, FramerateChange);
        _settings.RegisterChangeHandler(_settings.Target, TargetChange);
        _commandProvider.RegisterCommand("AddStrip", AddStrip);
        _commandProvider.RegisterCommand("Remove", Remove);
        _timer = new Timer((double)1 / _settings.FrameRate.Value);
        _timer.AutoReset = true;
        _timer.Elapsed += Send;
        _timer.Start();
    }

    public void Dispose()
    {
        _timer.Stop();
        _timer.Elapsed -= Send;
        _timer.Dispose();
        
        _settings.Target.Value?.Disconnect();
        
        foreach (var strip in _strips)
        {
            strip.Dispose();
        }
        _strips.Clear();
    }

    private object Remove()
    {
        Core.Instance.RemoveDriver(Id);
        return new { };
    }

    public DriverPositionData GetPositionData()
    {
        return new DriverPositionData
        {
            Id = Id,
            Data = _strips.Select(s => s.GetPositions()).OfType<StripPositionData>().ToList()
        };
    }
    
    public DriverColorData GetColorData()
    {
        return new DriverColorData
        {
            Id = Id,
            Data = _strips.Select(s => s.GetColors()).OfType<StripColorData>().ToList()
        };
    }
    
    private object AddStrip()
    {
        var strip = new Strip.Strip();
        _strips.Add(strip);
        return new { stripId = strip.Id };
    }
    
    public void RemoveStrip(string stripId)
    {
        var strip = _strips.FirstOrDefault(s => s.Id == stripId);
        if (strip == null) return;
        strip.Dispose();
        _strips.Remove(strip);
    }

    private void FramerateChange(int from, int to)
    {
        _timer.Stop();
        _timer.Interval = (double)1000 / to;
        _timer.Start();
    }

    private void TargetChange(ICommTarget? from, ICommTarget? to)
    {
        if (from != null)
        {
            _timer.Stop();
            from.Disconnect();
        }

        if (to != null)
        {
            to.Connect(new IPAddress([127, 0, 0, 1]), 5432);
            _timer.Start();
        }
    }

    private void Send(object? o, ElapsedEventArgs e)
    {
        var data = GetColorData();
        
        _settings.Target.Value?.Send(data);
        VisualizationProvider.Instance.SendColors(data);
    }
}