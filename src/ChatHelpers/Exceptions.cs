using System;
using System.Threading.Tasks;

namespace ChatHelpers
{
    public static class Exceptions
    {
        /// <summary>
        /// Если случилось неожиданное исключение, то не игнорируем, аналогично с AppDomain.UnhandledException
        /// </summary>
        public static void HandleUnobservedExceptions()
        {
            TaskScheduler.UnobservedTaskException += (_, args) =>
            {
                if (!args.Observed)
                {
                    Environment.FailFast("There was an exception from unobserved task.", args.Exception);
                }
            };
        }
    }
}