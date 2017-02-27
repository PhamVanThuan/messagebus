using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YmatouMQ.Common;
using YmatouMQ.Log;
using YmatouMQ.Subscribe;
using YmatouMQ.Common.Extensions;
using YmatouMQ.Common.Extensions._Task;
using YmatouMQ.Common.Utils;
using System.Threading;
using YmatouMQ.ConfigurationSync;
using YmatouMQNet4.Configuration;
using YmatouMQMessageMongodb.AppService;

namespace YmatouMQ.SubscribeAppDomainSingle
{
    public class MessageBusSubscribeManager
    {
        private static readonly ILog log = LogFactory.GetLogger(LogFactory._LogType, "YmatouMQ.SubscribeAppDomainSingle.MessageBusSubscribeSetup");
        private static readonly string typeName = typeof(_Subscribe).FullName;
        private static readonly string exeAssembly = typeof(_Subscribe).Assembly.FullName;
        private static readonly YmatouMQSubscribeProxy subscribeProxy = new YmatouMQSubscribeProxy();
        public static void Init()
        {
            //
            InitLog4NetService();
            //
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
          
            //
            RetryMessageAppService_Batch.Instance.StartBatchAddJob();
            MessageHandleStatusAppService_Batch.Instance.StartBatchAddJob();
            MessagePushStatusAppService.RunTask();
            //
            var task = Task.Factory.StartNew(() => AppdomainConfigurationManager.Builder.RegisterUpdateCallback(ReBuilderAppDomain))
                                   .ContinueWith(t => SubscribeStart(), TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously)
                                   .ContinueWith(t => MQMainConfigurationManager.Builder.Start(), TaskContinuationOptions.OnlyOnRanToCompletion)
                                   .ContinueWith(t => AppdomainConfigurationManager.Builder.Start(), TaskContinuationOptions.OnlyOnRanToCompletion)
                                   .WithHandleException(log, null, "消息总线推送服务启动异常{0}", "");
            //
            log.Debug("subsceibe 初始化完成");
        }
        public static void Close()
        {
            try
            {
                subscribeProxy.StopSubcribeAll();
                log.Debug("StopSubcribeAll stop ok.");
                MQMainConfigurationManager.Builder.Stop();
                log.Debug("MQMainConfigurationManager stop ok.");
                AppdomainConfigurationManager.Builder.Stop();
                log.Debug("AppdomainConfigurationManager stop ok.");
                MessageHandleStatusAppService_Batch.Instance.StopBatchAddJob();
                log.Debug("MessageHandleStatusAppService_Batch stop ok.");
                RetryMessageAppService_Batch.Instance.StopBatchAddJob();
                log.Debug("RetryMessageAppService_Batch stop ok.");
                MessagePushStatusAppService.Stop();
                log.Debug("MessagePushStatusAppService stop ok.");
            }
            catch (Exception ex)
            {
                log.Error("subscribe stop exception", ex);
            }
        }
        static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();
            log.Error("main appdomian TaskScheduler_UnobservedTaskException {0} {1}", sender, e.Exception.ToString());
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            log.Error("main appdomian UnhandledException {0},{1}", sender, e.ExceptionObject.ToString());
        }

