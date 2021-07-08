using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Text;
using System.Threading;
using ChatContract;
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
        public static async Task Main(string[] args)
        {
            Debug.WriteLine("Hello 1");
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            using var clientWebSocket = new ClientWebSocket();
            await clientWebSocket.ConnectAsync(new Uri(@"ws://localhost:5000"), CancellationToken.None);
            builder.RootComponents.Add<App>("#app");//todo: exception handling
            builder.Services
                .AddSingleton<IUiInputOutput>(_ => App.AppToSet.CurrentValue )
                .AddSingleton<IConnection>(_ =>
                {
                    Console.WriteLine("Requisting websocketConnection");
                    var webSocketConnection = new WebSocketConnection(clientWebSocket);
                    Console.WriteLine($"returning websocket: {clientWebSocket.State}");
                    return webSocketConnection;
                })
                .AddSingleton<ChatClientCoreBackgroundService>();
                
            await builder.Build().RunAsync();
        }
    }
}