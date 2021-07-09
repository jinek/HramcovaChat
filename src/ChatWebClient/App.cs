using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using ChatContract;
using ChatContract.Workflows;
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

        async Task<string> IUiInputOutput.InputAsync(CancellationToken cancellationToken)
        {
            using IDisposable _ = await _inputLock.LockAsync(cancellationToken);

            try
            {
                TaskCompletionSource = new TaskCompletionSource<string>();
                await using CancellationTokenRegistration cancellationTokenRegistration = cancellationToken.Register(
                    () =>
                    {
                        ((IUiInputOutput) this).Output("You can close the window.");
                        TaskCompletionSource.TrySetCanceled();
                    });
                return await TaskCompletionSource.Task;
            }
            finally
            {
                TaskCompletionSource = null;
            }
        }

        void IUiInputOutput.Output(string text)
        {
            InvokeAsync(() =>
            {
                _messages.Insert(0, $"{text}");
                StateHasChanged();
            }).FastFailOnException();
        }

        internal static readonly ThreadStaticParameter<App> AppToSet = new();
        // ReSharper disable once CA2211 для скорости сделал, не имеет значения сейчас
        public static string HostName="127.0.0.1";

        protected override async Task OnInitializedAsync()
        {
            using IDisposable _ = AppToSet.StartParameterRegion(this);

            RunChatWorkflow().FastFailOnException();

            await base.OnInitializedAsync();

            async Task RunChatWorkflow()
            {
                await _serviceProvider.GetRequiredService<ChatClientCoreBackgroundService>()
                    .ExecuteInternalAsync(CancellationToken.None);

                // Disabling the UI
                await InvokeAsync(() =>
                {
                    Sending = true;
                    StateHasChanged();
                });
            }
        }
    }
}