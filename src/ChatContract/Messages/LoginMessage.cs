using System.Runtime.Serialization;

namespace ChatContract.Messages
{
    [DataContract]
    public class LoginMessage
    {
        public LoginMessage(string login, bool retrieve30Messages)
        {
            Login = login;
            Retrieve30Messages = retrieve30Messages;
        }

        [DataMember] public string Login { get; private set; }

        [DataMember] public bool Retrieve30Messages { get; private set; }
    }
}