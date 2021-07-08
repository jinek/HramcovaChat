using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using ChatContract;
using ChatHelpers;
using Microsoft.Extensions.DependencyInjection;

namespace ChatWebClient
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public partial class App : IUiInputOutput
    {
        private readonly AsyncLock _inputLock = new();
        private TaskCompletionSource<string>? _taskCompletionSource;

        private TaskCompletionSource<string>? TaskCompletionSource
        {
            get => _taskCompletionSource;
            set
            {
                _taskCompletionSource = value;
                Sending = value == null;
            }
        }

        private bool Sending;

        public async Task<string> InputAsync()
        {
            Console.WriteLine("App: requisting input");
            using IDisposable _ = await _inputLock.LockAsync();
            Console.WriteLine("App: Lock passed");

            try
            {
                TaskCompletionSource = new TaskCompletionSource<string>();
                return await TaskCompletionSource.Task;
            }
            finally
            {
                Console.WriteLine("App: finally called");

                TaskCompletionSource = null;
                Console.WriteLine("App: set to null");

            }
        }

        public void Output(string text)
        {
            InvokeAsync(() =>
            {
                _messages.Insert(0,$"{text}");
                StateHasChanged();
            }).FastFailOnException();
        }

        internal static readonly ThreadStaticParameter<App> AppToSet = new();

        protected override async Task OnInitializedAsync()
        {
            using IDisposable _ = AppToSet.StartParameterRegion(this);
            _serviceProvider.GetRequiredService<ChatClientCoreBackgroundService>()
                .StartAsync(CancellationToken.None).FastFailOnException();
            //todo: если не залогинились вовремя (видимо пофиг)
            await base.OnInitializedAsync();
        }
    }
}