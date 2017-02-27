using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YmatouMQMessageMongodb.Domain.IRepository;
using YmatouMQMessageMongodb.Domain.Specifications;
using YmatouMQNet4.Configuration;
using YmatouMQ.Common.Extensions;
using YmatouMQMessageMongodb.Repository;
using YmatouMQ.Common;
using YmatouMQ.Log;
using YmatouMQ.Common.Utils;

namespace YmatouMQMessageMongodb.AppService.Configuration
{
    public class MQAppConfigurationAppService
    {
        private readonly IMQConfigurationRepository repo = new MQConfigurationRepository();
        private readonly IConnectionPAndSConfigureationRepository repo_conn = new ConnectionPAndSConfigureationRepository();
        private readonly ILog log = LogFactory.GetLogger(LogEngineType.RealtimelWriteFile, "YmatouMQMessageMongodb.AppService.Configuration.MQAppConfigurationAppService");
        public MQAppConfigurationAppService()
        {
        }

        public IEnumerable<string> FindAllAppId()
        {
            return repo.FindAllAppId();
        }

        //获取默认全局配置
        public MQMainConfiguration FindDefaultAppConfiguration(string conntype = "primary")
        {
            using (var monitor = new MethodMonitor(log, 200, "FindDefaultAppConfiguration {0}".Fomart(conntype)))
            {
                return ActionRetryHelp.Retry(() =>
                {
                    var cfg = repo.FindOne(MQConfigurationSpecifications.MmatchDefaultCfg(), MQConfigurationSpecifications.ConfigurationDb, MQConfigurationSpecifications.ConfigurationDefaultCfgTb);
                    if (conntype == MQConfigurationSpecifications.secondaryConn)
                    {
                        ReplaceConnectionString(cfg, FindRabbitMQConn(conntype));
                    }
                    return cfg;
                }
                , 1
                , TimeSpan.FromMilliseconds(200)
                , errorHandle: ex => log.Error("FindDefaultAppConfiguration {0}".Fomart(conntype), ex));
            }
        }
        //获取补偿服务消息配置
        public IEnumerable<MQMainConfiguration> FindCompensateMessageAppCfg()
        {
            using (var monitor = new MethodMonitor(log, 200, "FindCompensateMessageAppCfg.0"))
            {
                var tmp = new List<MQMainConfiguration>();
                var result = FindAllAppCfgInfoDetails(string.Empty);
                result.EachAction(cfg =>
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
                return tmp;
            }
        }
        //获取所有app cfg 配置
        public IEnumerable<MQMainConfiguration> FindAllAppCfgInfoDetails(string conntype = "primary")
        {
            using (var monitor = new MethodMonitor(log, 200, "FindAppCfgInfoDetails.0 {0}".Fomart(conntype)))
            {
                var result = ActionRetryHelp.Retry(() => repo.Find(MQConfigurationSpecifications.MmatchAppCfg3(null, null)
                                                                    , MQConfigurationSpecifications.ConfigurationDb
                                                                    , MQConfigurationSpecifications.ConfigurationAppDetailsTb).AsParallel().ToList()
                                                     , 1
                                                     , TimeSpan.FromMilliseconds(300)
                                                     , errorHandle: ex => log.Error("FindAppCfgInfoDetails {0}".Fomart(conntype), ex));
                if (result.IsEmptyEnumerable())
                {
                    log.Error("从mongodb获取配置数据失败");
                    return Enumerable.Empty<MQMainConfiguration>();
                }
                if (conntype == MQConfigurationSpecifications.secondaryConn)
                {
                    var conn = FindRabbitMQConn(conntype);
                    result.EachAction(c => ReplaceConnectionString(c, conn));
                }
                return result;
            }
        }
        //获取具体配置
        public IEnumerable<MQMainConfiguration> FindAppCfgInfoDetails(string appid, string code, string conntype = "primary")
        {
            using (var monitor = new MethodMonitor(log, 200, "FindAppCfgInfoDetails appid {0},code {1},conntype {2}".Fomart(appid, code, conntype)))
            {
                var cfg = ActionRetryHelp.Retry(() => repo.Find(MQConfigurationSpecifications.MmatchAppCfg3(appid, code)
                                                                , MQConfigurationSpecifications.ConfigurationDb
                                                                , MQConfigurationSpecifications.ConfigurationAppDetailsTb).AsParallel().ToList()
                                         , 1
                                         , TimeSpan.FromMilliseconds(200)
                                         , errorHandle: ex => log.Error("", ex));
                if (conntype == MQConfigurationSpecifications.secondaryConn)
                {
                    var conn = FindRabbitMQConn(conntype);
                    cfg.EachAction(c => ReplaceConnectionString(c, conn));
                }
                return cfg;
            }

        }
        //获取发布消息domain，配置明晰
        public IEnumerable<MQMainConfiguration> FindPublishMessageDomainAppCfgInfoDetails(string owerhost = null, string appid = null, string code = null, string conntype = "primary")
        {
            using (var monitor = new MethodMonitor(log, 200, "FindPublishMessageDomainAppCfgInfoDetails {0}".Fomart(conntype)))
            {
                var result = ActionRetryHelp.Retry(() =>
                      repo.Find(MQConfigurationSpecifications.MmatchAppCfg3(appid, code)
                        , MQConfigurationSpecifications.ConfigurationDb
                        , MQConfigurationSpecifications.ConfigurationAppDetailsTb).AsParallel().ToList()
                , 1
                , TimeSpan.FromMilliseconds(200)
                , errorHandle: ex => log.Error("", ex));

                if (conntype == MQConfigurationSpecifications.secondaryConn)
                {
                    var conn = FindRabbitMQConn(conntype);
                    result.EachAction(c => ReplaceConnectionString(c, conn));
                }
                return FillCfg(result, owerhost);
            }
        }
        public List<MQMainConfiguration> FillCfg(IEnumerable<MQMainConfiguration> allCfg, string ownerHost = null)
        {
            var serCfgCount = allCfg.Count();
            if (!string.IsNullOrEmpty(ownerHost))
                allCfg = allCfg.Where(e => e.OwnerHost.IsEmpty() || e.OwnerHost.Contains(ownerHost));

            log.Info("sync configuration host {0},ownerHost {1},filter cfg {2},mongo cfg count {3}"
                , _Utils.GetLocalHostIp(), ownerHost, allCfg.Count(), serCfgCount);
            var tmp = new List<MQMainConfiguration>();
            allCfg.EachAction(cfg =>
            {
                var msgTmp = new List<MessageConfiguration>();
                cfg.MessageCfgList.EachAction(m => msgTmp.Add(new MessageConfiguration
                {
                    Code = m.Code,
                    Enable = m.Enable,
                    PublishCfg = m.PublishCfg,
                    ExchangeCfg = m.ExchangeCfg,
                    MessagePropertiesCfg = m.MessagePropertiesCfg,
                    QueueCfg = m.QueueCfg,
                    ConsumeCfg = m.ConsumeCfg,
                    CallbackCfgList = AdapterCallBackList(m.CallbackCfgList)
                }));
                tmp.Add(new MQMainConfiguration
                {
                    AppId = cfg.AppId,
                    Version = cfg.Version,
                    ConnCfg = cfg.ConnCfg,
                    MessageCfgList = msgTmp
                });
            });
            return tmp;
        }
        private ConnectionPAndSConfigureation FindRabbitMQConn(string conntype)
        {
            return repo_conn.FindOne(MQConfigurationSpecifications.MatchConnectionId(conntype), MQConfigurationSpecifications.ConfigurationDb, MQConfigurationSpecifications.RabbitMQConnStringTb, false);
        }
        private static IEnumerable<CallbackConfiguration> AdapterCallBackList(IEnumerable<CallbackConfiguration> callback)
        {
            var tmp = new List<CallbackConfiguration>();
            callback.EachAction(c => tmp.Add(new CallbackConfiguration { CallbackKey = c.CallbackKey }));
            return tmp;
        }
        private static void ReplaceConnectionString(MQMainConfiguration cfg, ConnectionPAndSConfigureation conn)
        {
            if (conn == null || conn.ConnectionString.IsEmpty()) return;
            cfg.ConnCfg.ConnectionString = conn.ConnectionString;
        }
    }
}
