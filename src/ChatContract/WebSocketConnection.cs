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

        protected override async Task SendBufferStreamAsync(byte[] bytes, int length)
        {
            await _webSocket.SendAsync(new ReadOnlyMemory<byte>(bytes, 0, length), WebSocketMessageType.Binary, true,
                CancellationToken.None);
        }

        protected override async Task<int> ReceiveBufferStreamAsync(byte[] buffer, CancellationToken cancellationToken)
        {
            var memory = new Memory<byte>(buffer,0,buffer.Length);
            ValueWebSocketReceiveResult receiveResult = await _webSocket.ReceiveAsync(memory,cancellationToken);
            if (!receiveResult.EndOfMessage)
                throw new NotImplementedException();
            if (receiveResult.MessageType != WebSocketMessageType.Binary)
                //todo: может быть Close
                throw new NotImplementedException();
            
            return receiveResult.Count;
        }

        void IDisposable.Dispose()
        {
            _webSocket.Dispose();
        }
    }
}