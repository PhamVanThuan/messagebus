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
    public class MQDefaultCfgController : ApiController
    {
        //private static readonly IMQConfigurationRepository cfgRepo = new MQConfigurationRepository();
        public MQMainConfiguration Get([FromUri]string appid = null)
        {
            try
            {
                return CfgRepositoryDeclare.cfgRepo.Find(MQCfgControllerSpecifications.MmatchDefaultCfg(appid)).FirstOrDefault();
            }
            catch (Exception ex)
            {
                Ymatou.CommonService.ApplicationLog.Error("获取 MQDefaultCfg 错误 ", ex);
            }
            return null;
        }
    }
}
