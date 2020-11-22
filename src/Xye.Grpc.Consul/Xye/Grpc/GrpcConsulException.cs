using System;

namespace Xye.Grpc
{
    public class GrpcConsulException : Exception
    {
        public GrpcConsulException(string message)
            : base(message, null)
        {
        }
    }
}
