using Grpc.Core;
using System;

namespace Xye.Grpc.Core
{
    /// <summary>
    /// 客户端调用器
    /// </summary>
    internal sealed class ClientCallInvoker : CallInvoker
    {
        private readonly IEndpointStrategy _endpointStrategy;
        private readonly string _serviceName;
        private readonly int _maxRetry;

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="endpointStrategy">终端策略</param>
        /// <param name="serviceName">服务名</param>
        /// <param name="maxRetry">最大重试次数</param>
        public ClientCallInvoker(IEndpointStrategy endpointStrategy, string serviceName = "", int maxRetry = 1)
        {
            _serviceName = serviceName;
            _maxRetry = maxRetry;
            _endpointStrategy = endpointStrategy;
        }

        private TResponse Call<TResponse>(Func<CallInvoker, TResponse> call)
        {
            int maxRetry = _maxRetry;
            while (true)
            {
                EndpointModel endpoint;
                var callInvoker = _endpointStrategy.Get(_serviceName, out endpoint);
                if (callInvoker == null)
                {
                    throw new GrpcConsulException($"can't find servie: {_serviceName} from {(endpoint != null ? endpoint.ToString() : "")}");
                }
                try
                {
                    return call(callInvoker);
                }
                catch(Exception ex)
                {
                    if (ex is RpcException && endpoint.IsDisconvry)
                    {
                        _endpointStrategy.AddBlack(endpoint.ToString());
                    } //加入黑名单,避免注册中心还存在，但是实际服务已经关闭时
                    if (0 > --maxRetry)
                    {
                        throw;
                    }
                } //直接异常抛出去
            }
        }

        public override TResponse BlockingUnaryCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options, TRequest request)
        {
            return Call(ci => ci.BlockingUnaryCall(method, host, options, request));
        }

        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options, TRequest request)
        {
            return Call(ci => ci.AsyncUnaryCall(method, host, options, request));
        }

        public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options, TRequest request)
        {
            return Call(ci => ci.AsyncServerStreamingCall(method, host, options, request));
        }

        public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options)
        {
            return Call(ci => ci.AsyncClientStreamingCall(method, host, options));
        }

        public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options)
        {
            return Call(ci => ci.AsyncDuplexStreamingCall(method, host, options));
        }
    }
}
