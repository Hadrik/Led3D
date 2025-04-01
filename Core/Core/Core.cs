using Led3D_2.Communication;
using Led3D_2.Core.Driver;
using Led3D_2.Utility;

namespace Led3D_2.Core;

public class Core : Singleton<Core>, ICommandProvider, ISettingsProvider
{
    private readonly List<Driver.Driver> _drivers = [];
    public IReadOnlyList<Driver.Driver> Drivers => _drivers;
    private readonly List<Volume.Volume> _volumes = [];
    public IReadOnlyList<Volume.Volume> Volumes => _volumes;

    private class MySettings : SettingsProvider
    {
        public Setting<VisualizationProvider> VisualizationProvider { get; } = new()
        {
            Name = "VisualizationProvider",
            Description = "SubSettings for the visualization provider",
            Value = new VisualizationProvider(),
            ReadOnly = true
        };
    }
    private readonly MySettings _settings = new();
    private readonly CommandProvider _commandProvider = new();
    
    public List<string> GetCommands() => _commandProvider.GetCommands();
    public object? ExecuteCommand(string command) => _commandProvider.ExecuteCommand(command);
    public List<IDictionary<string, object?>> GetSettings() => _settings.GetSettings();
    public void UpdateSettings(Dictionary<string, object> newSettings) => _settings.UpdateSettings(newSettings);
    public void UpdateSetting(string key, object newValue) => _settings.UpdateSetting(key, newValue);

    
    public Core()
    {
        _commandProvider.RegisterCommand("AddDriver", AddDriver);
        _commandProvider.RegisterCommand("AddVolume", AddVolume);
    }

    private object AddDriver()
    {
        var driver = new Driver.Driver();
        _drivers.Add(driver);
        return new { driverId = driver.Id };
    }

    private object AddVolume()
    {
        var volume = new Volume.Volume();
        _volumes.Add(volume);
        return new { volumeId = volume.Id };
    }

    public void VisualizationPositionChange()
    {
        _settings.VisualizationProvider.Value.SendPositions(
            _drivers.Select(d => d.GetPositionData()).ToList()
        );
    }
    
    public void VisualizationColorChange(DriverColorData data)
    {
        _settings.VisualizationProvider.Value.SendColors(data);
    }
}