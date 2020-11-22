#if !NETCORE
using Xye.Grpc.Internal;
using System;

namespace Xye.Grpc.Configuration
{
    /// <summary>
    /// 基础配置类
    /// </summary>
    public abstract class BaseConfigs<TResult>
    {
        /// <summary>
        /// 相对配置文件路径
        /// </summary>
        private string _relativeFilePath = string.Empty;

        /// <summary>
        /// 完整配置文件路径
        /// </summary>
        private string _configFullName = string.Empty;

        /// <summary>
        /// 锁对象
        /// </summary>
        protected object lockHelper = new object();

        /// <summary>
        /// 初始化对象，并自动加载配置文件
        /// </summary>
        /// <param name="relativeFilePath">相对配置文件路径</param>
        public BaseConfigs(string relativeFilePath)
        {
            _relativeFilePath = relativeFilePath;
            LoadConfig();
        }

        /// <summary>
        /// 创建配置文件监控器 (缓存机制应用)
        /// </summary>
        protected virtual void CreateConfigMonitor()
        {
            FileWathcer.Monitor(ConfigFullName, MonitorCallback);
        }

        protected virtual void MonitorCallback(string changeType, string fullFileName, Exception ex)
        {
            if (ex != null)
            {
                throw ex;
            }
            if (fullFileName != ConfigFullName)
            {
                return;
            }
            UpdateConfig(changeType);
        }

        /// <summary>
        /// 重设有效的（文件中最新的）配置信息列表对象
        /// </summary>
        protected abstract void ResetRealConfig();

        /// <summary>
        /// 加载配置文件信息
        /// </summary>
        public void LoadConfig()
        {
            CreateConfigMonitor();
            ResetRealConfig();
        }

        /// <summary>
        /// 获取当前配置实体类
        /// </summary>
        /// <returns>当前配置实体类</returns>
        public abstract TResult GetConfig();

        /// <summary>
        /// 更新配置
        /// </summary>
        protected abstract void UpdateConfig(string changeType);

        /// <summary>
        /// 当前配置文件完整路径
        /// </summary>
        protected virtual string ConfigFullName
        {
            get
            {
                if (string.IsNullOrEmpty(_configFullName))
                {
                    _configFullName = ConfigHelper.GetFullPath(_relativeFilePath);
                }
                return _configFullName;
            }
            set
            {
                _configFullName = value;
            }
        }
    }
}
#endif