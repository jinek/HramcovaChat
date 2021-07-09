using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace ChatContract
{
    public sealed class WebSocketConnection : SerializingConnection, IDisposable
    {
        private readonly WebSocket _webSocket;

        public WebSocketConnection(WebSocket webSocket)
        {
            _webSocket = webSocket;
        }

        protected override async Task SendBytesAsync(byte[] bytes, int length, CancellationToken cancellationToken)
        {
            try
            {
                await _webSocket.SendAsync(new ReadOnlyMemory<byte>(bytes, 0, length), WebSocketMessageType.Binary,
                    true,
                    cancellationToken);
            }
            catch (InvalidOperationException invalidOperationException)
            {
                throw new ConnectivityException("Can not connect. Possible reason: " + invalidOperationException.Message);
            }
            //blazor.webassembly.js:1 System.InvalidOperationException: The WebSocket is not connected.
            catch (WebSocketException webSocketException)
            {
                throw new ConnectivityException(webSocketException);
            }
        }

        protected override async Task<int> ReceiveBytesAsync(byte[] buffer, CancellationToken cancellationToken)
        {
            var memory = new Memory<byte>(buffer,0,buffer.Length);
            try
            {
                ValueWebSocketReceiveResult receiveResult = await _webSocket.ReceiveAsync(memory, cancellationToken);

                if (!receiveResult.EndOfMessage || receiveResult.MessageType == WebSocketMessageType.Close)
                    throw new ConnectivityException("Chat protocol violation");
                
                if (receiveResult.MessageType == WebSocketMessageType.Close)
                    throw new ConnectivityException("Connection is closed by peer");

                return receiveResult.Count;
            }
            catch (InvalidOperationException invalidOperationException)
            {
                throw new ConnectivityException("Can not connect. Possible reason: " + invalidOperationException.Message);
            }
            catch (WebSocketException webSocketException)
            {
                throw new ConnectivityException(webSocketException);
            }
        }

        void IDisposable.Dispose()
        {
            _webSocket.Dispose();
        }
    }
}