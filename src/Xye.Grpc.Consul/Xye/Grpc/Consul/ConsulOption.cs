#if !NETCORE
namespace Xye.Grpc.Consul
{
    public class ConsulOption
    {
        /// <summary>
        /// Consul配置文件地址
        /// </summary>
        public string ConsulServerConfigPath { get; set; }

        /// <summary>
        /// Grpc服务配置文件地址
        /// </summary>
        public string GrpcServiceConfigPath { get; set; }

        /// <summary>
        /// Grpc服务注册配置路径
        /// </summary>
        public string GrpcServerRegisterPath { get; set; }
    }
}
#endif