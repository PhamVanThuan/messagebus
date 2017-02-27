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
using YmatouMQ.Common.Extensions;

using Route = System.Web.Http.RouteAttribute;

namespace YmatouMQAdmin.WebApp2.Controllers
{
    public class MQAppCfgController : ApiController
    {
        //
        // GET: /MQAppCfg/
        /// <summary>
        /// 总线配置main入口
        /// </summary>
        //private static readonly IMQConfigurationRepository cfgRepo = new MQConfigurationRepository();
        public IEnumerable<MQMainConfiguration> Get([FromUri]string appid = null, [FromUri]string code = null)
        {
            try
            {
                if (appid == "default")
                    return CfgRepositoryDeclare.cfgRepo.Find(MQCfgControllerSpecifications.MmatchDefaultCfg(appid));
                else               
                    return GetAppCfgInfoDetails(appid, code);                
            }
            catch (Exception ex)
            {
                Ymatou.CommonService.ApplicationLog.Error("获取MQAPP配置错误", ex);
            }
            return null;
        }
       
        private static IEnumerable<MQMainConfiguration> GetAppCfgInfoDetails(string appid, string code)
        {
            var result = CfgRepositoryDeclare.cfgRepo.Find(MQCfgControllerSpecifications.MmatchAppCfg3(appid, null), "MQ_Configuration_201505", "MQ_App_Cfg");
            if (!string.IsNullOrEmpty(code))
            {
                var r = result.FirstOrDefault();
                if (r != null)
                {
                    return new List<MQMainConfiguration>
                        {
                            new  MQMainConfiguration{AppId=r.AppId
                                ,ConnCfg=r.ConnCfg
                                ,Version=r.Version
                                ,MessageCfgList=new List<MessageConfiguration>{r.MessageCfgList.AsParallel().SingleOrDefault(c=>c.Code==code)}}
                        };
                }
            }
            return result;
        }

    }
}
