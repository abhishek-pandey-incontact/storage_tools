using System;
using System.Runtime.Serialization;

namespace StorageGlacierRetrieval.Controller
{
    [Serializable]
    internal class AmazonS3Exception : Exception
    {
        public AmazonS3Exception()
        {
        }

        public AmazonS3Exception(string message) : base(message)
        {
        }

        public AmazonS3Exception(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected AmazonS3Exception(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}