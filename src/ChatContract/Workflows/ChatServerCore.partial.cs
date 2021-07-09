using ChatContract.Connections;

namespace ChatContract.Workflows
{
    public sealed partial class ChatServerCore
    {
        /// <summary>
        /// Represents logged in clients
        /// </summary>
        private class ChatConnectedClient
        {
            public readonly string UserName;
            public readonly IConnection Connection;

            public ChatConnectedClient(string userName, IConnection connection)
            {
                UserName = userName;
                Connection = connection;
            }
        }
    }
}