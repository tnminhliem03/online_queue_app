using System.Net.WebSockets;
using System.Text;

namespace OnlineQueueAPI.Services
{
    public class WebSocketService
    {
        private readonly List<WebSocket> _clients = new List<WebSocket>();
        private readonly ILogger<WebSocketService> _logger;

        public WebSocketService(ILogger<WebSocketService> logger)
        {
            _logger = logger;
        }

        public void AddClient(WebSocket client)
        {
            _clients.Add(client);
            _logger.LogInformation("New client added to WebSocket service.");
        }

        public void RemoveClient(WebSocket client)
        {
            _clients.Remove(client);
            _logger.LogInformation("Client removed from WebSocket service.");
        }

        public async Task SendUpdateToClients(string type)
        {
            var buffer = Encoding.UTF8.GetBytes(type);
            var segment = new ArraySegment<byte>(buffer);

            var tasks = _clients.Select(client =>
                client.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None)
            );

            await Task.WhenAll(tasks);
        }


        public async Task HandleWebSocketConnection(WebSocket webSocket)
        {
            AddClient(webSocket);
            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    var buffer = new byte[1024 * 4];
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        _logger.LogInformation($"Received message: {message}");
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        _logger.LogInformation("Client requested to close the connection.");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while handling WebSocket connection.");
            }
            finally
            {
                RemoveClient(webSocket);
                try
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                    _logger.LogInformation("WebSocket connection closed.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while closing WebSocket connection.");
                }
            }
        }
    }
}
