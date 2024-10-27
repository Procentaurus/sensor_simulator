using System.Net.WebSockets;

/**
 * https://copilot.microsoft.com/
 * prompt: podaj przykład handlera na nawiązywanie połączenia webSocket, 
 * aby zapisać z jakimi sesjami jest nawiązane połączenie
*/

public class WebSocketConnectionManager
{
    private readonly Dictionary<string, WebSocket> _sockets = new Dictionary<string, WebSocket>();

    public string AddSocket(WebSocket socket)
    {
        string connectionId = Guid.NewGuid().ToString();
        _sockets[connectionId] = socket;
        return connectionId;
    }

    public WebSocket GetSocketById(string id)
    {
        return _sockets.FirstOrDefault(p => p.Key == id).Value;
    }

    public IEnumerable<WebSocket> GetAllSockets()
    {
        return _sockets.Values;
    }

    public void RemoveSocket(string id)
    {
        _sockets.Remove(id);
    }
}
