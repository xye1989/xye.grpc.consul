#if !NETCORE
using Xye.Grpc.Internal;
using System.Collections.Generic;

namespace Xye.Grpc.Configuration
{
    public class BaseCollectionConfigs<T> : BaseConfigs<List<T>>
    {
        /// <summary>
        /// 配置对象值
        /// </summary>
        protected List<T> configInfoList = null;

        /// <summary>
        /// 初始化对象，并自动加载配置文件
        /// </summary>
        /// <param name="relativeFilePath">相对配置文件路径</param>
        public BaseCollectionConfigs(string relativeFilePath)
            : base(relativeFilePath)
        { }

        /// <summary>
        /// 重设有效的（文件中最新的）配置信息列表对象
        /// </summary>
        protected override void ResetRealConfig()
        {
            lock (lockHelper)
            {
                object objCollection = ConfigHelper.LoadFile(typeof(List<T>), ConfigFullName);
                if (objCollection == null)
                {
                    return;
                }
                this.configInfoList = (List<T>)objCollection;
            }
        }

        /// <summary>
        /// 获取当前配置实体类
        /// </summary>
        /// <returns>当前配置实体类</returns>
        public override List<T> GetConfig()
        {
            return configInfoList;
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
                    configInfoList = null;
                    break;
                default:
                    ResetRealConfig();
                    break;
            }
        }
    }
}
#endif