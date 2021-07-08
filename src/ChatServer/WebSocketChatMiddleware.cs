using System;
using System.Net.WebSockets;
using System.Threading.Tasks;
using ChatContract;
using ChatHelpers;
using Microsoft.AspNetCore.Http;

namespace ChatServer
{
    internal class WebSocketChatMiddleware
    {
        public static readonly ThreadStaticParameter<WebSocket?> WebSocketToUse = new();
        
        private readonly RequestDelegate _next;

        public WebSocketChatMiddleware(RequestDelegate next)
        {/*"applicationUrl": "http://localhost:5005", todo: это убрать*/
            _next = next;
        }

        //todo: nullable
        //todo: handle shutdown
        //todo: будет ли автоматом вызываться IDispose
        public async Task InvokeAsync(HttpContext context, ChatServerCore chatServerCore)
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                await _next(context);
                return;
            }
            
            //todo: эти HostedServices/MiddleWare жесть как похожи
            
//todo: потом посмотреть диаграмму классов и решить что в какие сборки положить
            using WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
            using IDisposable _ = WebSocketToUse.StartParameterRegion(webSocket)!;
            await chatServerCore.ProcessServerWorkflow();
        }
    }
}