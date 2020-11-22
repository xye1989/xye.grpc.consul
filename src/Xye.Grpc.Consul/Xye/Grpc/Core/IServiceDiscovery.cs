using System.Collections.Generic;

namespace Xye.Grpc.Core
{
    public interface IServiceDiscovery
    {
        void Blacklist(string target);

        void CleanTarget();

        List<EndpointModel> FindServiceEndpoint(string serviceName, List<EndpointModel> localHealthTarget);

        EndpointModel GetServiceEndpoint(string name);

        void SaveFindServiceEndpoint(string name, List<EndpointModel> endpoints);

        List<EndpointModel> SaveServiceEndpoint(string name);
    }
}