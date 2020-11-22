#if !NETCORE
using Xye.Grpc.Internal;
#endif
using System.Collections.Generic;

namespace Xye.Grpc.Configuration
{
#if !NETCORE
    /// <summary>
    /// 获取
    /// </summary>
    public static class GrpcServerRegisterWrapper
    {
        private static BaseInfoConfigs<GrpcServerRegisterRoot> config = null;
        private static object _locker = new object();

        /// <summary>
        /// 获取当前配置实体类集合
        /// </summary>
        /// <returns>配置实体类集合</returns>
        public static GrpcServerRegister GrpcServerRegister
        {
            get
            {
                if (config == null)
                {
                    lock (_locker)
                    {
                        if (config == null)
                        {
                            config = new BaseInfoConfigs<GrpcServerRegisterRoot>(CommonUtilsHelper._.GrpcServerRegisterPath);
                        }
                    }
                }
                return config.GetConfig()?.GrpcServerRegister;
            }
        }
    }

    public class GrpcServerRegisterRoot
    {
        public GrpcServerRegister GrpcServerRegister { get; set; }
    }
#endif

    /// <summary>
    /// Grpc注册服务
    /// </summary>
    public class GrpcServerRegister
    {
        /// <summary>
        /// 注册服务名
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// Host
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// 端口
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 是否重写tag
        /// </summary>
        public bool EnableTagOverride { get; set; }

        /// <summary>
        /// tag
        /// </summary>
        public string[] Tags { get; set; }

        /// <summary>
        /// meta
        /// </summary>
        public IDictionary<string, string> Meta { get; set; }
    }
}
