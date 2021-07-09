using System;
using System.IO;
using System.Net;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace ChatIntegrationTests
{
    /// <summary>
    /// Показана лишь идея интеграционных/комбинированных тестов
    /// Для тестов запускается docker-compose
    /// </summary>
    public class IntegrationTests : DockerComposeTestsBase
    {
        /// <summary>
        /// Атачимся к консоли сервера и проверяем как работают команды
        /// </summary>
        [Fact]
        public void ServerLsCommandReturn()
        {
            var shellCmdCommand = new ShellCmdCommand("pwsh", @".\dockerattach.ps1");
            shellCmdCommand.WriteLine("ls");
            Assert.True(!string.IsNullOrEmpty(shellCmdCommand.ReadLine()), "There must be some output for ls command");
            try
            {
                shellCmdCommand.Shutdown();
            }
            catch (ShellCmdException)
            {
            }
        }

        /// <summary>
        /// Запускаем консольных и веб клиентов, пишем сообщения и проверяем workflow
        /// </summary>
        [Fact]
        public void ComplexWorkflowTest()
        {
            string messageId = Guid.NewGuid().ToString();
            using (var shellCmdCommand = new ShellCmdCommand("dotnet", @"ChatConsoleClient.dll ""127.0.0.1"""))
            {
                const string testUser1 = "TestUser1";
                shellCmdCommand.WriteLine(testUser1);
                int i = 0;
                while (!shellCmdCommand.ReadLine().Contains(testUser1) && i < 11) i++;
                Assert.True(i < 10, "Greetings message must come within 10 messages");
                shellCmdCommand.WriteLine($"Hello everybody {messageId}");
                i = 0;
                while (!shellCmdCommand.ReadLine().Contains(messageId) && i < 11) i++;
                Assert.True(i < 10, "Message must be echoed within 10 messages");
            }

            using (var shellCmdCommand = new ShellCmdCommand("dotnet", @"ChatConsoleClient.dll ""127.0.0.1"""))
            {
                const string testUser1 = "TestUser2";
                shellCmdCommand.WriteLine(testUser1);
                int i = 0;
                while (!shellCmdCommand.ReadLine().Contains(messageId) && i < 51) i++;
                Assert.True(i < 50, "30 Messages must be echoed on login");
            }
        }

        /// <summary>
        /// UI e2e тесты работающие поверх того же docker-compose
        /// </summary>
        [Fact]
        public void CheckWebClientFake()
        {
            //silenium
            new WebClient().DownloadString("http://localhost:24525");
        }
    }
}