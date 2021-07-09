using System;

namespace ChatIntegrationTests
{
    public class ShellCmdException : ApplicationException
    {
        public readonly int ExitCode;

        public ShellCmdException(int exitCode, string outputMessage) : base(
            $"Process exit code was {exitCode}: {outputMessage}")
        {
            ExitCode = exitCode;
        }
    }
}