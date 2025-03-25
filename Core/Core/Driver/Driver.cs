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

    private class MySettings
    {
        public int FrameRate { get; set; } = 30;
        public ICommTarget? Target { get; set; } = null;
    }
    private readonly MySettings _settings = new();
    private readonly SettingsProvider<MySettings> _settingsProvider;
    private readonly CommandProvider _commandProvider = new();
    
    public List<string> GetAvailableCommands() => _commandProvider.GetAvailableCommands();
    public object? ExecuteCommand(string command) => _commandProvider.ExecuteCommand(command);
    public Dictionary<string, string> GetAvailableSettings() => _settingsProvider.GetAvailableSettings();
    public Dictionary<string, object> GetSettings() => _settingsProvider.GetSettings();
    public void UpdateSettings(Dictionary<string, object> newSettings) => _settingsProvider.UpdateSettings(newSettings);
    public void UpdateSetting(string key, object newValue) => _settingsProvider.UpdateSetting(key, newValue);

    private readonly Timer _timer;
    
    public Driver()
    {
        _settingsProvider = new SettingsProvider<MySettings>(_settings);
        _settingsProvider.RegisterChangeHandler(s => s.FrameRate, FramerateChange);
        _settingsProvider.RegisterChangeHandler(s => s.Target, TargetChange);
        _commandProvider.RegisterCommand("AddStrip", AddStrip);
        _timer = new Timer((double)1 / _settings.FrameRate);
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
        _settings.Target?.Send(GetFrame());
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