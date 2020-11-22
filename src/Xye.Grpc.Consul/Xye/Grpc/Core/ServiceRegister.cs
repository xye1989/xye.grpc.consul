using Xye.Grpc.Configuration;
using System;
using System.Net;

namespace Xye.Grpc.Core
{
    public abstract class ServiceRegister : IServiceRegister
    {
        public abstract void UnregisterService(string serviceId);

        public abstract Entry RegisterService(GrpcServerRegister grpcServerRegister);

        public sealed class Entry : IDisposable
        {
            private readonly ServiceRegister _serviceDiscovery;

            internal Entry(ServiceRegister serviceDiscovery
                , string serviceId
                , string serviceName
                , string host
                , int port)
            {
                ServiceName = serviceName;
                Port = port;
                ServiceId = serviceId;
                _serviceDiscovery = serviceDiscovery;
            }

            /// <summary>
            /// 服务Id
            /// </summary>
            public string ServiceId { get; }

            /// <summary>
            /// 服务名称
            /// </summary>
            public string ServiceName { get; }

            /// <summary>
            /// IP
            /// </summary>
            public int Host { get; set; }

            /// <summary>
            /// 端口
            /// </summary>
            public int Port { get; }

            public void Dispose()
            {
                _serviceDiscovery.UnregisterService(ServiceId);
            }
        }
    }
}
