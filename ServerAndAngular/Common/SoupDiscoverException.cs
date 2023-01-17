using System;

namespace SoupDiscover.Common
{
    [Serializable]
    public class SoupDiscoverException : Exception
    {
        public SoupDiscoverException()
        {
        }

        public SoupDiscoverException(string message)
            : base(message)
        {
        }
        public SoupDiscoverException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public static void ThrowIfNull(object obj, string message)
        {
            if(obj == null)
            {
                throw new SoupDiscoverException(message);
            }
        }
        public static void ThrowIfNullOrEmpty(string obj, string message)
        {
            if (string.IsNullOrEmpty(obj))
            {
                throw new SoupDiscoverException(message);
            }
        }
    }
}
