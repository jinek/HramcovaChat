using System;
using System.Threading;

namespace ChatHelpers
{
    /// <summary>
    /// Аналогия <see cref="ThreadStaticParameter{T}"/> но сохраняет сообщение для дочерних потоков/тасков
    /// </summary>
    public sealed class ThreadStaticParameter<T>
    {
        //todo: add unit tests
        private readonly AsyncLocal<T?> _currentValue = new();

        public T? CurrentValue => _currentValue.Value;

        public IDisposable StartParameterRegion(T value)
        {
            _currentValue.Value = value;
            return new Region(this)!;
        }

        private class Region : IDisposable
        {
            //todo: low, should save previous value, not reset to default (add stack)
            private readonly ThreadStaticParameter<T> _parent;

            public Region(ThreadStaticParameter<T> parent)
            {
                _parent = parent;
            }

            public void Dispose()
            {
                ReleaseUnmanagedResources();
                GC.SuppressFinalize(this);
            }

            private void ReleaseUnmanagedResources()
            {
                //todo: low check same thread or child thread
                _parent._currentValue.Value = default;
            }

            ~Region()
            {
                throw new InvalidOperationException(
                    "This thread static parameter has not been explicitly disposed"); //todo: can happen on application close
            }
        }
    }
}