using System;
using System.Threading;
using System.Threading.Tasks;

namespace ChatContract.Connections
{
    /// <summary>
    /// Базовый класс коммуникации.
    /// <exception cref="ConnectivityException">Must be thrown when there are IO/connectivity issues</exception>
    /// </summary>
    public interface IConnection
    {
        Task<T> ReceiveMessageAsync<T>(TimeSpan? sendReceiveTimeout, CancellationToken cancellationToken);
        Task SendMessageAsync<T>(T message, CancellationToken cancellationToken);
    }
}