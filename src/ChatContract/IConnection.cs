using System;
using System.Threading.Tasks;

namespace ChatContract
{
    public interface IConnection
    {
        Task<T> ReceiveMessageAsync<T>(TimeSpan sendReceiveTimeout);
        Task SendMessageAsync<T>(T message);
    }
}