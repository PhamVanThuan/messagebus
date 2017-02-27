using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YmatouMQ.Common;
using YmatouMQ.Common.Extensions;
using YmatouMQ.Common.Utils;
using YmatouMQ.Log;
using YmatouMQ.Subscribe;
using YmatouMQNet4.Configuration;
using YmatouMQNet4.Logs;
using YmatouMQNet4.Utils;
using YmatouMQSubscribe;
using YmatouMQ.Common.Extensions._Task;
using YmatouMQ.ConfigurationSync;

namespace YmatouMQ.SubscribeAppDomain
{
    [Serializable]
    public class MessageBusSubscribeSetup
    {
        private static readonly ILog log = LogFactory.GetLogger(LogFactory._LogType, "YmatouMQNet4._SubscribeSetup.__SubscribeSetup");
        private static readonly _YmatouMQAppdomainManager mm = new _YmatouMQAppdomainManager();
        private static readonly string typeName = typeof(_Subscribe).FullName;
        private static readonly string exeAssembly = typeof(_Subscribe).Assembly.FullName;

        public static void Start()
        {
            //初始化日志            
            InitLog4NetService();
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            //
            var task = Task.Factory.StartNew(() => AppdomainConfigurationManager.Builder.RegisterUpdateCallback(UpdateCallback))
                                    .ContinueWith(t => _privateStart(), TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously)
                                    .ContinueWith(t => MQMainConfigurationManager.Builder.Start(), TaskContinuationOptions.OnlyOnRanToCompletion)
                                    .ContinueWith(t => AppdomainConfigurationManager.Builder.Start(), TaskContinuationOptions.OnlyOnRanToCompletion)
                                    .WithHandleException(log, null, "消息总线推送服务启动异常{0}", "");

            // AppdomainConfigurationManager.Builder.RegisterUpdateCallback(UpdateCallback);
            //
            //_privateStart();
        }

        static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();
            log.Error("main appdomian TaskScheduler_UnobservedTaskException {0} {1}", sender, e.Exception.ToString());
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            log.Error("main appdomian UnhandledException {0}", e.ExceptionObject.ToString());
        }

