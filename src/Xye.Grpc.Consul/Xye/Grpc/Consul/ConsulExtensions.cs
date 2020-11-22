using Grpc.Core.Interceptors;
#if NETCORE
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
#endif
using System;
using System.Collections.Generic;
using Xye.Grpc.Core;
using Xye.Grpc.Configuration;
using Xye.Grpc.Internal;

namespace Xye.Grpc.Consul
{
    public static class ConsulDI
    {
#if NETCORE
        public static IServiceCollection AddConsulRegister(this IServiceCollection services, IConfiguration configuration)
        {
            services
              .AddSingleton<ServiceRegisterFactory>()
              .Configure<ConsulServerConfig>(configuration.GetSection(nameof(ConsulServerConfig)))
              .Configure<GrpcServerRegister>(configuration.GetSection(nameof(GrpcServerRegister)))
              ;

            services.TryAdd(ServiceDescriptor.Singleton(typeof(ClientManager), typeof(ClientManager)));
            services.TryAdd(ServiceDescriptor.Singleton(typeof(IServiceRegister), typeof(ConsulServiceRegister)));

            return services;
        }

        public static IServiceCollection AddConsulDiscovery(this IServiceCollection services, IConfiguration configuration)
        {
            services
              .AddSingleton<ClientDaemon>() //客户端守护程序
              .Configure<ConsulServerConfig>(configuration.GetSection(nameof(ConsulServerConfig)))
              .Configure<List<GrpcService>>(configuration.GetSection("GrpcServices"))
              ;

            services.TryAdd(ServiceDescriptor.Singleton(typeof(ClientManager), typeof(ClientManager)));
            services.TryAdd(ServiceDescriptor.Singleton(typeof(IServiceDiscovery), typeof(ConsulServiceDiscovery)));
            services.TryAdd(ServiceDescriptor.Singleton(typeof(IEndpointStrategy), typeof(StickyEndpointStrategy)));
            services.TryAdd(ServiceDescriptor.Transient(typeof(IClientFactory<>), typeof(ClientFactory<>)));
            return services;
        }

        public static void UseConsulDiscovery(IServiceProvider serviceProvider, Action<string, Exception> loggerWriter = null)
        {
            CommonUtilsHelper.Configure(loggerWriter);

            var consulCfg = serviceProvider.GetService<IOptions<ConsulServerConfig>>();
            if (consulCfg != null && consulCfg.Value != null && consulCfg.Value.BlackTime > 0)
            {
                GlobalConfig.BlacklistPeriod = TimeSpan.FromSeconds(consulCfg.Value.BlackTime);
            }  //更新黑名单间隔时间

            var interceptors = serviceProvider.GetServices<Interceptor>()?.ToList();
            serviceProvider.GetRequiredService<IEndpointStrategy>().Interceptors = interceptors;
            serviceProvider.GetRequiredService<ClientDaemon>().Start();
        }
#else
        public static void AddConsulRegister(
            Action<ConsulOption> actAonsulOption
             , Action<string, Exception> loggerWriter = null
             , Func<IServiceRegister> registerFunc = null)
        {
            AddConsulOption(actAonsulOption);

            if (registerFunc == null)
            {
                registerFunc = () => new ConsulServiceRegister(new ClientManager());
            }

            CommonUtilsHelper.Configure(loggerWriter, registerFunc());
        }

        public static void AddConsulDiscovery(
            Action<ConsulOption> actAonsulOption
            , Action<List<Interceptor>> interceptorAct = null
            , Action<string, Exception> loggerWriter = null
            , Func<IEndpointStrategy> strategyFunc = null)
        {
            AddConsulOption(actAonsulOption);

            var serviceDiscovery = new ConsulServiceDiscovery(new ClientManager());
            if (strategyFunc == null)
            {
                strategyFunc = ()=> new StickyEndpointStrategy(serviceDiscovery);
            }
            var strategy = strategyFunc();
            List<Interceptor> interceptors = new List<Interceptor>();
            interceptorAct?.Invoke(interceptors);
            strategy.Interceptors = interceptors;  //加入拦截器

            CommonUtilsHelper.Configure(loggerWriter, strategy, serviceDiscovery);

            var blackTime = ConsulServerConfigWrapper.ConsulServiceConfig.BlackTime;
            if (blackTime > 0)
            {
                GlobalConfig.BlacklistPeriod = TimeSpan.FromSeconds(blackTime);
            } //更新黑名单间隔时间

            new ClientDaemon().Start();
        }

        private static void AddConsulOption(Action<ConsulOption> actAonsulOption)
        {
            ConsulOption consulOption = new ConsulOption();
            if (actAonsulOption == null)
            {
                throw new ArgumentException("请配置ConsulOption");
            }
            actAonsulOption(consulOption);
            CommonUtilsHelper.Configure(consulOption.ConsulServerConfigPath
                , consulOption.GrpcServiceConfigPath
                , consulOption.GrpcServerRegisterPath);
        }
#endif
    }
}
