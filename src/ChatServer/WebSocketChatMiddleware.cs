using System;
using System.Net.WebSockets;
using System.Threading.Tasks;
using ChatContract;
using ChatContract.Workflows;
using ChatHelpers;
using Microsoft.AspNetCore.Http;

namespace ChatServer
{
    /// <summary>
    /// Ждёт подключения <see cref="WebSocket"/> и запускает для них <see cref="ChatServerCore.ProcessServerWorkflow"/>
    /// </summary>
    internal class WebSocketChatMiddleware
    {
        public static readonly ThreadStaticParameter<WebSocket?> WebSocketToUse = new();

        private readonly RequestDelegate _next;

        public WebSocketChatMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ChatServerCore chatServerCore)
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                await _next(context);
                return;
            }

            //todo: эти HostedServices/MiddleWare могут быть объеденены

            using WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
            using IDisposable _ = WebSocketToUse.StartParameterRegion(webSocket)!;
            await chatServerCore.ProcessServerWorkflow();
        }
    }
}