using System;
using System.Threading;
using System.Threading.Tasks;
using ChatContract.Connections;
using ChatContract.Messages;
using ChatHelpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TaskExtensions = ChatHelpers.TaskExtensions;

namespace ChatContract.Workflows
{
    /// <summary>
    /// Это workflow чата на стороне клиента (подключается, спрашивает логин у пользователя,
    /// отправляет на сервер, запускает пинг и т.д.
    /// </summary>
    public sealed class ChatClientCoreBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public ChatClientCoreBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        //todo: dotnet игнорирует исключения в backgrountasks. 
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Для простоты указываем флаг получения 30 сообщений здесь, этот метод вызовется только из консоли
            Task executeInternalAsync = ExecuteInternalAsync(stoppingToken, true);
            executeInternalAsync.FastFailOnException();
            return executeInternalAsync;
        }

        public async Task ExecuteInternalAsync(CancellationToken stoppingToken, bool retrieve30Messages = false)
        {
            // localCts используем что бы фейл в одном цикле остановил все три
            var localCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            stoppingToken = localCts.Token;

            using IServiceScope scope = _serviceProvider.CreateScope()!;
            IConnection connection = scope.ServiceProvider.GetRequiredService<IConnection>();
            IUiInputOutput uiInputOutput = scope.ServiceProvider.GetRequiredService<IUiInputOutput>();

            uiInputOutput.Output("Please, enter your name");
            string userName = await uiInputOutput.InputAsync(stoppingToken);
            try
            {
                await connection.SendMessageAsync(new LoginMessage(userName, retrieve30Messages), stoppingToken);

                await TaskExtensions.WhenAny_Normal(
                    RepeatUntilCancelled(async () =>
                    {
                        // ping
                        await Task.Delay(ChatProtocol.PingTimeout, stoppingToken);
                        await connection.SendMessageAsync(new ChatProtocolMessage(),
                            stoppingToken); //todo: create static PingMessage
                    }),
                    RepeatUntilCancelled(async () =>
                    {
                        // reading from input and sending to server
                        string newMessage = await uiInputOutput.InputAsync(stoppingToken);
                        await connection.SendMessageAsync(new ChatProtocolMessage(newMessage), stoppingToken);
                    }),
                    RepeatUntilCancelled(async () =>
                    {
                        // receiving messages from the server
                        var message =
                            await connection.ReceiveMessageAsync<ChatProtocolMessage>(null, stoppingToken);
                        if (!message.HasMessage)
                            throw new ConnectivityException("Protocol violation by the peer");
                        uiInputOutput.Output($"{message.Login ?? "<Admin>"}: {message.Message}");
                    }));
            }
            catch (ConnectivityException connectivityException)
            {
                uiInputOutput.Output(
                    $"You are disconnected from chat because of connectivity issue: {connectivityException.Message}.");

                //выключаем все три цикла, если что-то одно перестало работать
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