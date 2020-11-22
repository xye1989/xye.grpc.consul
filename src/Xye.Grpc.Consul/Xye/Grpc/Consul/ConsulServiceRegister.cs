using Consul;
using Xye.Grpc.Configuration;
using Xye.Grpc.Core;
using Xye.Grpc.Internal;
using System;
using System.Collections.Generic;
using System.Net;

namespace Xye.Grpc.Consul
{
    /// <summary>
    /// consul服务注册
    /// </summary>
    public class ConsulServiceRegister : ServiceRegister
    {
        private readonly ClientManager _clientManager;

        public ConsulServiceRegister(ClientManager clientManager)
        {
            _clientManager = clientManager;
        }

        /// <summary>
        /// 注册服务
        /// </summary>
        /// <param name="GrpcServerRegister">服务注册</param>
        /// <returns></returns>
        public override Entry RegisterService(GrpcServerRegister grpcServerRegister)
        {
            try
            {
                if (string.IsNullOrEmpty(grpcServerRegister.Host))
                {
                    grpcServerRegister.Host = IPUtils.GetAddressIP();
                }
                var serviceId = $"{grpcServerRegister.ServiceName}_{grpcServerRegister.Host}:{grpcServerRegister.Port}";
                var acr = new AgentCheckRegistration
                {
                    TCP = $"{grpcServerRegister.Host}:{grpcServerRegister.Port}",
                    Name = serviceId,
                    ID = serviceId,
                    Interval = GlobalConfig.CheckInterval,
                    DeregisterCriticalServiceAfter = GlobalConfig.CriticalInterval
                };
                if (grpcServerRegister.Meta == null)
                {
                    grpcServerRegister.Meta = new Dictionary<string, string>();
                }
                if (!grpcServerRegister.Meta.ContainsKey("weights"))
                {
                    grpcServerRegister.Meta.Add("weights", "1");
                } //权重值，如果等于0，客户端会自动移除
                var asr = new AgentServiceRegistration
                {
                    Address = grpcServerRegister.Host,
                    ID = serviceId,
                    Name = grpcServerRegister.ServiceName,
                    Port = grpcServerRegister.Port,
                    Check = acr,
                    Tags = grpcServerRegister.Tags,
                    Meta = grpcServerRegister.Meta,
                    EnableTagOverride = grpcServerRegister.EnableTagOverride
                };
                var res = _clientManager.GetClient.Agent.ServiceRegister(asr).Result;
                if (res.StatusCode != HttpStatusCode.OK)
                {
                    throw new GrpcConsulException($"Failed to register service by {serviceId}");
                }
                return new Entry(this, serviceId, grpcServerRegister.ServiceName, grpcServerRegister.Host, grpcServerRegister.Port);
            }
            catch (Exception ex)
            {
                CommonUtilsHelper._.LoggerWriter(ex.Message, ex);
            }
            return null;
        }

        /// <summary>
        /// 反注册服务
        /// </summary>
        /// <param name="serviceId"></param>
        public override void UnregisterService(string serviceId)
        {
            var res = _clientManager.GetClient.Agent.ServiceDeregister(serviceId).Result;
        }
    }
}
