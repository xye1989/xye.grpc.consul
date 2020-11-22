#if !NETCORE
using Xye.Grpc.Internal;
#endif

namespace Xye.Grpc.Configuration
{
#if !NETCORE
    /// <summary>
    /// 获取
    /// </summary>
    public static class ConsulServerConfigWrapper
    {
        private static BaseInfoConfigs<ConsulServerRoot> config = null;
        private static object _locker = new object();

        /// <summary>
        /// 获取当前配置实体类集合
        /// </summary>
        /// <returns>配置实体类集合</returns>
        public static ConsulServerConfig ConsulServiceConfig
        {
            get
            {
                if (config == null)
                {
                    lock (_locker)
                    {
                        if (config == null)
                        {
                            config = new BaseInfoConfigs<ConsulServerRoot>(CommonUtilsHelper._.ConsulServerConfigPath);
                        }
                    }
                }
                return config.GetConfig()?.ConsulServerConfig;
            }
        }
    }

    public class ConsulServerRoot
    {
        public ConsulServerConfig ConsulServerConfig { get; set; }
    }
#endif

    /// <summary>
    /// 
    /// </summary>
    public class ConsulServerConfig
    {
        /// <summary>
        /// 宿主
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// 端口
        /// </summary>
        public string Port { get; set; }

        /// <summary>
        /// 守护线程睡眠时间（单位 秒）
        /// </summary>
        public int DaemonSleep { get; set; } = 1;

        /// <summary>
        /// 黑名单时间
        /// </summary>
        public double BlackTime { get; set; } = 60;
    }
}
