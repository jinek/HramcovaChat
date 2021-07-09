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

        public async Task<string> InputAsync(CancellationToken cancellationToken)
        {
            using IDisposable _ = await _inputLock.LockAsync(cancellationToken);

            try
            {
                TaskCompletionSource = new TaskCompletionSource<string>();
                await using CancellationTokenRegistration cancellationTokenRegistration = cancellationToken.Register(
                    () =>
                    {
                        Output("You can close the window.");
                        TaskCompletionSource.TrySetCanceled();
                    });
                return await TaskCompletionSource.Task;
            }
            finally
            {
                TaskCompletionSource = null;
            }
        }

        public void Output(string text)
        {
            InvokeAsync(() =>
            {
                _messages.Insert(0, $"{text}");
                StateHasChanged();
            }).FastFailOnException();
        }

        internal static readonly ThreadStaticParameter<App> AppToSet = new();
        public static string HostName;

        protected override async Task OnInitializedAsync()
        {
            using IDisposable _ = AppToSet.StartParameterRegion(this);
            RunChatWorkflow().FastFailOnException();
            //todo: если не залогинились вовремя (видимо пофиг)
            await base.OnInitializedAsync();

            async Task RunChatWorkflow()
            {
                await _serviceProvider.GetRequiredService<ChatClientCoreBackgroundService>()
                    .ExecuteInternalAsync(CancellationToken.None);

                await InvokeAsync(() =>
                {
                    Sending = true;
                    StateHasChanged();
                });
            }
        }
    }
}