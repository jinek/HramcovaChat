using System;
using System.Collections;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ChatHelpers
{
    public static class TaskExtensions
    {
        public static async void FastFailOnException(this Task task)
        {
            //using this because not to wait the finalizing of the task
            try
            {
                await task;
            }
            catch (Exception exception)
            {
                Environment.FailFast("Unexpected exception",
                    exception);
            }
        }

        /// <summary>
        /// Это тоже самое, что WhenAny, но не прячет исключения
        /// <see cref="Task.WhenAny"/>
        /// </summary>
        public static Task WhenAny_Normal(params Task[] tasks)
        {
            var taskCompletionSource = new TaskCompletionSource();

            foreach (Task task in tasks)
            {
                task.ContinueWith(finishedTask =>
                {
                    if (finishedTask.IsCanceled)
                        taskCompletionSource.TrySetCanceled();
                    if (finishedTask.IsFaulted)
                    {
                        AggregateException finishedTaskException = finishedTask.Exception!;
                        AggregateException flattened = finishedTaskException.Flatten();
                        Exception exceptionToSet = flattened.InnerExceptions.Count == 1
                            ? flattened.InnerException!
                            : finishedTaskException;
                        taskCompletionSource.TrySetException(exceptionToSet);
                    }

                    taskCompletionSource.TrySetResult();
                });
            }

            return taskCompletionSource.Task;
        }
    }
}