using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using ChatContract;
using ChatContract.Connections;
using ChatContract.Messages;
using ChatContract.Workflows;
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
        private static void Main()
        {
            Exceptions.HandleUnobservedExceptions();

            new HostBuilder()
                .UseConsoleLifetime()
                .ConfigureLogging(builder => builder
                    .AddDebug()
                    .SetMinimumLevel(LogLevel.Trace))
                .ConfigureServices(collection => collection
                    .AddSingleton<IUiInputOutput, ConsoleInputOutput>()
                    .AddSingleton<ChatServerCore>()
                    .AddScoped<IConnection>(_ =>
                    {
                        WebSocket? currentWebSocket = WebSocketChatMiddleware.WebSocketToUse.CurrentValue;
                        TcpClient? currentTcpClient = TcpListenerHostedService.TcpClientToUse.CurrentValue;

                        if (currentWebSocket != null)
                        {
                            if (currentTcpClient != null)
                                throw new InvalidOperationException(
                                    "Для одного потока скоуп должен быть либо установлен из обработки " +
                                    "WebServer, либо из TcpListener. Оба не может быть.");
                            return new WebSocketConnection(currentWebSocket!);
                        }

                        if (currentTcpClient != null)
                            return new TcpSocketConnection(currentTcpClient);

                        throw new InvalidOperationException("Должен быть один из скоупов.");
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