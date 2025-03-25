using System.Net;
using System.Timers;
using ColorHelper;
using Led3D_2.Communication;
using Led3D_2.Utility;
using Timer = System.Timers.Timer;

namespace Led3D_2.Core.Driver;

public class Driver : ICommandProvider, ISettingsProvider
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
            DefaultValue = 30
        };
        
        public Setting<ICommTarget?> Target { get; } = new()
        {
            Name = "Target",
            DefaultValue = null
        };
    }
    private readonly MySettings _settings = new();
    private readonly CommandProvider _commandProvider = new();
    
    public List<string> GetAvailableCommands() => _commandProvider.GetAvailableCommands();
    public object? ExecuteCommand(string command) => _commandProvider.ExecuteCommand(command);
    public List<IDictionary<string, object?>> GetAvailableSettings() => _settings.GetAvailableSettings();
    public Dictionary<string, object> GetSettingValues() => _settings.GetSettingValues();
    public void UpdateSettings(Dictionary<string, object> newSettings) => _settings.UpdateSettings(newSettings);
    public void UpdateSetting(string key, object newValue) => _settings.UpdateSetting(key, newValue);

    private readonly Timer _timer;
    
    public Driver()
    {
        _settings.RegisterChangeHandler(_settings.FrameRate, FramerateChange);
        _settings.RegisterChangeHandler(_settings.Target, TargetChange);
        _commandProvider.RegisterCommand("AddStrip", AddStrip);
        _timer = new Timer((double)1 / _settings.FrameRate.Value);
        _timer.AutoReset = true;
        _timer.Elapsed += Send;
    }
    
    private object AddStrip()
    {
        var strip = new Strip.Strip();
        _strips.Add(strip);
        return new { stripId = strip.Id };
    }

    private void FramerateChange(int from, int to)
    {
        _timer.Stop();
        _timer.Interval = (double)1 / to;
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
        _settings.Target.Value?.Send(GetFrame());
    }
    
    private List<List<HSV>> GetFrame()
    {
        var frame = new List<List<HSV>>();
        foreach (var strip in _strips)
        {
            var stripFrame = strip.GetColors();
            if (stripFrame == null) continue;
            frame.Add(stripFrame);
        }
        return frame;
    }
}