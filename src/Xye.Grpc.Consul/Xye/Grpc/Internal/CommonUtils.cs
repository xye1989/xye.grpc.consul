#if !NETCORE
using Xye.Grpc.Core;
#endif
using System;

namespace Xye.Grpc.Internal
{
    internal static class CommonUtilsHelper
    {
        public static readonly CommonUtils _ = new CommonUtils();

#if NETCORE
        /// <summary>
        /// 
        /// </summary>
        /// <param name="loggerWriter"></param>
        public static void Configure(Action<string, Exception> loggerWriter)
        {
            FormatLogger(loggerWriter);
        }
#else
        /// <summary>
        /// 
        /// </summary>
        /// <param name="loggerWriter"></param>
        /// <param name="endpointStrategy"></param>
        /// <param name="serviceDiscovery"></param>
        public static void Configure(Action<string, Exception> loggerWriter
            , IEndpointStrategy endpointStrategy
            , IServiceDiscovery serviceDiscovery)
        {
            FormatLogger(loggerWriter);
            _.EndpointStrategy = endpointStrategy;
            _.ServiceDiscovery = serviceDiscovery;
        }

        public static void Configure(Action<string, Exception> loggerWriter
           , IServiceRegister serviceRegister)
        {
            FormatLogger(loggerWriter);
            _.ServiceRegister = serviceRegister;
        }

        /// <summary>
        /// 配置路径
        /// </summary>
        /// <param name="consulServerConfigPath"></param>
        /// <param name="grpcServiceConfigPath"></param>
        /// <param name="grpcServerRegisterPath"></param>
        public static void Configure(string consulServerConfigPath
            , string grpcServiceConfigPath = ""
            , string grpcServerRegisterPath = "")
        {
            if (string.IsNullOrEmpty(_.ConsulServerConfigPath))
            {
                _.ConsulServerConfigPath = consulServerConfigPath;
            }
            if (string.IsNullOrEmpty(_.GrpcServiceConfigPath))
            {
                _.GrpcServiceConfigPath = grpcServiceConfigPath;
            }
            if(string.IsNullOrEmpty(_.GrpcServerRegisterPath))
            {
                _.GrpcServerRegisterPath = grpcServerRegisterPath;
            }
        }
#endif
        /// <summary>
        /// 标准化日志记录器
        /// </summary>
        /// <param name="loggerWriter"></param>
        private static void FormatLogger(Action<string, Exception> loggerWriter)
        {
            if (_.LoggerWriter == null)
            {
                if (loggerWriter == null)
                {
                    loggerWriter = (msg, ex) => { };
                }
                _.LoggerWriter = loggerWriter;
            }
        }
    }

    internal class CommonUtils
    {
        public Action<string, Exception> LoggerWriter { get; set; }

#if !NETCORE
        /// <summary>
        /// 策略工厂
        /// </summary>
        public IEndpointStrategy EndpointStrategy { get; set; }

        /// <summary>
        /// 服务发现
        /// </summary>
        public IServiceDiscovery ServiceDiscovery { get; set; }

        /// <summary>
        /// 服务注册
        /// </summary>
        public IServiceRegister ServiceRegister { get; set; }

        /// <summary>
        /// consul服务配置路径
        /// </summary>
        public string ConsulServerConfigPath { get; set; }

        /// <summary>
        /// grpc服务配置路径
        /// </summary>
        public string GrpcServiceConfigPath { get; set; }

        /// <summary>
        /// grpc服务注册路径
        /// </summary>
        public string GrpcServerRegisterPath { get; set; }
#endif
    }
}