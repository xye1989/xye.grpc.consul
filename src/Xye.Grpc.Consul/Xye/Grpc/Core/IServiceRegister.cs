using Xye.Grpc.Configuration;

namespace Xye.Grpc.Core
{
    public interface IServiceRegister
    {
        ServiceRegister.Entry RegisterService(GrpcServerRegister grpcServerRegister);
    }
}