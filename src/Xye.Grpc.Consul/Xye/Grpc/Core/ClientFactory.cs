using Grpc.Core;
using System;
using System.Collections.Generic;
using Xye.Grpc.Configuration;
using System.Linq;
#if NETCORE
using Microsoft.Extensions.Options;
#else
using Xye.Grpc.Internal;
#endif

namespace Xye.Grpc.Core
{
    /// <summary>
    /// 客户端工厂
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ClientFactory<T> : IClientFactory<T> 
        where T : ClientBase<T>
    {
        private readonly IEndpointStrategy _endpointStrategy;
        private readonly GrpcService _grpcService;
        private string _name;

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="Services"></param>
        /// <param name="endpointStrategy"></param>
        public ClientFactory(
#if NETCORE
            IOptionsMonitor<List<GrpcService>> Services
             , IEndpointStrategy endpointStrategy
#endif
           )
        {
            _name = $"{typeof(T).DeclaringType.FullName}";
            List<GrpcService> services;
#if NETCORE
            services = Services.CurrentValue;
            _endpointStrategy = endpointStrategy;
#else
            services = GrpcServiceWrapper.Services;
            _endpointStrategy = CommonUtilsHelper._.EndpointStrategy;
#endif
            _grpcService = services.FirstOrDefault(s => s.Name == _name);
        }

        /// <summary>
        /// 获取实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Get()
        {
            if (_grpcService == null)
            {
                throw new GrpcConsulException($"未发现GrpcService:Name={_name}配置");
            } //每次使用时再做判断
            var clientCallInvoker = new ClientCallInvoker(_endpointStrategy
                , _grpcService.ServiceName
                , _grpcService.MaxRetry);
            var client = (T)Activator.CreateInstance(typeof(T), clientCallInvoker);
            return client;
        }
    }
}
