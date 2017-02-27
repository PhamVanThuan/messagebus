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
    public class MQAppdomainCfgController : ApiController
    {
        //private static readonly IMQAppdomainConfigurationRepository cfgRepo = new MQAppdomainConfigurationRepository();
        public IEnumerable<AppdomainConfiguration> Get([FromUri]string domainName = null,[FromUri]string appid=null, [FromUri]string code = null)
        {
            try
            {
                var result = CfgRepositoryDeclare.cfgAppdomainRepo.Find(MQAppdomainConfigurationSpecifications._MatchOneOrAllAppdomain(domainName), "MQ_Configuration_201505", "MQ_Appdomain_Cfg");
                if (result == null || !result.Any()) return result;
                if (string.IsNullOrEmpty(appid) || string.IsNullOrEmpty(code)) return result;
                var val = result.FirstOrDefault();               
                return new List<AppdomainConfiguration> 
                {
                    {new AppdomainConfiguration
                    {
                        AppId=val.AppId,
                        Code=val.Code,
                        DomainName=val.AppId,
                        Host=val.Host,
                        Status=val.Status,
                        Version=val.Version,
                        Items=new List<DomainItem>{{val.Items.AsParallel ().FirstOrDefault(e=>e.AppId==appid&&e.Code==code)}}
                    }}
                };
            }
            catch (Exception ex)
            {
                ApplicationLog.Error("获取 MQAppdomainCfgController 错误 ", ex);
            }
            return null;
        }
    }
}