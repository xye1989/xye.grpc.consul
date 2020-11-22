using Xye.Grpc.Core;
using Grpc.Core;
using Grpc.Core.Interceptors;
using System;

namespace Xye.Grpc.Interceptors
{
    public abstract class ClientInterceptor : Interceptor
    {
        /// <summary>
        /// OnServerExecuting
        /// </summary>
        /// <param name="context"></param>
        public abstract void OnCalling(ClientContext context);

        /// <summary>
        /// OnServerExecuted
        /// </summary>
        /// <param name="context"></param>
        public abstract void OnCalled(ClientContext context);

        #region ClientMethod

        /// <summary>
        /// 核心调用
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="context"></param>
        /// <param name="DoCall"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private object Call<TRequest, TResponse>(ClientInterceptorContext<TRequest, TResponse> context, Func<object> DoCall, TRequest request = null)
              where TRequest : class
              where TResponse : class
        {
            var clientContext = new ClientContext()
            {
                ServiceName = context.Method.ServiceName,
                MethodType = context.Method.Type,
                MethodName = context.Method.Name,
                Host = context.Host,
                Options = context.Options,
                Request = request,
            };
            OnCalling(clientContext);
            var response = DoCall();
            clientContext.Response = response;
            OnCalled(clientContext);
            return response;
        }

        /// <summary>
        /// BlockingUnaryCall
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <param name="continuation"></param>
        /// <returns></returns>
        public override TResponse BlockingUnaryCall<TRequest, TResponse>(
            TRequest request
            , ClientInterceptorContext<TRequest, TResponse> context
            , BlockingUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            return Call(context
            , () =>
            {
                return continuation(request, context);
            }
            , request) as TResponse;
        }

        /// <summary>
        /// AsyncUnaryCall
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <param name="continuation"></param>
        /// <returns></returns>
        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
            TRequest request
            , ClientInterceptorContext<TRequest, TResponse> context
            , AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            return Call(context
            , () =>
            {
                return continuation(request, context);
            }
            , request) as AsyncUnaryCall<TResponse>;
        }

        /// <summary>
        /// AsyncClientStreamingCall
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="context"></param>
        /// <param name="continuation"></param>
        /// <returns></returns>
        public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(
            ClientInterceptorContext<TRequest, TResponse> context
            , AsyncClientStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            return Call(context
             , () =>
             {
                 return continuation(context);
             }) as AsyncClientStreamingCall<TRequest, TResponse>;
        }

        /// <summary>
        /// AsyncDuplexStreamingCall
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="context"></param>
        /// <param name="continuation"></param>
        /// <returns></returns>
        public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(
            ClientInterceptorContext<TRequest, TResponse> context
            , AsyncDuplexStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            return Call(context
               , () =>
               {
                   return continuation(context);
               }) as AsyncDuplexStreamingCall<TRequest, TResponse>;
        }

        /// <summary>
        /// AsyncServerStreamingCall
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <param name="continuation"></param>
        /// <returns></returns>
        public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(
            TRequest request
            , ClientInterceptorContext<TRequest, TResponse> context
            , AsyncServerStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            return Call(context
              , () =>
              {
                  return continuation(request, context);
              }
              , request) as AsyncServerStreamingCall<TResponse>;
        }
        #endregion
    }
}
