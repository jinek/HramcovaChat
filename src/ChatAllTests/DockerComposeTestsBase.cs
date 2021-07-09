using System;
using ChatContract;

namespace ChatIntegrationTests
{
    public class DockerComposeTestsBase : IDisposable
    {
        private readonly ShellCmdCommand? _shellCmdCommand;

        protected DockerComposeTestsBase()
        {
            Console.WriteLine("Going to run slow integration tests");
            _shellCmdCommand = new ShellCmdCommand("pwsh", @".\RunDocker.ps1");
            do
            {
                string line = _shellCmdCommand.ReadLine();
                if (line.Contains(ChatServerCore.ServerStartedCode))
                    break;
            } while (!_shellCmdCommand.EOF);
        }

        void IDisposable.Dispose()
        {
            try
            {
                _shellCmdCommand.Shutdown();
            }
            catch (ShellCmdException)
            {

            }
            new ShellCmdCommand("pwsh", @".\dockercomposedown.ps1").WaitForExit();
        }
    }
}