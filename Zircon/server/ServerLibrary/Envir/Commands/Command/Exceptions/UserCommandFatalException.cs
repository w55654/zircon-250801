using System;

namespace Server.Envir.Commands.Exceptions
{
    internal class UserCommandFatalException : Exception
    {
        public UserCommandFatalException(string message) : base(message)
        {
        }
    }
}