        private static void InitLog4NetService()
        {
            log4net.GlobalContext.Properties["LogFileName"] = AppDomain.CurrentDomain.FriendlyName.Replace(":", "");
            log4net.GlobalContext.Properties["LogDirectory"] = AppDomain.CurrentDomain.FriendlyName.Replace(":", "");
            log4net.Config.XmlConfigurator.Configure(new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "log4net.config")));
        }

        private static void _privateStart()
        {
            var startMemory = GC.GetTotalMemory(false);
            log.Debug("MQ subscribe begin start ");
            var watch = Stopwatch.StartNew();
            var adCfgList = AppdomainConfigurationManager.Builder.GetAllAppdomain();
            adCfgList = adCfgList.Where(e => e.Value.Status == DomainAction.Normal
                //&& ((e.Value.AppId == "test2" && e.Value.Code == "liguo") || e.Value.AppId == "apisocial" && e.Value.Code == "feed")
                && (e.Value.Host.IsEmpty() || _Utils.IsOwnerCurrentHost(e.Value.Host))).ToDictionary(e => e.Key, e => e.Value);
            var domaincount = adCfgList.Sum(e => e.Value.Items.Count(_i => _i._Status == DomainAction.Normal));
            log.Debug("本机 {0} 需要创建 {1} 个appdomain,loadAppdomainCfg count {2}", _Utils.GetLocalHostIp(), domaincount, adCfgList.Count);
            //获取所有ＭＱ应用配置           
            var mqCfglist = MQMainConfigurationManager.Builder.GetConfiguration();
            //创建所有appdomain
            adCfgList
                .Select(e => e.Value)
                .SelectMany(e => e.Items)
                .Where(e => e._Status == DomainAction.Normal)
                .EachAction(e => TryCreateDomainAndStartAsync(mqCfglist, e));

            watch.Stop();
            var endMemory = GC.GetTotalMemory(false);
            log.Debug("MQ subscribe start success,create {0} domain,run time {1} ms,memory {2:N0} byte", mm.Domains.Count(), watch.ElapsedMilliseconds, (endMemory - startMemory));

        }

        private static void TryCreateDomainAndStartAsync(Dictionary<string, MQMainConfiguration> mqCfglist, DomainItem e)
        {
            try
            {
                ThreadPool.QueueUserWorkItem(o =>
                {
                    var appDomain = CreateAppdomain(mqCfglist, (DomainItem)o);
                    if (!string.IsNullOrEmpty(appDomain))
                        SubscribeStart(appDomain);
                }, e);
            }
            catch (Exception ex)
            {
                log.Error("创建domain 异常", ex);
            }
        }
        private static string CreateAppdomain(Dictionary<string, MQMainConfiguration> mqCfglist, DomainItem domain)
        {
            try
            {
                //校验配置&并创建appdomain
                MQMainConfiguration _cfg;
                if ((_cfg = mqCfglist.TryGetVal(domain.AppId)) != null)
                {
                    if (_cfg.MessageCfgList.Any(c => c.Code == domain.Code))
                    {
                        // var appdomainName = string.Format("ad.{0}.{1}", domain.AppId, domain.Code);
                        var appdomainName = _Utils.CreateDomainName(domain.AppId, domain.Code);
                        mm.CreateDomain(appdomainName, exeAssembly, typeName, new object[] { domain.AppId, domain.Code });
                        return appdomainName;
                    }
                    else
                    {
                        log.Error("appid {0} , code {1} 错误，不能创建appdomain", domain.AppId, domain.Code);
                    }
                }
                else
                {
                    log.Error("domain {0} ,appid {1} 配置在MQMainConfiguration不存在,不能创建appdomain", domain.AppId, domain.Code);
                }
            }
            catch (Exception ex)
            {
                log.Error("appdomain create fail。appid: {0},code:{1},err:{2}", domain.AppId, domain.Code, ex);
            }
            return null;
        }

        private static void SubscribeStart(string domainName)
        {
            try
            {
                using (var monitor = new MethodMonitor(log, 1, "Start {0}".Fomart(domainName)))
                {
                    var sub = (ISubscribe)mm.LoadDomainInstance(domainName);
                    if (sub != null) sub.Start();
                }
            }
            catch (Exception ex)
            {
                mm.TryUnLoadAppDomain(domainName);
                mm.RemoveAppDomain(domainName);
                log.Error("domain {0} start error ,{1}", domainName, ex);
            }
        }

        public static void Stop()
        {
            try
            {
                log.Debug("MQ subscribe begin stop ");
                var watch = Stopwatch.StartNew();
                mm.Domains.EachAction(e =>
                {
                    TrySubScribeStop(e);
                    mm.TryUnLoadAppDomain(e);
                });
                var domainCount = mm.Domains.Count();
                mm.ClearAllAppDomain();
                AppdomainConfigurationManager.Builder.Stop();
                MQMainConfigurationManager.Builder.Stop();
                watch.Stop();
                log.Debug("MQ subscribe stop success,UnLoadAppDomain {0} domain,run time {1:N0} ms", domainCount, watch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                log.Error("stop messagebus server errror ", ex);
            }
        }

        private static void TrySubScribeStop(string domainName)
        {
            try
            {
                var sub = (ISubscribe)mm.LoadDomainInstance(domainName);
                if (sub != null) sub.Stop();
            }
            catch (Exception ex)
            {
                log.Error("subscribe UnLoad fail {0},{1}", domainName, ex);
            }
        }
        private static void ReBuilderAppDomain(Dictionary<string, AppdomainConfiguration> _domainList)
        {
            //根据状态卸载appdomain           
            log.Debug("检测到配置变更，执行domain更新操作 current appdomain {0} 个,获取到appdomain {1} 个", mm.AppdomainCount, _domainList.Count);
            var domainList = _domainList.Where(e => (e.Value.Host.IsEmpty() || _Utils.IsOwnerCurrentHost(e.Value.Host))).SafeToDictionary(e => e.Key, e => e.Value);
            var logString = new StringBuilder();
            domainList.Values.Select(e => e).SelectMany(e => e.Items).Where(e => e._Status == DomainAction.Remove).EachAction(_d =>
            {
                var domainName = _Utils.CreateDomainName(_d.AppId, _d.Code);
                var sub = (ISubscribe)mm.LoadDomainInstance(domainName);
                if (sub != null) sub.Stop();
                mm.TryUnLoadAppDomain(domainName);
                mm.RemoveAppDomain(domainName);
                logString.AppendFormat("{0},", domainName);
            });
            if (!string.IsNullOrEmpty(logString.ToString()))
                log.Debug("根据appdomain状态需要卸载 {0} 个appdomain", logString.ToString());
            //
            //检查是否有新增的appdomain 
            var addNewDomainNameArray = CheckNeedCreateAppdomainSettings(domainList, MQMainConfigurationManager.Builder.GetConfiguration());
            log.Debug("配置变更后domainlist {0}", string.Join(",", addNewDomainNameArray));
            if (!addNewDomainNameArray.IsNull() && addNewDomainNameArray.Count() > 0)
            {
                log.Debug("需要新创建 {0} 个appdomain", addNewDomainNameArray.Count());
                addNewDomainNameArray.EachAction(domain =>
                {
                    var appid = domain.Split(new char[] { '.' })[1];
                    var code = domain.Split(new char[] { '.' })[2];
                    TryCreateDomainAndStart(domain, appid, code);
                });
                log.Debug("current domain count {0}", mm.Domains.Count());
            }
        }

        private static void TryCreateDomainAndStart(string domain, string appid, string code)
        {
            try
            {
                mm.CreateDomain(domain, exeAssembly, typeName, new object[] { appid, code });
                SubscribeStart(domain);
                log.Debug("create new appdomain {0} ok", domain);
            }
            catch (Exception ex)
            {
                log.Error("appdomain create fail mqappid: {0},err:{1}", domain, ex);
            }
        }

        //检查存在新建的appdomain
        private static List<string> CheckNeedCreateAppdomainSettings(Dictionary<string, AppdomainConfiguration> domainList, Dictionary<string, MQMainConfiguration> cfglist)
        {
            var domainNameArray = new List<string>();

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
                            if (!mm.Domains.Contains(domainName))
                                domainNameArray.Add(domainName);
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
        //检查存在卸载的appdomian
        private static List<string> CheckNeedRemoveAppdomainSettings(Dictionary<string, AppdomainConfiguration> domainList)
        {
            var domainNameArray = new List<string>();
            domainList.EachAction(d =>
            {
                d.Value.Items.EachAction(_d =>
                {
                    domainNameArray.Add(_Utils.CreateDomainName(_d.AppId, _d.Code));
                });
            });
            return domainNameArray;
        }       
        private static void UpdateCallback(Dictionary<string, AppdomainConfiguration> updateCallback)
        {
            ReBuilderAppDomain(updateCallback);
        }
    }
}
