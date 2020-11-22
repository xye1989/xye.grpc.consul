using Xye.Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebTest.Services
{
    public class TestService : ITestService
    {
        private readonly CFGrantBoxGrpc.TestService.TestServiceClient _client;

        public TestService(IClientFactory<CFGrantBoxGrpc.TestService.TestServiceClient> clientFactory)
        {
            _client = clientFactory.Get();
        }

        public async Task<string> GetAsync()
        {
            try
            {
                var resp = await _client.GetIPAsync(new CFGrantBoxGrpc.GetIPRequest());
                if (resp != null)
                {
                    return $"{resp.Data}";
                }
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
