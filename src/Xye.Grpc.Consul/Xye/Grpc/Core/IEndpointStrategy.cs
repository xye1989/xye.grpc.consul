using Grpc.Core;
using Grpc.Core.Interceptors;
using System.Collections.Generic;

namespace Xye.Grpc.Core
{
    /// <summary>
    /// 终端策略
    /// </summary>
    public interface IEndpointStrategy
    {
        /// <summary>
        /// 拦截器
        /// </summary>
        List<Interceptor> Interceptors { get; set; }

        /// <summary>
        /// 获取目标IP:Port
        /// </summary>
        List<string> Targets { get; }

        /// <summary>
        /// 获取调用器
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        CallInvoker Get(string serviceName, out EndpointModel endpoint);

        /// <summary>
        /// 创建
        /// </summary>
        /// <param name="keys"></param>
        void Create(params string[] keys);

        /// <summary>
        /// 撤销
        /// </summary>
        /// <param name="keys"></param>
        void Revoke(params string[] keys);

        /// <summary>
        /// 关闭所有
        /// </summary>
        void ShutDownAll();
        
        /// <summary>
        /// 获取key
        /// </summary>
        /// <param name="serviceName"服务名></param>
        /// <param name="target">目标IP:Port</param>
        /// <returns></returns>
        string GetCallInvokerKey(string serviceName, string target);

        /// <summary>
        /// 是否存在服务
        /// </summary>
        /// <param name="key"></param>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        bool IsExistService(string key, string serviceName);

        /// <summary>
        /// 添加黑名单
        /// </summary>
        /// <param name="target"></param>
        void AddBlack(string target);
    }
}
