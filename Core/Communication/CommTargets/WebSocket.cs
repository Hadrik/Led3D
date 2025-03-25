using System.Net;
using System.Net.WebSockets;
using System.Text;
using ColorHelper;

namespace Led3D_2.Communication.CommTargets;

public class WebSocket : ICommTarget
{
    public Dictionary<string, string> GetAvailableSettings()
    {
        throw new NotImplementedException();
    }

    public Dictionary<string, object> GetSettings()
    {
        throw new NotImplementedException();
    }

    public void UpdateSettings(Dictionary<string, object> newSettings)
    {
        throw new NotImplementedException();
    }

    public void UpdateSetting(string key, object newValue)
    {
        throw new NotImplementedException();
    }

    private HttpListener _httpListener = new();
    private List<System.Net.WebSockets.WebSocket> _clients = [];
    private CancellationTokenSource _tokenSource = new();

    public bool Connect(IPAddress address, int? port = null)
    {
        try
        {
            var prefix = $"http://{address}:{port ?? 80}";
            _httpListener.Prefixes.Add(prefix);
            _httpListener.Start();
            Task.Run(async () =>
            {
                while (!_tokenSource.Token.IsCancellationRequested)
                {
                    var context = await _httpListener.GetContextAsync();
                    if (context.Request.IsWebSocketRequest)
                    {
                        var webSocketContext = await context.AcceptWebSocketAsync(null);
                        var webSocket = webSocketContext.WebSocket;
                        _clients.Add(webSocket);
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                        context.Response.Close();
                    }
                }
            });
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public bool Send(List<List<HSV>> data)
    {
        var message = Newtonsoft.Json.JsonConvert.SerializeObject(data);
        var buffer = Encoding.UTF8.GetBytes(message);
        var segment = new ArraySegment<byte>(buffer);
        
        var tasks = _clients.Select(c => c.SendAsync(segment, System.Net.WebSockets.WebSocketMessageType.Text, true, _tokenSource.Token));
        Task.WhenAll(tasks);
        
        return true;
    }

    public void Disconnect()
    {
        _tokenSource.Cancel();
        foreach (var client in _clients)
        {
            client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server shutting down", _tokenSource.Token);
        }
        _httpListener.Stop();
    }
}