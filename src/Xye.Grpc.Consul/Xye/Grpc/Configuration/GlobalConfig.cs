using System;

namespace Xye.Grpc.Configuration
{
    internal static class GlobalConfig
    {
        public static TimeSpan BlacklistPeriod = TimeSpan.FromSeconds(90);
        public static readonly TimeSpan CheckInterval = TimeSpan.FromSeconds(10);
        public static readonly TimeSpan CriticalInterval = TimeSpan.FromSeconds(30);
    }
}
