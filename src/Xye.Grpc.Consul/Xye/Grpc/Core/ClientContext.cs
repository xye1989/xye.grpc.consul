using Grpc.Core;
using System.Reflection;

namespace Xye.Grpc.Core
{
    /// <summary>
    /// 客户端上下文
    /// </summary>
    public class ClientContext
    {
        /// <summary>
        /// 请求
        /// </summary>
        public object Request { set; get; }

        /// <summary>
        /// 响应
        /// </summary>
        public object Response { set; get; }

        /// <summary>
        /// Host
        /// </summary>
        public string Host { set; get; }

        /// <summary>
        /// 参数
        /// </summary>
        public CallOptions Options { set; get; }

        /// <summary>
        /// 服务名称
        /// </summary>
        public string ServiceName { set; get; }

        /// <summary>
        /// 方法名称
        /// </summary>
        public string MethodName { set; get; }

        /// <summary>
        /// 方法类型
        /// </summary>
        public MethodType MethodType { set; get; }

        /// <summary>
        /// 执行方法
        /// </summary>
        public MethodInfo MethodInfo { set; get; }
    }
}
