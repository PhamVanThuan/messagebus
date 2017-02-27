using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YmatouMQ.Common;
using YmatouMQ.Common.Utils;
using YmatouMQ.Log;
using YmatouMQMessageMongodb.Domain.IRepository;
using YmatouMQMessageMongodb.Domain.Specifications;
using YmatouMQMessageMongodb.Repository;
using YmatouMQNet4.Configuration;

namespace YmatouMQMessageMongodb.AppService.Configuration
{
    public class MQAppDomainConfigurationAppService
    {
        private readonly IMQAppdomainConfigurationRepository repo = new MQAppdomainConfigurationRepository();
        private readonly ILog log = LogFactory.GetLogger(LogEngineType.RealtimelWriteFile, "MQAppDomainConfigurationAppService.MQAppDomainConfigurationAppService");

        public MQAppDomainConfigurationAppService()
        {
        }
        //获取所有appdomain配置
        public IEnumerable<AppdomainConfiguration> FindAllAppDomainConfiguration()
        {
            using (var monitor = new MethodMonitor(log, 200, "FindAllAppDomainConfiguration1"))
            {
                return ActionRetryHelp.Retry(() => repo.Find(MQConfigurationSpecifications._MatchOneOrAllAppdomain(null)
                                             , MQConfigurationSpecifications.ConfigurationDb
                                             , MQConfigurationSpecifications.AppDomainTb).AsParallel().AsEnumerable()
                                             , 1
                                             , TimeSpan.FromMilliseconds(300)
                                             , errorHandle: ex => log.Error("FindAllAppDomainConfiguration1", ex));

            }
        }
        //获取指定的domain
        public IEnumerable<AppdomainConfiguration> FindAllAppDomainConfiguration(string domainName, string appid = null, string code = null)
        {
            using (var monitor = new MethodMonitor(log, 200, "FindAllAppDomainConfiguration2"))
            {
                var result = ActionRetryHelp.Retry(() => repo.Find(MQConfigurationSpecifications._MatchOneOrAllAppdomain(domainName)
                                              , MQConfigurationSpecifications.ConfigurationDb
                                              , MQConfigurationSpecifications.AppDomainTb).AsParallel().AsEnumerable()
                                              , 1
                                              , TimeSpan.FromMilliseconds(300)
                                              , errorHandle: ex => log.Error("FindAllAppDomainConfiguration1", ex));

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
        }
    }
}
