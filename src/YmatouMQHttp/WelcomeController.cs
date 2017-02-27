using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace YmatouMQHttp
{
    [Route("bus/test/")]
    public class WelcomeController : ApiController
    {
        public string Get()
        {
            return "welcome ymatou mq..." + DateTime.Now;
        }
    }
}
