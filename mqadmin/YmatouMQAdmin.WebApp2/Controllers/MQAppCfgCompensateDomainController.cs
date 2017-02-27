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

namespace YmatouMQAdmin.WebApp2.Controllers
{
    public class MQAppCfgCompensateDomainController : ApiController
    {
        //private static readonly IMQConfigurationRepository cfgRepo = new MQConfigurationRepository();
        [Route("api2/compensate/compensateDomain/cfg")]
        public IEnumerable<MQMainConfiguration> Get()
        {
            var tmp = new List<MQMainConfiguration>();
            try
            {
                var allCfg = CfgRepositoryDeclare.cfgRepo.Find(MQCfgControllerSpecifications.MmatchAppCfg3(null, null), "MQ_Configuration_201505", "MQ_App_Cfg");
                allCfg.EachAction(cfg =>
                {
                    var msgTmp = new List<MessageConfiguration>();
                    cfg.MessageCfgList.EachAction(m => msgTmp.Add(new MessageConfiguration
                    {
                        Code = m.Code,
                        Enable = m.Enable,
                        CallbackCfgList = m.CallbackCfgList
                    }));
                    tmp.Add(new MQMainConfiguration
                    {
                        AppId = cfg.AppId,
                        Version = cfg.Version,
                        MessageCfgList = msgTmp
                    });
                });
            }
            catch (Exception ex)
            {
                Ymatou.CommonService.ApplicationLog.Error("获取maindomain配置错误", ex);
            }
            return tmp;
        }       
    }
}