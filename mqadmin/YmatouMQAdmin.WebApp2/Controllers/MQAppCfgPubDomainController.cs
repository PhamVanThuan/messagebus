using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using YmatouMQAdmin.Domain.IRepository;
using YmatouMQAdmin.Domain.Specifications;
using YmatouMQAdmin.Repository;
using YmatouMQNet4.Configuration;
using YmatouMQ.Common.Extensions;
using Ymatou.CommonService;

namespace YmatouMQAdmin.WebApp2.Controllers
{
    public class MQAppCfgPubDomainController : ApiController
    {
        //private static readonly IMQConfigurationRepository cfgRepo = new MQConfigurationRepository();
        [Route("api2/pub/maindomain/cfg")]
        public IEnumerable<MQMainConfiguration> Get([FromUri]string owerhost = null)
        {
            try
            {
                var allCfg = _CfgWrapper.FindAllCfg();
                return _CfgWrapper.FillCfg(allCfg, owerhost);
            }
            catch (Exception ex)
            {
                ApplicationLog.Error("获取maindomain配置错误", ex);
                return Enumerable.Empty<MQMainConfiguration>();
            }
        }
    }
}