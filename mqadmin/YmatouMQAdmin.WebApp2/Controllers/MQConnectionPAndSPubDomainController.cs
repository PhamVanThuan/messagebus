using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using YmatouMQAdmin.Domain.IRepository;
using YmatouMQAdmin.Repository;
using YmatouMQNet4.Configuration;
using YmatouMQ.Common.Extensions;
using Ymatou.CommonService;
using YmatouMQAdmin.Domain.Specifications;

namespace YmatouMQAdmin.WebApp2.Controllers
{
    [Route("api2/mqconn/pubdomian/primarysecondary/cfg")]
    public class MQConnectionPAndSPubDomainController : ApiController
    {

        public IEnumerable<MQMainConfiguration> Get([FromUri]string owerhost = null, [FromUri]string appid = null, [FromUri]string code = null, [FromUri]string conntype = "secondary")
        {
            try
            {
                var allCfg = _CfgWrapper.FindAllCfg();

                allCfg = _CfgWrapper.FillCfg(allCfg, owerhost);

                _CfgWrapper.ReplaceConnectionString(conntype, allCfg);
                return allCfg;
            }
            catch (Exception ex)
            {
                ApplicationLog.Error("获取maindomain配置错误", ex);
                return Enumerable.Empty<MQMainConfiguration>();
            }
        }
    }
}