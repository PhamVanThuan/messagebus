using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using YmatouMQNet4.Dto;

namespace YmatouMQServer.Controllers
{
    public class BusServerTestController : ApiController
    {
        [Route("bus/bustest/")]
        public ResponseData<string> Get()
        {
            return ResponseData<string>.CreateSuccess("mq bus app v1.0", "ok");
        }
    }
}
