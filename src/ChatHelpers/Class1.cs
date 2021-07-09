using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ChatHelpers
{
    public static class TaskExtensions
    {
        public static async void FastFailOnException(this Task task)
        {
            //using this because not to wait the finilizing of the task
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

    public static class TaskLinqExtensions
    {
        public static async Task ForeachAsync<T>(this IEnumerable<T> enumerable, Func<T,Task> action)
        {
            foreach (T value in enumerable)
            {
                await action(value);
            }
        }

    }
}