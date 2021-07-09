using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ChatContract;
using ChatContract.Messages;
using ChatContract.Workflows;
using ChatHelpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ChatServer
{
    /// <summary>
    /// Слушает TCP клиентов и запускает для них workflow чата <see cref="ChatServerCore.ProcessServerWorkflow"/>
    /// </summary>
    internal class TcpListenerHostedService : BackgroundService
    {
        public static readonly ThreadStaticParameter<TcpClient?> TcpClientToUse = new();
        private readonly ChatServerCore _chatServerCore;
        private readonly TcpListener _tcpListener;

        public TcpListenerHostedService(ChatServerCore chatServerCore)
        {
            _chatServerCore = chatServerCore;
            _tcpListener = TcpListener.Create(ChatProtocol.SocketPortNumber);
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _tcpListener.Start();
            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _tcpListener.Stop();
            return base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                TcpClient tcpClient = await _tcpListener.AcceptTcpClientAsync();
                using IDisposable _ = TcpClientToUse.StartParameterRegion(tcpClient)!;
                _chatServerCore.ProcessServerWorkflow().FastFailOnException(); //todo: dispose
                //todo: здесь некрасиво выглядет диспоуз _, так как потом async опирация выполняется
            }
        }
    }
}