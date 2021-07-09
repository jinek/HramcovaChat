using System;
using System.Linq;
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
        private static void Main(string[] args)
        {
            string address = "127.0.0.1";

            new HostBuilder()
                .UseConsoleLifetime()
                .ConfigureLogging(builder => builder.AddDebug().SetMinimumLevel(LogLevel.Trace)) //todo: это копипаст
                .ConfigureServices(collection => collection
                    .AddSingleton<IUiInputOutput, ConsoleInputOutput>()
                    .AddSingleton<IConnection>(ConnectionFactory)
                    .AddSingleton<ChatClientCoreBackgroundService>()
                    .AddHostedService<ChatClientCoreBackgroundService>())
                .Build()
                .Run();
            
            IConnection ConnectionFactory(IServiceProvider serviceProvider)
            {
                var uiInputOutput = serviceProvider.GetRequiredService<IUiInputOutput>();
                try
                {
                    if (args.Any())
                    {
                        address = args[0];
                        if (string.IsNullOrEmpty(address))
                        {
                            OutputAndExit($"Can not connect to server: {address}");
                        }
                    }
                    else
                    {
#if DEBUG
                        address = "127.0.0.1";
#endif
                        OutputAndExit("Please, provide server name as first argument");
                    }

                    uiInputOutput.Output($"Connecting to {address}");
                    var socket = new TcpClient(address, ChatProtocol.SocketPortNumber);
                    return new TcpSocketConnection(socket);
                }
                catch (SocketException socketException)
                {
                    OutputAndExit($"Can not connect to server: {socketException.Message}");
                    throw;
                }

                void OutputAndExit(string text)
                {
                    uiInputOutput
                        .Output(text);
                    Environment.Exit(3);
                }
            }
        }
    }
}