using NLog;

namespace Led3D_2.Utility;

/// <summary>
/// Any class that provides commands accessible from the API has to implement this interface
/// </summary>
public interface ICommandProvider
{
    /// <summary>
    /// List all available commands
    /// </summary>
    /// <returns>
    /// List of command names
    /// </returns>
    List<string> GetCommands();
    
    /// <summary>
    /// Execute a command
    /// </summary>
    /// <param name="command">
    /// Command name
    /// </param>
    /// <returns>
    /// Object of any type returned by the command
    /// </returns>
    /// <exception cref="ArgumentException">
    /// command not found
    /// </exception>
    object? ExecuteCommand(string command);
}

/// <summary>
/// Helper class to manage commands
/// </summary>
public class CommandProvider : ICommandProvider
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
    
    private readonly Dictionary<string, Delegate> _commands = new();
    
    /// <summary>
    /// Register a new command
    /// </summary>
    /// <param name="command">
    /// Command name
    /// </param>
    /// <param name="action">
    /// Delegate to execute when the command is called
    /// </param>
    public void RegisterCommand(string command, Delegate action)
    {
        _commands[command] = action;
    }
    
    public List<string> GetCommands()
    {
        return _commands.Keys.ToList();
    }

    public object? ExecuteCommand(string command)
    {
        if (_commands.TryGetValue(command, out var value)) return value.DynamicInvoke();
        
        Log.Warn("Command '{command}' not found", command);
        throw new ArgumentException($"Command '{command}' not found");
    }
}