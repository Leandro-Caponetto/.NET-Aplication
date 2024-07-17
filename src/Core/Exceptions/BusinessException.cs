using System;

namespace Core.Exceptions
{
    public class BusinessException : Exception
    {
        public BusinessException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public BusinessException(string message)
            : base(message)
        {
        }
    }
}
