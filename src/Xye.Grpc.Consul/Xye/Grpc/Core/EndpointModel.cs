
namespace Xye.Grpc.Core
{
    public class EndpointModel
    {
        public bool IsDisconvry { get; set; }

        public string Host { get; set; }

        public int Port { get; set; }

        public override string ToString()
        {
            return $"{Host}:{Port}";
        }
    }
}
