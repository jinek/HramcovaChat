using System;
using System.Threading;
using System.Threading.Tasks;
using ChatHelpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ChatContract
{
    public sealed class ChatClientCoreBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public ChatClientCoreBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using IServiceScope scope = _serviceProvider.CreateScope()!;
            IConnection connection = scope.ServiceProvider.GetRequiredService<IConnection>();
            IUiInputOutput uiInputOutput = scope.ServiceProvider.GetRequiredService<IUiInputOutput>();

            uiInputOutput.Output("Please, enter your name");
            string userName = await uiInputOutput.InputAsync();
            await connection.SendMessageAsync(new LoginMessage(userName));
            
            Task.Run(async () =>
            {
                //Ping
                while (true)
                {
                    //todo: закрываться, если потеряна связь при отправке
                    await Task.Delay(ChatProtocol.PingTimeout);
                    await connection.SendMessageAsync(new ChatProtocolMessage()); //todo: PingMessage static
                }
            }).FastFailOnException();

            Task.Run(async () =>
            {
                while (true)
                {
                    string newMessage = await uiInputOutput.InputAsync();
                    await connection.SendMessageAsync(new ChatProtocolMessage(newMessage));
                }
            }).FastFailOnException();

            await Task.Run(async () =>
            {
                while (true)
                {
                    //todo: закрываться, если потеряна связь при отправке
                    try
                    {
                        var message =
                            await connection.ReceiveMessageAsync<ChatProtocolMessage>(ChatProtocol.PingTimeout);
                        if (!message.HasMessage)
                            throw new NotImplementedException();
                        uiInputOutput.Output($"New message from {message.Login}: {message.Message}");
                    }
                    catch (OperationCanceledException)
                    {
                    }
                }
            });
        }
    }
}