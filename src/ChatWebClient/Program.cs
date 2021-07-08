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
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            using var clientWebSocket = new ClientWebSocket();
            using var connectionCts = new CancellationTokenSource(ChatProtocol.SendReceiveTimeout);
            await clientWebSocket.ConnectAsync(new Uri(@"ws://localhost:5000"), connectionCts.Token);
            
            builder.RootComponents.Add<App>("#app"); //todo: exception handling
            builder.Services
                .AddSingleton<IUiInputOutput>(_ => App.AppToSet.CurrentValue ?? throw new NotImplementedException())
                .AddSingleton<IConnection>(_ => new WebSocketConnection(clientWebSocket))
                .AddSingleton<ChatClientCoreBackgroundService>();

            await builder.Build().RunAsync();
        }
    }
}