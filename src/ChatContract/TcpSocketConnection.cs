using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ChatContract
{
    public sealed class TcpSocketConnection : SerializingConnection, IDisposable
    {
        private const int IntSize = sizeof(int);
        private readonly TcpClient _tcpClient;
        private readonly NetworkStream _stream;

        public TcpSocketConnection(TcpClient tcpClient)
        {
            _tcpClient = tcpClient;//todo: dispose
            _stream = _tcpClient.GetStream();
        }

        protected override async Task SendBufferStreamAsync(byte[] bytes, int length)
        {
            Debug.Assert(length.GetType() == typeof(int));
            await _stream.WriteAsync(BitConverter.GetBytes(length));
            await _stream.WriteAsync(bytes, 0, length);//todo: что за подсказка
        }

        protected override async Task<int> ReceiveBufferStreamAsync(byte[] buffer, CancellationToken cancellationToken)
        {//todo: IOException
            if (await _stream.ReadAsync(buffer, 0, IntSize, cancellationToken) != IntSize)
                throw new NotImplementedException();
            int messageLength = BitConverter.ToInt32(buffer);
            int bytesRead = await _stream.ReadAsync(buffer,0,messageLength,cancellationToken);
            if (bytesRead != messageLength)
                throw new NotImplementedException();
            return bytesRead;
        }

        void IDisposable.Dispose()
        {
            _tcpClient.Dispose();
            _stream.Dispose();
        }
    }
}