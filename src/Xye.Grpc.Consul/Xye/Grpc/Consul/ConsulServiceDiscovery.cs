using Xye.Grpc.Core;
using Xye.Grpc.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Xye.Grpc.Consul
{
    /// <summary>
    /// consul服务发现
    /// </summary>
    public class ConsulServiceDiscovery : ServiceDiscovery
    {
        private readonly ClientManager _clientManager;

        public ConsulServiceDiscovery(ClientManager clientManager)
        {
            _clientManager = clientManager;
        }

        /// <summary>
        /// 发现服务终端
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="localHealthTarget">本地已存在的健康目标地址</param>
        /// <returns></returns>
        public override List<EndpointModel> FindServiceEndpoint(string serviceName, List<EndpointModel> localHealthTarget)
        {
            try
            {
                var res = _clientManager.GetClient.Health.Service(serviceName).Result;
                if (res.StatusCode != HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Failed to query services");
                }
                var response = res.Response;
                if (response == null || !response.Any())
                {
                    return new List<EndpointModel>();
                }
                List<EndpointModel> healthTarget = new List<EndpointModel>();
                foreach (var r in response)
                {
                    if (r.Service.Meta == null || !r.Service.Meta.ContainsKey("weights") || r.Service.Meta["weights"] != "0")
                    {
                        healthTarget.Add(new EndpointModel
                        {
                            IsDisconvry = true,
                            Host = r.Service.Address,
                            Port = r.Service.Port
                        });
                    } //如果权重不是为0，则可用
                }
                return healthTarget;
            }
            catch (Exception ex)
            {
                CommonUtilsHelper._.LoggerWriter(ex.Message, ex);
            }
            return localHealthTarget; //如果获取远程异常，则已本地为主
        }
    }
}
