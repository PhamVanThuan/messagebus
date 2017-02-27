using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Ymatou.CommonService;
using YmatouMQAdmin.Domain.IRepository;
using YmatouMQAdmin.Domain.Specifications;
using YmatouMQAdmin.Repository;
using YmatouMQNet4.Configuration;

namespace YmatouMQAdmin.WebApp2.Controllers
{
    public class MQConnectionPAndSController : ApiController
    {
        /// <summary>
        /// MQ 链接管理（主，从连接属性）
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        [Route("api2/mqconn/subdomian/primarysecondary/cfg")]
        public IEnumerable<MQMainConfiguration> Get([FromUri]string appid = null, [FromUri]string code = null, [FromUri]string conntype = "secondary")
        {
            try
            {
                IEnumerable<MQMainConfiguration> cfg;
                if (appid == "default")
                    cfg = _CfgWrapper.FindDefaultCfg();
                else
                    cfg = _CfgWrapper.GetAppCfgInfoDetails(appid, code).AsParallel().ToList();

                if (!cfg.Any()) return cfg;

                _CfgWrapper.ReplaceConnectionString(conntype, cfg);

                return cfg;
            }
            catch (Exception ex)
            {
                ApplicationLog.Error("获取主从配置错误", ex);
                return Enumerable.Empty<MQMainConfiguration>();
            }
        }
    }
}