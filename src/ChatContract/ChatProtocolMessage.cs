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
        public LoginMessage(string login)
        {
            Login = login;
        }

        [DataMember]
        public string Login { get; private set; }
        
        [DataMember]
        public bool Retrieve30Messages { get; private set; }
    }

    public static class ChatProtocol
    {
        public static readonly TimeSpan PingTimeout = TimeSpan.FromMinutes(10);//todo: small timeouts
        public static readonly TimeSpan SendReceiveTimeout = TimeSpan.FromMinutes(10);
        public const int SocketPortNumber=24523;
        public const int BufferSize = 1024;
    }
}