using Xye.Grpc.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Xye.Grpc.Core
{
    /// <summary>
    /// 服务发现
    /// </summary>
    public abstract class ServiceDiscovery : IServiceDiscovery
    {
        private readonly ConcurrentDictionary<string, DateTime> _blacklist
            = new ConcurrentDictionary<string, DateTime>();
        private static ConcurrentDictionary<string, List<EndpointModel>> _targetlist
            = new ConcurrentDictionary<string, List<EndpointModel>>(); //本地目标列表

        public abstract List<EndpointModel> FindServiceEndpoint(string serviceName, List<EndpointModel> localHealthTarget);

        /// <summary>
        /// 查找服务终端
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public EndpointModel GetServiceEndpoint(string name)
        {
            List<EndpointModel> targets;
            if (!_targetlist.TryGetValue(name, out targets))
            {
                return null;
            }
            var rnd = new Random();
            var now = DateTime.UtcNow;
            while (0 < targets.Count)
            {
                var choice = rnd.Next(targets.Count);
                var target = targets[choice];
                var endpoint = target.ToString();
                DateTime lastFailure;
                if (_blacklist.TryGetValue(endpoint, out lastFailure))
                {
                    if (now - lastFailure < GlobalConfig.BlacklistPeriod)
                    {
                        targets.RemoveAt(choice);
                        continue;
                    } //是否在黑名单有效期内

                    _blacklist.TryRemove(endpoint, out lastFailure); //移除黑名单
                }
                return target;
            }
            return null;
        }

        /// <summary>
        /// 远程查找服务终端
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        public List<EndpointModel> SaveServiceEndpoint(string serviceName)
        {
            List<EndpointModel> healthTargetLocal;
            _targetlist.TryGetValue(serviceName, out healthTargetLocal); //先查询本地有的，以防止consul挂了，则以本地为主
            List<EndpointModel> healthTarget = FindServiceEndpoint(serviceName, healthTargetLocal);
            SaveFindServiceEndpoint(serviceName, healthTarget); //更新到本地
            return healthTarget;
        }

        /// <summary>
        /// 保存或去除发现的服务终端地址
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="endpoints">服务终端</param>
        public void SaveFindServiceEndpoint(string serviceName, List<EndpointModel> endpoints)
        {
            if (endpoints != null && endpoints.Any())
            {
                _targetlist.AddOrUpdate(serviceName, endpoints, (n, e) =>
                {
                    return endpoints;
                });
            } //存在则更新
            else
            {
                List<EndpointModel> targets;
                if (_targetlist.ContainsKey(serviceName))
                {
                    _targetlist.TryRemove(serviceName, out targets);
                }
            } //不存在则去除
        }

        /// <summary>
        /// 清理目标终端
        /// </summary>
        public void CleanTarget()
        {
            if (!_targetlist.Any())
            {
                return;
            }
            _targetlist = new ConcurrentDictionary<string, List<EndpointModel>>();
        }

        /// <summary>
        /// 黑名单
        /// </summary>
        /// <param name="target"></param>
        public void Blacklist(string target)
        {
            var now = DateTime.UtcNow;
            _blacklist.AddOrUpdate(target, k => now, (k, old) => now);
        }
    }
}