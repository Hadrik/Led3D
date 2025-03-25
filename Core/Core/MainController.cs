using Led3D_2.Utility;

namespace Led3D_2.Core;

public class MainController : Singleton<MainController>, ICommandProvider
{
    private readonly List<Driver.Driver> _drivers = [];
    public IReadOnlyList<Driver.Driver> Drivers => _drivers;
    private readonly List<Volume.Volume> _volumes = [];
    public IReadOnlyList<Volume.Volume> Volumes => _volumes;

    private readonly CommandProvider _commandProvider = new();
    public List<string> GetAvailableCommands() => _commandProvider.GetAvailableCommands();
    public object? ExecuteCommand(string command) => _commandProvider.ExecuteCommand(command);
    
    public MainController()
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
}