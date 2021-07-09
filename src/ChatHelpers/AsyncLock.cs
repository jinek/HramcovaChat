using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace ChatHelpers
{
    // idea from Microsoft.EntityFrameworkCore.Internal.AsyncLock
    public sealed class AsyncLock
    {
        //todo: tests
        private readonly SemaphoreSlim _semaphore = new(1);
        private readonly Releaser _releaser;
        private readonly Task<IDisposable> _releaserTaskCached;

        public AsyncLock()
        {
            _releaser = new Releaser(this);
            _releaserTaskCached = Task.FromResult((IDisposable) _releaser);
        }

        public Task<IDisposable> LockAsync(CancellationToken cancellationToken = default)
        {
            Task wait = _semaphore.WaitAsync(cancellationToken)!;

            return wait.IsCompleted
                ? _releaserTaskCached!
                : wait.ContinueWith(
                    (_, state) => (IDisposable) ((AsyncLock) state!)._releaser!,
                    this, CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default)!;
        }

        private readonly struct Releaser : IDisposable
        {
            private readonly AsyncLock _toRelease;

            internal Releaser([NotNull] AsyncLock toRelease)
            {
                _toRelease = toRelease;
            }

            public void Dispose()
                => _toRelease._semaphore.Release();
        }
    }
}