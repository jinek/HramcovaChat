using System;

namespace ChatContract.Connections
{
    public class ConnectivityException : Exception
    {
        /// <summary>
        /// Message of inner exception will be shown to the user
        /// </summary>
        /// <param name="innerException"></param>
        public ConnectivityException(Exception innerException) : base(innerException.Message, innerException)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message">Will be shown to the user</param>
        public ConnectivityException(string message) : base(message)
        {
        }
    }
}