using System;
using System.Threading.Tasks;
using ChatContract;

namespace ChatConsoleClient
{
    internal class ConsoleInputOutput : IUiInputOutput
    {
        public async Task<string> InputAsync()
        {
            // creating one more thread to release current
            return await Task.Run(Console.ReadLine) ?? throw new NotImplementedException();
        }

        public void Output(string text)
        {
            Console.WriteLine(text);
        }
    }
}