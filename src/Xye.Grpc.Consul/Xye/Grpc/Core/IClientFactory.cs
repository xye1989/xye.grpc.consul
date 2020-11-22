using Grpc.Core;

namespace Xye.Grpc.Core
{
    public interface IClientFactory<T> where T : ClientBase<T>
    {
        T Get();
    }
}