using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Text;
using System.Threading;
using ChatContract;
using ChatContract.Connections;
using ChatContract.Messages;
using ChatContract.Workflows;
using ChatHelpers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ChatWebClient
{
    public class Program
    {
        //todo: make internal/private
        public static async Task Main()
        {
            Exceptions.HandleUnobservedExceptions(); //todo: разобраться с исключениями в Blazor

            var builder = WebAssemblyHostBuilder.CreateDefault();
            
            // Создаём подключение к серверу
            using var clientWebSocket = new ClientWebSocket();
            using var connectionCts = new CancellationTokenSource(ChatProtocol.SendReceiveTimeout);
            try
            {
                App.HostName = new Uri(builder.HostEnvironment.BaseAddress).Host;
                var uri = new Uri($@"ws://{App.HostName}:{ChatProtocol.HttpPortNumber}");
                await clientWebSocket.ConnectAsync(uri, connectionCts.Token);
            }
            catch (WebSocketException)
            {
                /*Also will be handled by WebSocketConnection*/
            }

            //Запускаем приложение
            
            builder.RootComponents.Add<App>("#app");
            builder.Services
                .AddSingleton<IUiInputOutput>(_ => App.AppToSet.CurrentValue ?? throw new NotImplementedException())
                .AddSingleton<IConnection>(_ => new WebSocketConnection(clientWebSocket))
                .AddSingleton<ChatClientCoreBackgroundService>();

            await builder.Build().RunAsync();
        }
    }
}