using Xye.Grpc.Core;
using Xye.Grpc.Interceptors;
using System;

namespace WebTest.Interceptors
{
    /// <summary>
    /// 时间记录
    /// </summary>
    public class TimingRecordInterceptor : ClientInterceptor
    {
        private const string TIME_TRACKING_FLAG = "timetrackingflag";
        private DateTime dt;
        public TimingRecordInterceptor()
        {

        }

        public override void OnCalling(ClientContext context)
        {
            dt = DateTime.Now;
            //context.Options.Headers.Add(TIME_TRACKING_FLAG, DateTime.Now.Ticks.ToString());
        }

        public override void OnCalled(ClientContext context)
        {
            var ticks = dt.Ticks;
            var dtNow = DateTime.Now;
            double milliseconds = TimeSpan.FromTicks(dtNow.Ticks - ticks).TotalMilliseconds;
            Console.WriteLine($"{dtNow.ToString("yyyy-MM-dd HH:mm:ss")}-[GRpc服务客户端调用耗时]{context.ServiceName}.{context.MethodName} {milliseconds} ms");
        }
    }
}
