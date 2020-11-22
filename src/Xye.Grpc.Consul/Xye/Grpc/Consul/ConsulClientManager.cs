using Consul;
using Xye.Grpc.Configuration;
#if NETCORE
using Microsoft.Extensions.Options;
#endif
using System;

namespace Xye.Grpc.Consul
{
    /// <summary>
    /// Consul客户端管理
    /// </summary>
    public class ClientManager
    {
        private readonly ConsulServerConfig _consulServerConfig;
        private readonly static object _locker = new object();
        private ConsulClient _consulClient = null;

#if NETCORE
        public ClientManager(IOptions<ConsulServerConfig> options)
        {
            _consulServerConfig = options.Value;
        }
#else
        public ClientManager()
        {
            _consulServerConfig = ConsulServerConfigWrapper.ConsulServiceConfig;
        }
#endif

        /// <summary>
        /// 
        /// </summary>
        public ConsulClient GetClient
        {
            get
            {
                if (_consulClient == null && _consulServerConfig != null)
                {
                    lock (_locker)
                    {
                        if (_consulClient == null && _consulServerConfig != null)
                        {
                            _consulClient = new ConsulClient(cc =>
                            {
                                cc.Address = new Uri($"http://{_consulServerConfig.Host}:{_consulServerConfig.Port}");
                            });
                        }
                    }
                }
                return _consulClient;
            }
        }
    }
}