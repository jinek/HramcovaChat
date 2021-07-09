using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChatHelpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ChatContract
{
    public sealed class ChatServerCore
    {
        public const string LogCategoryName = "ChatServer";
        private readonly IServiceProvider _serviceProvider;
        private readonly IUiInputOutput _inputOutput;
        private readonly IHostApplicationLifetime _applicationLifetime;

        public ChatServerCore(
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory,
            IUiInputOutput inputOutput,
            IHostApplicationLifetime applicationLifetime)
        {
            _serviceProvider = serviceProvider;
            _inputOutput = inputOutput;
            _applicationLifetime = applicationLifetime;

            // for simplicity
            RunUiCommands().FastFailOnException();
        }

        public const string ServerStartedCode = "Server started";
        
        private async Task RunUiCommands()
        {
            //не понятна где должна быть эта логика так как сервер один
            _inputOutput.Output(ServerStartedCode);
            while (!_applicationLifetime.ApplicationStopping.IsCancellationRequested)
            {
                string command = await _inputOutput.InputAsync(_applicationLifetime.ApplicationStopping);
                switch (command)
                {
                    case "exit":
                        _inputOutput.Output("Exiting...");
                        _applicationLifetime.StopApplication();
                        break;
                    case "ls":
                    {
                        if(!_connectedClients.Any())
                            _inputOutput.Output("No clients authorized");
                        foreach (ChatConnectedClient connectedClientsKey in _connectedClients.Keys)
                        {
                            _inputOutput.Output(connectedClientsKey.UserName);
                        }
                    }
                        break;
                    default:
                        _inputOutput.Output("Unknown command");
                        break;
                }
            }
        }

        private class ChatConnectedClient
        {
            public readonly string UserName;
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
            _inputOutput.Output("New client connected");
            string? login = null;
            try
            {
                var loginMessage =
                    await connection.ReceiveMessageAsync<LoginMessage>(ChatProtocol.PingTimeout,
                        CancellationToken.None);
                login = loginMessage.Login;
                _inputOutput.Output($"Authorized as {login}");
                var chatConnection = new ChatConnectedClient(login, connection);
                if (!_connectedClients.TryAdd(chatConnection, null))
                    throw new NotImplementedException();

                try
                {
                    if (loginMessage.Retrieve30Messages)
                    {
                        await _messages.ForeachAsync(async message =>
                            await connection.SendMessageAsync(message, CancellationToken.None));
                    }

                    using (var _ = ChatProtocolMessage.UserNameToSet.StartParameterRegion("Admin"))
                    {
                        await BroadcastMessage(new ChatProtocolMessage($"Welcome to the chat, {login}"));
                    }

                    while (true)
                    {
                        ChatProtocolMessage chatProtocolMessage;
                        try
                        {
                            using IDisposable
                                _ = ChatProtocolMessage.UserNameToSet
                                    .StartParameterRegion(login)!; //todo: почему nullable?
                            chatProtocolMessage =
                                await connection.ReceiveMessageAsync<ChatProtocolMessage>(ChatProtocol.PingTimeout +
                                    ChatProtocol.SendReceiveTimeout, CancellationToken.None);
                        }
                        catch (OperationCanceledException)
                        {
                            throw new ConnectivityException($"No response from {login} for {ChatProtocol.PingTimeout}");
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
                    await BroadcastMessage(new ChatProtocolMessage($"User {login} is leaving the chat"));
                }
            }
            catch (ConnectivityException connectivityException)
            {
                _inputOutput.Output(
                    $"User {login ?? "<Login name not provided>"} has just disconnected: {connectivityException.Message}");
            }

            async Task BroadcastMessage(ChatProtocolMessage chatProtocolMessage)
            {
                await _connectedClients
                    .Keys
                    /*.Where(client => client != chatConnection) broadcasting message to same user as well*/
                    .ForeachAsync(async client =>
                        await client.Connection.SendMessageAsync(chatProtocolMessage, CancellationToken.None));
            }
        }
    }
}