using System;

namespace ChatContract.Messages
{
    public static class ChatProtocol
    {
        public static readonly TimeSpan PingTimeout = TimeSpan.FromMilliseconds(30000);
        public static readonly TimeSpan SendReceiveTimeout = TimeSpan.FromMilliseconds(10000);
        
        /// <summary>
        /// Порт на котором сервер слушает tcp/ip
        /// </summary>
        public const int SocketPortNumber = 24523;
        
        /// <summary>
        /// Порт на котором слушается WebSocket (ws)
        /// </summary>
        public const int HttpPortNumber = 24524;
        public const int BufferSize = 1024;
    }
}