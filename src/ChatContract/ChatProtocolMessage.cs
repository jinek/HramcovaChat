using System;
using System.Runtime.Serialization;
using ChatHelpers;

namespace ChatContract
{
    [DataContract]
    public class ChatProtocolMessage
    {
        public static readonly ThreadStaticParameter<string?> UserNameToSet = new();
        public ChatProtocolMessage(string? message=null)
        {
            Message = message;
        }
        
        [OnDeserialized]
        // ReSharper disable once UnusedMember.Local Used by deserializer
        // ReSharper disable once UnusedParameter.Local ThreadStaticParameter used instead, but required by Serializer
        private void OnDeserialized(StreamingContext _)
        {
            string? userNameToSet = UserNameToSet.CurrentValue;
            if (userNameToSet == null) return;
            if (Login != null)
                throw new NotImplementedException("Уже установлено+unit test");
            Login = userNameToSet!;
        }

        [DataMember]
        public string? Message { get; private set; }

        [IgnoreDataMember]
        public bool HasMessage => Message != null;

        [DataMember]
        public string? Login { get; private set; }
    }

    [DataContract]
    public class LoginMessage
    {
        public LoginMessage(string login, bool retrieve30Messages)
        {
            Login = login;
            Retrieve30Messages = retrieve30Messages;
        }

        [DataMember]
        public string Login { get; private set; }
        
        [DataMember]
        public bool Retrieve30Messages { get; private set; }
    }

    public static class ChatProtocol
    {
        public static readonly TimeSpan PingTimeout = TimeSpan.FromMilliseconds(30000);
        public static readonly TimeSpan SendReceiveTimeout = TimeSpan.FromMilliseconds(10000);
        public const int SocketPortNumber=24523;
        public const int HttpPortNumber=24524;
        public const int BufferSize = 1024;
    }
}