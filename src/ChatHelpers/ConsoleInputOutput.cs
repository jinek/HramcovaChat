using System;
using System.Threading;
using System.Threading.Tasks;

namespace ChatHelpers
{
    public class ConsoleInputOutput : IUiInputOutput
    {
        public async Task<string> InputAsync(CancellationToken cancellationToken)
        {
            CancellationTokenRegistration cancellationTokenRegistration = cancellationToken.Register(() =>
            {
                Console.WriteLine("Input has been cancelled. Press enter to detach.");
            });

            try
            {
                // creating one more thread to release current
                return await Task.Run(async () =>
                {
                    string? readLine;
                    do
                    {
                        readLine = Console.ReadLine();

                        if (readLine != null)
                            break;

                        Output("No input attached. Waiting one more minute.");

                        await Task.Delay(60000, cancellationToken);
                    } while (!cancellationToken.IsCancellationRequested);

                    cancellationToken.ThrowIfCancellationRequested();
                    return readLine!;
                }, cancellationToken);
            }
            finally
            {
                cancellationTokenRegistration.Unregister();
            }
        }

        public void Output(string text)
        {
            Console.WriteLine(text);
        }
    }
}