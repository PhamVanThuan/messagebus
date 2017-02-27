using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using YmatouMQAdmin.Domain.IRepository;
using YmatouMQAdmin.Domain.Specifications;
using YmatouMQAdmin.Repository;
using YmatouMQNet4.Configuration;

namespace YmatouMQAdmin.WebApp2.Controllers
{
    //[Route("json/reply/LoginBillsRequestDto")]
    public class MQSysCfgController : ApiController
    {
        //获取MQ应用系统配置项
        private static readonly IMQSystemConfigurationRepository cfgRepo = new MQSystemConfigurationRepository();
        public IEnumerable<MQSystemConfiguration> Get([FromUri]string appid = null)
        {
            return cfgRepo.Find(MQCfgControllerSpecifications.MatchSysCfgAppId(appid), "MQ_Configuration_201505", "MQ_Sys_Cfg");
        }
    }
}
