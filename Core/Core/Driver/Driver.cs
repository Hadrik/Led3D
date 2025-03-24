using Led3D_2.Utility;

namespace Led3D_2.Core.Driver;

public class Driver : ICommandProvider
{
    private static int _idCounter = 0;
    public string Id { get; } = $"D{Interlocked.Increment(ref _idCounter):D3}";
    private readonly List<Strip.Strip> _strips = [];
    public IReadOnlyList<Strip.Strip> Strips => _strips;

    private readonly CommandProvider _commandProvider = new();
    public List<string> GetAvailableCommands() => _commandProvider.GetAvailableCommands();
    public object ExecuteCommand(string command) => _commandProvider.ExecuteCommand(command);
    
    public Driver()
    {
        _commandProvider.RegisterCommand("AddStrip", AddStrip);
    }
    
    public object AddStrip()
    {
        var strip = new Strip.Strip();
        _strips.Add(strip);
        return new { stripId = strip.Id };
    }
}