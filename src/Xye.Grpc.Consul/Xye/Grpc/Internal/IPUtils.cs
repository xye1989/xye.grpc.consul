using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Xye.Grpc.Internal
{
    internal class IPUtils
    {
        /// <summary>
        /// 获取默认端口
        /// </summary>
        /// <returns></returns>
        public static int GetDefaultPort()
        {
            return 80;
        }

        /// <summary>
        /// 获取本地IP地址信息
        /// </summary>
        public static string GetAddressIP()
        {
            //192 -> 172 -> 10 -> 0
            IDictionary<int, string> dict = new Dictionary<int, string>();
            var addressList = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
            foreach (IPAddress _IPAddress in addressList)
            {
                if (_IPAddress.AddressFamily.ToString() == "InterNetwork")
                {
                    var ip = _IPAddress.ToString();
                    if (ip.IndexOf("10.") == 0) TryAddValue(10, ip, dict);
                    else if (_IPAddress.ToString().IndexOf("172.") == 0) TryAddValue(172, ip, dict);
                    else if (_IPAddress.ToString().IndexOf("192.") == 0) TryAddValue(192, ip, dict);
                    else TryAddValue(0, ip, dict);
                }
            }
            if (dict.Count > 0)
            {
                return dict.ToList()
                        .OrderByDescending(ip => ip.Key)
                        .Select(ip => ip.Value)
                        .FirstOrDefault();
            }
            return "";
        }

        private static void TryAddValue(int key, string value, IDictionary<int, string> dict)
        {
            if (dict.ContainsKey(key))
            {
                dict[key] = value;
            }
            else
            {
                dict.Add(key, value);
            }
        }
    }
}
