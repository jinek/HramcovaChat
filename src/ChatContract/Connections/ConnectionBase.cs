using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Threading.Tasks;
using ChatContract.Messages;
using ChatHelpers;

namespace ChatContract.Connections
{
    public abstract class ConnectionBase : IConnection
    {
        private readonly AsyncLock _readLock = new();
        private readonly AsyncLock _writeLock = new();
        private readonly byte[] _sendBuffer = new byte[ChatProtocol.BufferSize];
        private readonly byte[] _receiveBuffer = new byte[ChatProtocol.BufferSize];

        public async Task<T> ReceiveMessageAsync<T>(TimeSpan? receiveTimeout, CancellationToken cancellationToken)
        {
            var dataContractJsonSerializer = new DataContractJsonSerializer(typeof(T));

            using CancellationTokenSource? cancellationTokenSource =
                receiveTimeout != null ? new CancellationTokenSource((TimeSpan) receiveTimeout) : null;

            using CancellationTokenSource cancellationTokenSourceFinal =
                CancellationTokenSource.CreateLinkedTokenSource(cancellationToken,
                    cancellationTokenSource?.Token ?? CancellationToken.None);

            int length;
            // ReSharper disable once MethodSupportsCancellation this timeout is only ping-pong speed
            using (await _readLock.LockAsync())
            {
                try
                {
                    length = await ReceiveBytesAsync(_receiveBuffer, cancellationTokenSourceFinal.Token);
                }
                catch (OperationCanceledException)
                {
                    throw new ConnectivityException($"Peer has not answered within {receiveTimeout}");
                }
            }

            await using var memoryStream = new MemoryStream(_receiveBuffer, 0, length);
            return (T) dataContractJsonSerializer.ReadObject(memoryStream)!;
        }

        public async Task SendMessageAsync<T>(T message, CancellationToken cancellationToken)
        {
            var dataContractJsonSerializer = new DataContractJsonSerializer(typeof(T));

            var memoryStream = new MemoryStream(_sendBuffer);
            dataContractJsonSerializer.WriteObject(memoryStream, message);
            int length = (int) memoryStream.Position;

            if (length >= ChatProtocol.BufferSize)
                throw new ConnectivityException("For simplicity chat supports only small messages. Aborting.");
            
            using (await _writeLock.LockAsync(cancellationToken))
            {
                await SendBytesAsync(_sendBuffer, length,
                    cancellationToken);
            }
            //todo: all assemblies must be checked math operations
        }

        protected abstract Task SendBytesAsync(byte[] bytes, int length, CancellationToken cancellationToken);

        protected abstract Task<int> ReceiveBytesAsync(byte[] buffer, CancellationToken cancellationToken);
    }
}