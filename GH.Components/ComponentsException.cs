using System;

namespace GH
{
    public class ComponentsException : Exception
    {
        public ComponentsException() { }

        public ComponentsException(string message)
            : base(message)
        {
        }

        public ComponentsException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
