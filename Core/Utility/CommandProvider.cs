using NLog;

namespace Led3D_2.Utility;

public interface ICommandProvider
{
    List<string> GetAvailableCommands();
    object ExecuteCommand(string command);
}

public class CommandProvider : ICommandProvider
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
    
    private readonly Dictionary<string, Delegate> _commands = new();
    
    public void RegisterCommand(string command, Delegate action)
    {
        _commands[command] = action;
    }
    
    public List<string> GetAvailableCommands()
    {
        return _commands.Keys.ToList();
    }

    public object ExecuteCommand(string command)
    {
        if (!_commands.TryGetValue(command, out var value))
        {
            Log.Warn("Command '{command}' not found", command);
            return null;
        }

        return value.DynamicInvoke();
    }
}