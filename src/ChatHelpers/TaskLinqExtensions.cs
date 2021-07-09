using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatHelpers
{
    public static class TaskLinqExtensions
    {
        public static async Task ForeachAsync<T>(this IEnumerable<T> enumerable, Func<T, Task> action)
        {
            foreach (T value in enumerable)
            {
                await action(value);
            }
        }
    }
}