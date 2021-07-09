using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using ChatContract;
using ChatHelpers;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace ChatServer
{
    internal static class Program
    {
        public static void Main()
        {
            //todo: check exceptions
            new HostBuilder()
                .UseConsoleLifetime()
                .ConfigureLogging(builder => builder
                    .AddDebug()
                    .SetMinimumLevel(LogLevel.Trace))
                .ConfigureServices(collection => collection
                    .AddSingleton<IUiInputOutput,ConsoleInputOutput>()
                    .AddSingleton<ChatServerCore>()
                    .AddScoped<IConnection>(_ =>
                    {
                        WebSocket? currentWebSocket = WebSocketChatMiddleware.WebSocketToUse.CurrentValue;
                        TcpClient? currentTcpClient = TcpListenerHostedService.TcpClientToUse.CurrentValue;

                        if (currentWebSocket != null)
                        {
                            if (currentTcpClient != null)
                                throw new NotImplementedException();
                            return new WebSocketConnection(currentWebSocket!);
                        }

                        if (currentTcpClient != null)
                            return new TcpSocketConnection(currentTcpClient);

                        throw new NotImplementedException();
                    })
                    .AddHostedService<TcpListenerHostedService>())
                .ConfigureWebHost(builder => builder
                    .CaptureStartupErrors(false)
                    .UseKestrel(options => options.ListenAnyIP(ChatProtocol.HttpPortNumber))
                    .Configure(app => app
                        .UseWebSockets()
                        .UseMiddleware<WebSocketChatMiddleware>()))
                .Build()
                .Run();
        }
    }
}