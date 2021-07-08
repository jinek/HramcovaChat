using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;
using ChatHelpers;
using Microsoft.Extensions.DependencyInjection;

namespace ChatContract
{
    public sealed class ChatServerCore
    {
        private readonly IServiceProvider _serviceProvider;

        public ChatServerCore(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        private class ChatConnectedClient
        {
            private readonly string UserName;
            public readonly IConnection Connection;

            public ChatConnectedClient(string userName, IConnection connection)
            {
                UserName = userName;
                Connection = connection;
            }
        }

        private readonly ConcurrentDictionary<ChatConnectedClient, object?> _connectedClients = new();
        private readonly ConcurrentStack<ChatProtocolMessage> _messages = new();
        public async Task ProcessServerWorkflow()
        {
            using IServiceScope scope = _serviceProvider.CreateScope()!;
            IConnection connection = scope.ServiceProvider.GetRequiredService<IConnection>();
            var loginMessage = await connection.ReceiveMessageAsync<LoginMessage>(ChatProtocol.SendReceiveTimeout);
            string login = loginMessage.Login;
            var chatConnection = new ChatConnectedClient(login, connection);
            if (!_connectedClients.TryAdd(chatConnection, null))
                throw new NotImplementedException();

            try
            {
                if (loginMessage.Retrieve30Messages)
                {
                    await _messages.ForeachAsync(async message => await connection.SendMessageAsync(message));
                }
                using(var _ = ChatProtocolMessage.UserNameToSet.StartParameterRegion("Admin"))
                {
                    await BroadcastMessage(new ChatProtocolMessage($"Welcome to the chat, {login}"));
                }

                while (true)
                {
                    ChatProtocolMessage chatProtocolMessage;
                    try
                    {
                        using IDisposable
                            _ = ChatProtocolMessage.UserNameToSet.StartParameterRegion(login)!; //todo: почему nullable?
                        chatProtocolMessage =
                            await connection.ReceiveMessageAsync<ChatProtocolMessage>(ChatProtocol.PingTimeout +
                                ChatProtocol.SendReceiveTimeout);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }

                    if (chatProtocolMessage.HasMessage)
                    {
                        _messages.Push(chatProtocolMessage);
                        await BroadcastMessage(chatProtocolMessage);
                    }
                }
            }
            finally
            {
                if (!_connectedClients.TryRemove(chatConnection, out _))
                    throw new NotImplementedException();
            }

            async Task BroadcastMessage(ChatProtocolMessage chatProtocolMessage)
            {
                await _connectedClients
                    .Keys
                    /*.Where(client => client != chatConnection) broadcasting message to same user as well*/
                    .ForeachAsync(async client =>
                        await client.Connection.SendMessageAsync(chatProtocolMessage));
            }
        }
    }
}