using Grpc.Core;
using Grpc.Core.Interceptors;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Xye.Grpc.Internal;
#if NETCORE
using Microsoft.Extensions.DependencyInjection;
#endif

namespace Xye.Grpc.Core
{
    /// <summary>
    /// 策略是全局的
    /// </summary>
    public class StickyEndpointStrategy : IEndpointStrategy
    {
        private const string SEPARATOR = "###";
        private readonly IServiceDiscovery _serviceDiscovery;
        private readonly ConcurrentDictionary<string, Channel> _channels
            = new ConcurrentDictionary<string, Channel>(); // key = target = ip:port
        private readonly ConcurrentDictionary<string, CallInvoker> _invokers
            = new ConcurrentDictionary<string, CallInvoker>(); //key = servicename_target = servicename_ip:port
        private List<Interceptor> _interceptors;
        /// <summary>
        /// 拦截器
        /// </summary>
        public List<Interceptor> Interceptors
        {
            get
            {
                return _interceptors;
            }
            set
            {
                _interceptors = value;
            }
        }

        /// <summary>
        /// 目标终端
        /// </summary>
        public List<string> Targets
        {
            get
            {
                return _invokers.Select(i => i.Key).ToList();
            }
        }

        public StickyEndpointStrategy(IServiceDiscovery serviceDiscovery)
        {
            _serviceDiscovery = serviceDiscovery;
        }

        /// <summary>
        /// 是否存在服务
        /// </summary>
        /// <param name="key"></param>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        public bool IsExistService(string key, string serviceName)
        {
            return key.StartsWith($"{serviceName}{SEPARATOR}");
        }

        /// <summary>
        /// 获取
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        public CallInvoker Get(string serviceName, out EndpointModel endpoint)
        {
            endpoint = _serviceDiscovery.GetServiceEndpoint(serviceName);
            if (endpoint == null)
            {
                CommonUtilsHelper._.LoggerWriter($"Can't find service: {serviceName}, target is empty", null);
                return null;
            } //如果没有目标地址则直接返回
            var target = endpoint.ToString();
            if (string.IsNullOrEmpty(target))
            {
                CommonUtilsHelper._.LoggerWriter($"Can't find service: {serviceName}, target is empty", null);
                return null;
            } //如果没有目标地址则直接返回
            CallInvoker callInvoker;
            var key = GetCallInvokerKey(serviceName, target);
            if (!_invokers.TryGetValue(key, out callInvoker))
            {
                CommonUtilsHelper._.LoggerWriter($"Can't find service: {serviceName}, no key", null);
                return null;
            } //读取缓存
            return callInvoker;
        }

        /// <summary>
        /// 创建
        /// </summary>
        /// <param name="keys"></param>
        public void Create(params string[] keys)
        {
            if (keys == null || keys.Length == 0)
            {
                return;
            }
            foreach (var key in keys)
            {
                CallInvoker callInvoker;
                string target = GetServiceAndTarget(key)[1];
                if (_invokers.TryGetValue(key, out callInvoker))
                {
                    //判断渠道是否关闭
                    if (!_channels.ContainsKey(target))
                    {
                        _invokers.TryUpdate(key, CreateCallInvoker(CreateChannel(target)), callInvoker);
                    } //这种情况很少发生 
                    continue;
                } //确保存在则继续
                callInvoker = CreateCallInvoker(CreateChannel(target));
                _invokers.TryAdd(key, callInvoker);
            }
        }

        /// <summary>
        /// 撤销
        /// </summary>
        /// <param name="key"></param>
        public void Revoke(params string[] keys)
        {
            if (keys == null || keys.Length == 0)
            {
                return;
            }
            foreach (var key in keys)
            {
                CallInvoker serverCallInvoker;
                if (!_invokers.TryGetValue(key, out serverCallInvoker))
                {
                    continue;
                } //确保不存在则继续
                _invokers.TryRemove(key, out serverCallInvoker);
                ShutDownChannel(key);
            }
        }

        /// <summary>
        /// 关闭所有
        /// </summary>
        public void ShutDownAll()
        {
            foreach (var invoker in _invokers)
            {
                CallInvoker failedCallInvoker;
                _invokers.TryRemove(invoker.Key, out failedCallInvoker);
                ShutDownChannel(invoker.Key);
            }
        }

        /// <summary>
        /// 根据服务和终端获得Key
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public string GetCallInvokerKey(string serviceName, string target)
        {
            return $"{serviceName}{SEPARATOR}{target}";
        }

        /// <summary>
        /// 添加黑名单
        /// </summary>
        /// <param name="target"></param>
        public void AddBlack(string target)
        {
            _serviceDiscovery.Blacklist(target);
        }

        /// <summary>
        /// 创建Call
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        private CallInvoker CreateCallInvoker(Channel channel)
        {
            var callInvoker = new ServerCallInvoker(channel);
            if (_interceptors != null && _interceptors.Any())
            {
                return callInvoker.Intercept(_interceptors.ToArray()); //加入拦截器
            }
            return callInvoker;
        }

        /// <summary>
        /// 创建渠道
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        private Channel CreateChannel(string target)
        {
            Channel channel;
            if (!_channels.TryGetValue(target, out channel))
            {
                channel = new Channel(target, ChannelCredentials.Insecure);
                _channels.TryAdd(target, channel);
            }
            //如果渠道被关闭后会报：Safe handle has been closed
            return channel;
        }

        /// <summary>
        /// 关闭渠道
        /// </summary>
        /// <param name="serverCallInvoker"></param>
        private void ShutDownChannel(string key)
        {
            string target = GetServiceAndTarget(key)[1];
            Channel channel;
            if (_channels.TryGetValue(target, out channel)) //&& ReferenceEquals(channel, failedChannel)
            {
                _channels.TryRemove(target, out channel);
            }
            channel.ShutdownAsync();
        }

        /// <summary>
        /// 获取服务和终端
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private string[] GetServiceAndTarget(string key)
        {
            string[] sort = key.Split(new string[] { SEPARATOR }, StringSplitOptions.RemoveEmptyEntries);
            if (sort.Length > 1)
            {
                return sort;
            }
            return new string[] { "", "" };
        }
    }
}
