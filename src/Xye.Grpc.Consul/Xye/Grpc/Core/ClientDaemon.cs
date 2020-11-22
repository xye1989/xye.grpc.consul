using Xye.Grpc.Configuration;
using Xye.Grpc.Consul;
using Xye.Grpc.Internal;
#if NETCORE
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Xye.Grpc.Core
{
    /// <summary>
    /// 客服端守护
    /// </summary>
    internal class ClientDaemon
    {
        private readonly IServiceDiscovery _serviceDiscovery;
        private readonly IEndpointStrategy _endpointStrategy;
#if NETCORE
        private readonly IServiceProvider _serviceProvider;

        public ClientDaemon(
            IServiceProvider serviceProvider
            , IServiceDiscovery serviceDiscovery
            , IEndpointStrategy endpointStrategy)
        {
            _serviceProvider = serviceProvider;
            _serviceDiscovery = serviceDiscovery;
            _endpointStrategy = endpointStrategy;
        }
#else
        public ClientDaemon()
        {
            _serviceDiscovery = CommonUtilsHelper._.ServiceDiscovery;
            _endpointStrategy = CommonUtilsHelper._.EndpointStrategy;
        }
#endif

        /// <summary>
        /// 开启
        /// </summary>
        public void Start()
        {
            SingleRun();
            ThreadRun();
        }

        private int GetSleepTime()
        {
            ConsulServerConfig consulServiceConfig = null;
#if NETCORE
            consulServiceConfig = _serviceProvider.GetRequiredService<IOptionsMonitor<ConsulServerConfig>>().CurrentValue;
#else
            consulServiceConfig = ConsulServerConfigWrapper.ConsulServiceConfig;
#endif
            int daemonSleep = 1;
            if (consulServiceConfig != null)
            {
                daemonSleep = consulServiceConfig.DaemonSleep;
            }
            if (daemonSleep < 1)
            {
                daemonSleep = 1;
            }
            daemonSleep = daemonSleep * 1000; //转换成毫秒
            return daemonSleep;
        }

        /// <summary>
        /// 线程运行
        /// </summary>
        public void ThreadRun()
        {
            var daemonSleep = GetSleepTime();
            Task.Run(() =>
            {
                Thread.Sleep(daemonSleep);
                DoRun();
            }).ContinueWith(t =>
            {
                if (t.IsCompleted)
                {
                    ThreadRun();
                }
            });
        }

        /// <summary>
        /// 单步执行
        /// </summary>
        public void SingleRun()
        {
            DoRun();
        }

        /// <summary>
        /// 核心执行
        /// </summary>
        private void DoRun()
        {
            try
            {
                //这里做兼容，从不同地址获取配置信息
                List<GrpcService> listServ = null;
#if NETCORE
                IOptionsMonitor<List<GrpcService>> grpcServices = _serviceProvider.GetRequiredService<IOptionsMonitor<List<GrpcService>>>();
                listServ = grpcServices.CurrentValue;
#else
                listServ = GrpcServiceWrapper.Services;
#endif
                if (listServ == null || !listServ.Any())
                {
                    _serviceDiscovery.CleanTarget();
                    _endpointStrategy.ShutDownAll();
                    return;
                } //如果没有配置，则清理所有

                var haveTargetKeys = _endpointStrategy.Targets;
                var serviceNames = listServ
                    .OrderByDescending(s => s.IsDiscovery)
                    .Select(s => s.ServiceName)
                    .Where(s => !string.IsNullOrEmpty(s))
                    .Distinct(); //服务发现优先
                
                foreach (var serviceName in serviceNames)
                {
                    List<EndpointModel> healthTarget = null;
                    var service = listServ.Where(s => s.ServiceName == serviceName);
                    var discoveryServ = service.Where(s => s.IsDiscovery).FirstOrDefault();
                    if (discoveryServ != null)
                    {
                        healthTarget = _serviceDiscovery.SaveServiceEndpoint(discoveryServ.ServiceName);
                    } //优先处理 服务发现
                    else
                    {
                        healthTarget = service
                                            .Where(s => !s.IsDiscovery)
                                            .Select(s => new EndpointModel
                                            {
                                                IsDisconvry = false,
                                                Host = s.Host,
                                                Port = s.Port
                                            }).ToList();
                        _serviceDiscovery.SaveFindServiceEndpoint(serviceName, healthTarget);
                    } //如果本机服务存在
                    if (healthTarget == null)
                    {
                        healthTarget = new List<EndpointModel>();
                    }
                    //处理健康地址
                    var oldTargetKeys = haveTargetKeys.Where(t => _endpointStrategy.IsExistService(t, serviceName)).ToList();
                    var healthTargetKeys = healthTarget.Select(h => _endpointStrategy.GetCallInvokerKey(serviceName, h.ToString())).ToList();

                    //求出不存在的
                    var unHealthTargetKey = oldTargetKeys.Except(healthTargetKeys);

                    _endpointStrategy.Revoke(unHealthTargetKey.ToArray()); //不健康的剔除
                    _endpointStrategy.Create(healthTargetKeys.ToArray()); //健康的创建
                }

                //释放
                haveTargetKeys.Clear();
            }
            catch (Exception ex)
            {
                CommonUtilsHelper._.LoggerWriter.Invoke(string.Format("[客户端守护线程]发生异常 {0}", ex.Message), ex);
            }
        }
    }
}
