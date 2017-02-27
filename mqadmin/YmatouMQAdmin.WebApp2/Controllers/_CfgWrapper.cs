using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using YmatouMQAdmin.Domain.IRepository;
using YmatouMQAdmin.Domain.Specifications;
using YmatouMQNet4.Configuration;
using YmatouMQ.Common.Extensions;
using YmatouMQAdmin.Repository;
using IAlarmRepo = YmatouMQMessageMongodb.Domain.IRepository.IAlarmRepository;
using AlarmRepo = YmatouMQMessageMongodb.Repository.AlarmRepository;
namespace YmatouMQAdmin.WebApp2.Controllers
{
    public class CfgRepositoryDeclare
    {
        private static readonly Lazy<IAlarmRepo> alarmRepo = new Lazy<IAlarmRepo>(() => new AlarmRepo());
        public static IAlarmRepo AlarmRepoInstance { get { return alarmRepo.Value; } }
        /// <summary>
        /// app configuration
        /// </summary>
        private static readonly Lazy<IMQConfigurationRepository> _cfgRepo = new Lazy<IMQConfigurationRepository>(() => new MQConfigurationRepository());
        public static IMQConfigurationRepository cfgRepo { get { return _cfgRepo.Value; } }

        /// <summary>
        /// primary ,secondary configuration
        /// </summary>
        private static readonly Lazy<IConnectionPAndSConfigureationRepository> _connRepo = new Lazy<IConnectionPAndSConfigureationRepository>(() => new ConnectionPAndSConfigureationRepository());
        public static IConnectionPAndSConfigureationRepository connRepo { get { return _connRepo.Value; } }
        /// <summary>
        /// app domain configuration
        /// </summary>
        private static readonly Lazy<IMQAppdomainConfigurationRepository> _cfgAppdomainRepo = new Lazy<IMQAppdomainConfigurationRepository>(() => new MQAppdomainConfigurationRepository());
        public static IMQAppdomainConfigurationRepository cfgAppdomainRepo { get { return _cfgAppdomainRepo.Value; } }

        private static readonly Lazy<IMessageRepository> _MsgRepo = new Lazy<IMessageRepository>(() => new MQMessageRepository());
        public static IMessageRepository MsgRepo { get { return _MsgRepo.Value; } }

        private static readonly Lazy<IMessageStatusRepository> _statusRepo = new Lazy<IMessageStatusRepository>(() => new MessageStatusRepository());
        public static IMessageStatusRepository statusRepo { get { return _statusRepo.Value; } }

        private static readonly Lazy<YmatouMQMessageMongodb.Domain.IRepository.IMessageRepository> _NewMsgRepo = new Lazy<YmatouMQMessageMongodb.Domain.IRepository.IMessageRepository>(() => new YmatouMQMessageMongodb.Repository.MQMessageRepository());
        public static YmatouMQMessageMongodb.Domain.IRepository.IMessageRepository NewMsgRepo { get { return _NewMsgRepo.Value; } }

        private static readonly Lazy<YmatouMQMessageMongodb.Domain.IRepository.IMessageStatusRepository> _NewStatusRepo = new Lazy<YmatouMQMessageMongodb.Domain.IRepository.IMessageStatusRepository>(() => new YmatouMQMessageMongodb.Repository.MessageStatusRepository());
        public static YmatouMQMessageMongodb.Domain.IRepository.IMessageStatusRepository NewStatusRepo { get { return _NewStatusRepo.Value; } }

        private static readonly Lazy<YmatouMQMessageMongodb.Domain.Domain.IRetryMessageRepository> _NewRetryMsgRepo = new Lazy<YmatouMQMessageMongodb.Domain.Domain.IRetryMessageRepository>(() => new YmatouMQMessageMongodb.Repository.RetryMessageRepository());
        public static YmatouMQMessageMongodb.Domain.Domain.IRetryMessageRepository NewRetryMsgRepo { get { return _NewRetryMsgRepo.Value; } }
        private CfgRepositoryDeclare() { }
    }
    public class _CfgWrapper
    {
        public static IEnumerable<MQMainConfiguration> FindDefaultCfg()
        {
            return CfgRepositoryDeclare.cfgRepo.Find(MQCfgControllerSpecifications.MmatchDefaultCfg("default")).AsParallel().ToList();
        }
        public static void ReplaceConnectionString(string conntype, IEnumerable<MQMainConfiguration> cfg)
        {
            var connCfg = CfgRepositoryDeclare.connRepo.FindOne(MQCfgControllerSpecifications.MatchConnectionId(conntype), false);
            if (connCfg != null)
            {
                foreach (var item in cfg)
                {
                    item.ConnCfg.ConnectionString = connCfg.ConnectionString;
                }
            }
        }
        public static IEnumerable<MQMainConfiguration> GetAppCfgInfoDetails(string appid, string code)
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
        public static IEnumerable<MQMainConfiguration> FindAllCfg()
        {
            return CfgRepositoryDeclare.cfgRepo.Find(MQCfgControllerSpecifications.MmatchAppCfg3(null, null), "MQ_Configuration_201505", "MQ_App_Cfg").AsParallel().ToList();
        }
        public static List<MQMainConfiguration> FillCfg(IEnumerable<MQMainConfiguration> allCfg, string ownerHost = null)
        {
            if (!string.IsNullOrEmpty(ownerHost))
                allCfg = allCfg.Where(e => e.OwnerHost.IsEmpty() || e.OwnerHost.Contains(ownerHost));

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
        private static IEnumerable<CallbackConfiguration> AdapterCallBackList(IEnumerable<CallbackConfiguration> callback)
        {
            var tmp = new List<CallbackConfiguration>();
            callback.EachAction(c => tmp.Add(new CallbackConfiguration { CallbackKey = c.CallbackKey }));
            return tmp;
        }
    }
}