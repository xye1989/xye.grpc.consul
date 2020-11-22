#if !NETCORE
using Xye.Grpc.Internal;

namespace Xye.Grpc.Configuration
{
    public class BaseInfoConfigs<T> : BaseConfigs<T>
    {
        /// <summary>
        /// 配置对象值
        /// </summary>
        protected T configInfo = default(T);

        /// <summary>
        /// 初始化对象，并自动加载配置文件
        /// </summary>
        /// <param name="relativeFilePath">相对配置文件路径</param>
        public BaseInfoConfigs(string relativeFilePath)
            : base(relativeFilePath)
        { }

        /// <summary>
        /// 重设有效的（文件中最新的）配置信息列表对象
        /// </summary>
        protected override void ResetRealConfig()
        {
            lock (lockHelper)
            {
                object objInfo = ConfigHelper.LoadFile(typeof(T), ConfigFullName);
                if (objInfo == null)
                {
                    return;
                }
                configInfo = (T)objInfo;
            }
        }

        /// <summary>
        /// 获取当前配置实体类
        /// </summary>
        /// <returns>当前配置实体类</returns>
        public override T GetConfig()
        {
            return configInfo;
        }

        /// <summary>
        /// 更新配置
        /// </summary>
        /// <param name="changeType"></param>
        protected override void UpdateConfig(string changeType)
        {
            switch (changeType)
            {
                case FileChangeTypeConst.RENAMED:
                case FileChangeTypeConst.DELETED:
                    configInfo = default(T);
                    break;
                default:
                    ResetRealConfig();
                    break;
            }
        }
    }
}
#endif