        private static void InitLog4NetService()
        {
            log4net.GlobalContext.Properties["LogFileName"] = AppDomain.CurrentDomain.FriendlyName.Replace(":", "");
            log4net.GlobalContext.Properties["LogDirectory"] = AppDomain.CurrentDomain.FriendlyName.Replace(":", "");
            log4net.Config.XmlConfigurator.Configure(new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "log4net.config")));
        }
        private static void SubscribeStart()
        {
            var startMemory = GC.GetTotalMemory(false);
            log.Debug("MQ subscribe begin start ");
            var watch = Stopwatch.StartNew();
            var adCfgList = AppdomainConfigurationManager.Builder.GetAllAppdomain();
            adCfgList = adCfgList.Where(e => e.Value.Status == DomainAction.Normal
                //&& ((e.Value.AppId == "test2" && e.Value.Code == "liguo")
                //|| e.Value.AppId == "busperformanctest" && e.Value.Code == "performanctest40"
                // )
               && (e.Value.Host.IsEmpty() || _Utils.IsOwnerCurrentHost(e.Value.Host))).ToDictionary(e => e.Key, e => e.Value);
            var domaincount = adCfgList.Sum(e => e.Value.Items.Count(_i => _i._Status == DomainAction.Normal));
            log.Debug("本机 {0} 需要创建 {1} 个消息订阅实例,loadAppdomainCfg count {2}", _Utils.GetLocalHostIp(), domaincount, adCfgList.Count);
            var mqCfglist = MQMainConfigurationManager.Builder.GetConfiguration();
            //创建所有appdomain
            adCfgList
                .Select(e => e.Value)
                .SelectMany(e => e.Items)
                .Where(e => e._Status == DomainAction.Normal)
                .EachAction(e => TryCreateSubscribeAndStartAsync(mqCfglist, e));

            watch.Stop();
            var endMemory = GC.GetTotalMemory(false);
            log.Debug("MQ subscribe start success,create {0} domain,run time {1} ms,memory {2:N0} byte"
                , subscribeProxy.SuscribeInstanceCount, watch.ElapsedMilliseconds, (endMemory - startMemory));
        }
        private static void TryCreateSubscribeAndStartAsync(Dictionary<string, MQMainConfiguration> mqCfglist, DomainItem domianCfg)
        {
            var cfgInfo = mqCfglist.TryGetVal(domianCfg.AppId);
            if (cfgInfo == null || !cfgInfo.MessageCfgList.Any(m => m.Code == domianCfg.Code))
            {
                log.Debug("appdomain appid {0}，code {1} 不存在对应的配置，无法创建Subscribe", domianCfg.AppId, domianCfg.Code);
                return;
            }
            var subscribeName = _Utils.CreateDomainName(domianCfg.AppId, domianCfg.Code);
            //ThreadPool.QueueUserWorkItem(o =>
            //{
            using (var monitor = new MethodMonitor(log, 1, "Start {0}".Fomart(subscribeName)))
            {
                try
                {
                    subscribeProxy.CreateSubscribe(subscribeName, domianCfg.AppId, domianCfg.Code)
                                  .Start();
                }
                catch (Exception ex)
                {
                    subscribeProxy.RemoveSubcribe(subscribeName);
                    log.Error("CreateSubscribe Exception appid :{0},code:{1},ex:{2}", domianCfg.AppId, domianCfg.Code, ex.ToString());
                }
            }
            // }
            //, null);
        }
        //配置便更重新构建domain
        private static void ReBuilderAppDomain(Dictionary<string, AppdomainConfiguration> _domainList)
        {
            //根据状态卸载appdomain           
            log.Debug("检测到配置变更，执行订阅更新操作,当前订阅实例 {0} 个,获取到 {1} 个", subscribeProxy.SuscribeInstanceCount, _domainList.Count);
            var domainList = _domainList.Where(e => (e.Value.Host.IsEmpty() || _Utils.IsOwnerCurrentHost(e.Value.Host))).SafeToDictionary(e => e.Key, e => e.Value);
            var logString = new StringBuilder();
            domainList.Values.Select(e => e).SelectMany(e => e.Items).Where(e => e._Status == DomainAction.Remove).EachAction(_d =>
            {
                var domainName = _Utils.CreateDomainName(_d.AppId, _d.Code);
                subscribeProxy.StopSubcribe(domainName);
                logString.AppendFormat("{0},", domainName);
            });
            if (!string.IsNullOrEmpty(logString.ToString()))
                log.Debug("根据状态需要卸载 {0} 个订阅实例", logString.ToString());
            //
            //检查是否有新增的appdomain 
            var addNewDomainNameArray = CheckNeedCreateSubsceibeSettings(domainList, MQMainConfigurationManager.Builder.GetConfiguration());
            log.Debug("配置变更后 {0}", string.Join(",", addNewDomainNameArray));
            if (!addNewDomainNameArray.IsNull() && addNewDomainNameArray.Count() > 0)
            {
                log.Debug("需要新创建 {0} 个appdomain", addNewDomainNameArray.Count());
                addNewDomainNameArray.EachAction(domain =>
                {
                    subscribeProxy.CreateSubscribe(_Utils.CreateDomainName(domain.Item1, domain.Item2)
                                                     , exeAssembly
                                                     , typeName
                                                     , new object[] { domain.Item1, domain.Item2 })
                                                     .Start();
                });
                log.Debug("current domain count {0}", subscribeProxy.SuscribeInstanceCount);
            }
        }
        private static List<Tuple<string, string>> CheckNeedCreateSubsceibeSettings(Dictionary<string, AppdomainConfiguration> domainList
            , Dictionary<string, MQMainConfiguration> cfglist)
        {
            var domainNameArray = new List<Tuple<string, string>>();

            domainList.EachAction(d =>
            {
                d.Value.Items.Where(_i => _i._Status == DomainAction.Normal).EachAction(_d =>
                {
                    MQMainConfiguration _cfg;
                    if ((_cfg = cfglist.TryGetVal(_d.AppId)) != null)
                    {
                        if (_cfg.MessageCfgList.Any(c => c.Code == _d.Code))
                        {
                            var domainName = _Utils.CreateDomainName(_d.AppId, _d.Code);
                            if (!subscribeProxy.AllSubscribeNames.Contains(domainName))
                                domainNameArray.Add(Tuple.Create(_d.AppId, _d.Code));
                        }
                        else
                        {
                            log.Error("domain Cfg: appid {0} code {1} 配置不存在对应MQ配置,无法创建", _d.AppId, _d.Code);
                        }
                    }
                });
            });
            return domainNameArray;
        }
    }
}
