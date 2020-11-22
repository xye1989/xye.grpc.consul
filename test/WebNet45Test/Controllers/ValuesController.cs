using CFGrantBoxGrpc;
using Xye.Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace WebNet45Test.Controllers
{
    public class ValuesController : ApiController
    {
        // GET api/values
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        public string Get(int id)
        {
            var testClient = new ClientFactory<CFGrantBoxGrpc.TestService.TestServiceClient>().Get();
            var result = testClient.GetIP(new GetIPRequest());
            if (result != null)
            {
                return ("Get[" + id + "]" + result.Code + " " + result.Message + " " + result.Data);
            }
            return "value";
        }
    }
}
