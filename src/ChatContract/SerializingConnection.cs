using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Threading.Tasks;
using ChatHelpers;

namespace ChatContract
{
    public abstract class SerializingConnection : IConnection
    {
        private readonly AsyncLock _readLock = new();
        private readonly AsyncLock _writeLock = new();
        private readonly byte[] _sendBuffer = new byte[ChatProtocol.BufferSize];
        private readonly byte[] _receiveBuffer = new byte[ChatProtocol.BufferSize];

        public async Task<T> ReceiveMessageAsync<T>(TimeSpan receiveTimeout)
        {
            var dataContractJsonSerializer = new DataContractJsonSerializer(typeof(T));
            using var cancellationTokenSource = new CancellationTokenSource(receiveTimeout);
            int length;
            using (await _readLock.LockAsync(cancellationTokenSource.Token))
            {
                length = await ReceiveBufferStreamAsync(_receiveBuffer, cancellationTokenSource.Token);
            }

            await using var memoryStream = new MemoryStream(_receiveBuffer, 0, length);
            return (T) dataContractJsonSerializer.ReadObject(memoryStream)!;
        }

        public async Task SendMessageAsync<T>(T message)
        {
            var dataContractJsonSerializer = new DataContractJsonSerializer(typeof(T));

            var memoryStream = new MemoryStream(_sendBuffer);
            dataContractJsonSerializer.WriteObject(memoryStream, message);
            using (await _writeLock.LockAsync())
            {
                await SendBufferStreamAsync(_sendBuffer, (int) memoryStream.Position); //todo: all must be checked math
            }
        }

        protected abstract Task SendBufferStreamAsync(byte[] bytes, int length);

        protected abstract Task<int> ReceiveBufferStreamAsync(byte[] buffer, CancellationToken cancellationToken);
    }
}