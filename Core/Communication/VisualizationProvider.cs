using System.Text.Json;
using Fleck;
using Led3D_2.Core.Driver;
using Led3D_2.Utility;
using NLog;

namespace Led3D_2.Communication;

/// <summary>
/// Hosts a websocket server to visualize the LED setup
/// </summary>
public class VisualizationProvider : ISettingsProvider
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
    
    private class MySettings : SettingsProvider
    {
        public Setting<bool> Enabled { get; } = new()
        {
            Name = "Enabled",
            FriendlyName = "Enable Visualization",
            DefaultValue = false
        };
        public Setting<string> Port { get; } = new()
        {
            Name = "Port",
            FriendlyName = "Port",
            DefaultValue = "1738"
        };
    }
    private readonly MySettings _settings = new();

    private WebSocketServer? _server;
    private List<IWebSocketConnection> _clients = [];
    
    public List<IDictionary<string, object?>> GetSettings() => _settings.GetSettings();
    public void UpdateSettings(Dictionary<string, object> newSettings) => _settings.UpdateSettings(newSettings);
    public void UpdateSetting(string key, object newValue) => _settings.UpdateSetting(key, newValue);
    
    public VisualizationProvider()
    {
        _settings.RegisterChangeHandler(_settings.Enabled, Toggle);
    }

    public void SendColors(DriverColorData colors)
    {
        if (!_settings.Enabled.Value) return;
    
        try
        {
            var message = JsonSerializer.Serialize(new
            {
                type = "colors",
                data = colors
            });
        
            BroadcastMessage(message);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to send color data");
        }
    }
    
    public void SendPositions(List<DriverPositionData> positions)
    {
        if (!_settings.Enabled.Value) return;
    
        try
        {
            var message = JsonSerializer.Serialize(new
            {
                type = "positions",
                data = positions
            });
        
            BroadcastMessage(message);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to send position data");
        }
    }

    private void Toggle(bool from, bool to)
    {
        if (to == true)
        {
            StartServer();
        }
        else
        {
            StopServer();
        }
    }
    
    private void StartServer()
    {
        try
        {
            var port = int.Parse(_settings.Port.Value);
            _server = new WebSocketServer($"ws://0.0.0.0:{port}");
        
            _server.Start(socket =>
            {
                socket.OnOpen = () =>
                {
                    Log.Info("Visualization client connected");
                    _clients.Add(socket);
                };
            
                socket.OnClose = () =>
                {
                    Log.Info("Visualization client disconnected");
                };
            
                socket.OnError = ex => Log.Error(ex, "Visualization socket error");
            });
        
            Log.Info("Visualization server started on port {port}", port);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to start visualization server");
            _settings.Enabled.Value = false;
        }
    }
    
    private void StopServer()
    {
        if (_server == null) return;
        
        foreach (var client in _clients.Where(c => c.IsAvailable))
        {
            client.Close();
        }
        
        _server.Dispose();
        _server = null;
        _clients.Clear();
        
        Log.Info("Visualization server stopped");
    }
    
    private void BroadcastMessage(string message)
    {
        foreach (var client in _clients.Where(c => c.IsAvailable))
        {
            client.Send(message);
        }
    }
}