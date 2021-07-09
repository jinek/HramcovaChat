using System;
using System.Threading;
using System.Threading.Tasks;

namespace ChatContract
{
    /// <summary>
    /// <exception cref="ConnectivityException">Must be thrown when there are IO/connectivity issues</exception>
    /// </summary>
    public interface IConnection
    {
        Task<T> ReceiveMessageAsync<T>(TimeSpan? sendReceiveTimeout, CancellationToken cancellationToken);
        Task SendMessageAsync<T>(T message, CancellationToken cancellationToken);
    }

    public class ConnectivityException : Exception
    {
        /// <summary>
        /// Message of inner exception will be shown to the user
        /// </summary>
        /// <param name="innerException"></param>
        public ConnectivityException(Exception innerException) : base(innerException.Message, innerException)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message">Will be shown to the user</param>
        public ConnectivityException(string message) : base(message)
        {
        }
    }
}