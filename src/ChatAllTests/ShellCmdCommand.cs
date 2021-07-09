using System;
using System.Diagnostics;

namespace ChatIntegrationTests
{
    internal class ShellCmdCommand : IDisposable
    {
        private readonly Process _process;
        private string _errorOutput = string.Empty;

        public ShellCmdCommand(string fileName, string args, string? directory=null)
        {
            Debug.WriteLine($"Running: {fileName} {args}");

            var psi = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            if (directory != null)
            {
                psi.WorkingDirectory = directory;
            }

            _process = Process.Start(psi)!;
            _process.ErrorDataReceived += (_, eventArgs) =>
            {
                _errorOutput += eventArgs.Data;
                Debug.WriteLine(eventArgs.Data);
                Console.WriteLine(eventArgs.Data);
            };
            _process.BeginErrorReadLine();
        }

        public bool EOF => _process.StandardOutput.EndOfStream;

        public string? ReadLine()
        {
            string? readLine = _process.StandardOutput.ReadLine();
            Debug.WriteLine(readLine);
            Console.WriteLine(readLine);
            return readLine;
        }

        public void WriteLine(string line)
        {
            _process.StandardInput.WriteLine(line);
        }

        public void WaitForExit()
        {
            _process.WaitForExit();
            int pExitCode = _process.ExitCode;
            if (pExitCode != 0) throw new ShellCmdException(pExitCode, _errorOutput);

            string output = _process.StandardOutput.ReadToEnd();
            Debug.WriteLine(output);
        }

        public void Shutdown()
        {
            _process.Kill(true);
            try
            {
                WaitForExit();
            }
            catch (ShellCmdException shellCmdCommand) when (shellCmdCommand.ExitCode == -1)
            {
                // выполнение прервано нами
            }
        }

        void IDisposable.Dispose()
        {
            Shutdown();
        }
    }
}