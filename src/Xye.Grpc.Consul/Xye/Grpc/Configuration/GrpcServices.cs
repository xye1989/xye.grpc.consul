#if !NETCORE
using Xye.Grpc.Internal;
using System.Collections.Generic;
#endif

namespace Xye.Grpc.Configuration
{
#if !NETCORE
    public static class GrpcServiceWrapper
    {
        /// <summary>
        /// 集合配置实例
        /// </summary>
        private static BaseInfoConfigs<GrpcServiceRoot> config = null;

        private static object locker = new object();

        /// <summary>
        /// Grpc服务
        /// </summary>
        public static List<GrpcService> Services
        {
            get
            {
                if (config == null)
                {
                    lock (locker)
                    {
                        if (config == null)
                        {
                            config = new BaseInfoConfigs<GrpcServiceRoot>(CommonUtilsHelper._.GrpcServiceConfigPath);
                        }
                    }
                }
                return config.GetConfig()?.GrpcServices;
            }
        }
    }

    public class GrpcServiceRoot
    {
        /// <summary>
        /// Grpc服务
        /// </summary>
        public List<GrpcService> GrpcServices { get; set; }
    }
#endif

    public class GrpcService
    {
        /// <summary>
        /// 服务名称 
        /// namespace.servicename
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 服务名 指定为服务代码（小写）
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// 端口
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 宿主IP
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// 是否使用服务发现
        /// </summary>
        public bool IsDiscovery { get; set; }

        /// <summary>
        /// 最大重试次数
        /// </summary>
        public int MaxRetry { get; set; }
    }
}
