using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatHelpers
{
    public static class TaskExtensions
    {
        public static void FastFailOnException(this Task task)
        {//using this because not to wait the finilizing of the task
            task.ContinueWith(
                task1 =>
                {
                    Environment.FailFast("Unexpected exception while processing Socket workflow",
                        task1.Exception!.InnerException);
                }, TaskContinuationOptions.OnlyOnFaulted);
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