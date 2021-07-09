using System;
using System.Runtime.Serialization;
using ChatHelpers;

namespace ChatContract.Messages
{
    [DataContract]
    public class ChatProtocolMessage
    {
        public static readonly ThreadStaticParameter<string?> UserNameToSet = new();

        public ChatProtocolMessage(string? message = null)
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
                throw new InvalidOperationException(
                    "Зачем-то пытаемся установить Login у уже установленного сообщения");
            Login = userNameToSet!;
        }

        [DataMember] public string? Message { get; private set; }

        /// <summary>
        /// Если false, то это ping сообщение
        /// </summary>
        [IgnoreDataMember] public bool HasMessage => Message != null;

        [DataMember] public string? Login { get; private set; }
    }
}