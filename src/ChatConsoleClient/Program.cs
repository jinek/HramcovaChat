using System.Net.Sockets;
using ChatContract;
using ChatHelpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ChatConsoleClient
{
    internal static class Program
    {
        private static void Main()
        {
            //todo: SocketException

            new HostBuilder()
                .UseConsoleLifetime()
                .ConfigureLogging(builder => builder.AddDebug().SetMinimumLevel(LogLevel.Trace)) //todo: это копипаст
                .ConfigureServices(collection => collection
                    .AddSingleton<IUiInputOutput, ConsoleInputOutput>()
                    .AddSingleton<IConnection>(_ =>
                    {
                        var socket = new TcpClient("127.0.0.1", ChatProtocol.SocketPortNumber);
                        return new TcpSocketConnection(socket);
                    })
                    .AddSingleton<ChatClientCoreBackgroundService>()
                    .AddHostedService<ChatClientCoreBackgroundService>())
                .Build()
                .Run();
        }
    }
}