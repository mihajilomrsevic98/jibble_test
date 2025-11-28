namespace Jibble
{
    public class ODataServiceException : Exception
    {
        public ODataServiceException(string message)
            : base(message)
        {
        }

        public ODataServiceException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
