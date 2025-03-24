using Led3D_2.Utility;

namespace Led3D_2.Core;

public class MainController : Singleton<MainController>, ICommandProvider
{
    private readonly List<Driver.Driver> _drivers = [];
    public IReadOnlyList<Driver.Driver> Drivers => _drivers;

    private readonly CommandProvider _commandProvider = new();
    public List<string> GetAvailableCommands() => _commandProvider.GetAvailableCommands();
    public object ExecuteCommand(string command) => _commandProvider.ExecuteCommand(command);
    
    public MainController()
    {
        _commandProvider.RegisterCommand("AddDriver", AddDriver);
    }
    
    public object AddDriver()
    {
        var driver = new Driver.Driver();
        _drivers.Add(driver);
        return new { driverId = driver.Id };
    }
}