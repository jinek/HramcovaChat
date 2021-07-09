using System;
using System.Threading;
using System.Threading.Tasks;
using ChatHelpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TaskExtensions = ChatHelpers.TaskExtensions;

namespace ChatContract
{
    public sealed class ChatClientCoreBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public ChatClientCoreBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

//todo: делать await завершения таска, потому что там могут быть ошибки
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //this is called only from console.
            Task executeInternalAsync = ExecuteInternalAsync(stoppingToken, true);
            executeInternalAsync.FastFailOnException();
            return executeInternalAsync;
        }

        public async Task ExecuteInternalAsync(CancellationToken stoppingToken, bool retrieve30Messages=false)
        {
            var localCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            stoppingToken = localCts.Token;
            
            using IServiceScope scope = _serviceProvider.CreateScope()!;
            IConnection connection = scope.ServiceProvider.GetRequiredService<IConnection>();
            IUiInputOutput uiInputOutput = scope.ServiceProvider.GetRequiredService<IUiInputOutput>();

            uiInputOutput.Output("Please, enter your name");
            string userName = await uiInputOutput.InputAsync(stoppingToken);
            try
            {
                await connection.SendMessageAsync(new LoginMessage(userName, retrieve30Messages),stoppingToken);

                await TaskExtensions.WhenAny_Normal(
                    RepeatUntilCancelled(async () =>
                    {
                        //ping
                        //todo: закрываться, если потеряна связь при отправке
                        await Task.Delay(ChatProtocol.PingTimeout, stoppingToken);
                        await connection.SendMessageAsync(new ChatProtocolMessage(), stoppingToken); //todo: PingMessage static
                    }),
                    RepeatUntilCancelled(async () =>
                    {
                        string newMessage = await uiInputOutput.InputAsync(stoppingToken);
                        await connection.SendMessageAsync(new ChatProtocolMessage(newMessage), stoppingToken);
                    }),
                    RepeatUntilCancelled(async () =>
                    {
                        var message =
                            await connection.ReceiveMessageAsync<ChatProtocolMessage>(null,stoppingToken);
                        if (!message.HasMessage)
                            throw new NotImplementedException();
                        uiInputOutput.Output($"{message.Login ?? "<Admin>"}: {message.Message}");
                    }));
            }
            catch (ConnectivityException connectivityException)
            {
                uiInputOutput.Output(
                    $"You are disconnected from chat because of connectivity issue: {connectivityException.Message}.");
                localCts.Cancel();
            }
            
            uiInputOutput.Output("You can close chat now");

            Task RepeatUntilCancelled(Func<Task> actionToRepeat)
            {
                return Task.Run(async () =>
                {
                    while (!stoppingToken.IsCancellationRequested)
                        await actionToRepeat();
                }, stoppingToken);
            }
        }
    }
}