using System;
using System.Diagnostics;
using System.IO;
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
            _tcpClient = tcpClient; //todo: dispose
            _stream = _tcpClient.GetStream();//todo: IO
        }

        protected override async Task SendBytesAsync(byte[] bytes, int length, CancellationToken cancellationToken)
        {
            Debug.Assert(length.GetType() == typeof(int));
            try
            {
                await _stream.WriteAsync(BitConverter.GetBytes(length), cancellationToken);
                await _stream.WriteAsync(bytes, 0, length, cancellationToken); //todo: что за подсказка
            }
            catch (IOException ioException)
            {
                throw new ConnectivityException(ioException);
            }
        }

        protected override async Task<int> ReceiveBytesAsync(byte[] buffer, CancellationToken cancellationToken)
        {
            try
            {
                if (await _stream.ReadAsync(buffer, 0, IntSize, cancellationToken) != IntSize)
                    throw new ConnectivityException($"Protocol violation: not enough bytes returned. Must be {IntSize}");

                int messageLength = BitConverter.ToInt32(buffer);
                int bytesRead = await _stream.ReadAsync(buffer, 0, messageLength, cancellationToken);
                if (bytesRead != messageLength)
                    throw new ConnectivityException("Protocol violation: not enough bytes returned");
                return bytesRead;
            }
            catch (IOException ioException)
            {
                throw new ConnectivityException(ioException);//todo: copypaste
            }
        }

        void IDisposable.Dispose()
        {
            _tcpClient.Dispose();
            _stream.Dispose();
        }
    }
}