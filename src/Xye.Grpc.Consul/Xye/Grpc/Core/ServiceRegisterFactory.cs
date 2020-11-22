#if NETCORE
using Microsoft.Extensions.Options;
#endif
using Xye.Grpc.Configuration;
using Xye.Grpc.Internal;

namespace Xye.Grpc.Core
{
    /// <summary>
    /// 服务注册工厂
    /// </summary>
    public class ServiceRegisterFactory
    {
        private readonly IServiceRegister _serviceRegister;
        private readonly GrpcServerRegister _grpcServerRegisters;
        private ServiceRegister.Entry _entry;

#if NETCORE
        public ServiceRegisterFactory(

            IServiceRegister serviceRegister
            , IOptions<GrpcServerRegister> grpcServerRegisters

            )
        {

            _serviceRegister = serviceRegister;
            _grpcServerRegisters = grpcServerRegisters.Value;
        }
#else
        private static object _locker = new object();
        private static ServiceRegisterFactory _serviceRegisterFactory;
        public static ServiceRegisterFactory Instance
        {
            get
            {
                if (_serviceRegisterFactory == null)
                {
                    lock (_locker)
                    {
                        if (_serviceRegisterFactory == null)
                        {
                            _serviceRegisterFactory = new ServiceRegisterFactory();
                        }
                    }
                }
                return _serviceRegisterFactory;
            }
        }

        private ServiceRegisterFactory()
        {
            _serviceRegister = CommonUtilsHelper._.ServiceRegister;
            _grpcServerRegisters = GrpcServerRegisterWrapper.GrpcServerRegister;
        }
#endif

        public void RegisterService()
        {
            _entry = _serviceRegister.RegisterService(_grpcServerRegisters);
        }

        /// <summary>
        /// 反注册
        /// </summary>
        public void UnregisterService()
        {
            if (_entry != null)
            {
                _entry.Dispose();
            }
        }
    }
}


