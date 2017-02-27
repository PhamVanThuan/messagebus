//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.IO;
//using System.Threading.Tasks;
//using System.Net;
//using System.Threading;
//using YmatouMQ.Common.Extensions._Task;
//using YmatouMQ.Common.Extensions;
//using YmatouMQ.Common.Extensions.Serialization;
//using YmatouMQ.Common.Utils;
//using YmatouMQ.Common;
//using YmatouMQNet4.Utils;
//using YmatouMQ.Log;

//namespace YmatouMQNet4.Configuration
//{
//    /// <summary>
//    /// mq appdomain 配置管理
//    /// </summary>
//    public class AppdomainConfigurationManager
//    {
//        private static readonly string file_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "{0}.domain.dump.Config".Fomart(AppDomain.CurrentDomain.FriendlyName.Replace(":", "")));
//        private static readonly Lazy<AppdomainConfigurationManager> lazy = new Lazy<AppdomainConfigurationManager>(() => new AppdomainConfigurationManager());
//        private readonly ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
//        private readonly ILog log = LogFactory.GetLogger(LogEngineType.RealtimelWriteFile, "YmatouMQNet4.Configuration.AppdomainConfigurationManager");
//        private readonly object olock = new object();
//        private readonly CancellationTokenSource cts = new CancellationTokenSource();
//        private Action<Dictionary<string, AppdomainConfiguration>> callback;
//        private Dictionary<string, AppdomainConfiguration> cache_appdomain_cfg = null;
//        //private Timer timer;
//        private Thread syncThread;
//        private bool runing;

//        public static AppdomainConfigurationManager Builder { get { return lazy.Value; } }

//        private AppdomainConfigurationManager()
//        {
//            InitAppdomainCfgSyncWork();
//        }
//        public void RegisterUpdateCallback(Action<Dictionary<string, AppdomainConfiguration>> callback)
//        {
//            this.callback = callback;
//        }
//        public Dictionary<string, AppdomainConfiguration> GetAllAppdomain()
//        {
//            YmtSystemAssert.AssertArgumentNotNull(cache_appdomain_cfg, "AppdomainConfiguration 为空");
//            return cache_appdomain_cfg;
//        }
//        public DomainItem GetAppDomain(string appid, string code)
//        {
//            YmtSystemAssert.AssertArgumentNotEmpty(appid, "domainName 不能为空");
//            YmtSystemAssert.AssertArgumentNotEmpty(code, "code 不能为空");
//            var cfg = cache_appdomain_cfg.TryGetVal("{0}.{1}".Fomart(appid, code));
//            YmtSystemAssert.AssertArgumentNotNull(cfg, "appid:{0},code:{1} 不存在".Fomart(appid, code));
//            var domainInfo = cfg.Items.SingleOrDefault(i => i.AppId == appid && i.Code == code);
//            YmtSystemAssert.AssertArgumentNotNull(domainInfo, "appid:{0},code:{1} 不存在".Fomart(appid, code));
//            return domainInfo;
//        }
//        public void Start()
//        {
//            if (runing) return;
//            runing = true;
//            #region
//            //timer = new Timer(o =>
//            //{
//            //    if (!runing) return;
//            //    try
//            //    {
//            //        //firstSyncAppDomainCfg =true 第一次同步配置全部获取配置 
//            //        AppdomainCfgSyncWork();
//            //        //firstSyncAppDomainCfg = false;
//            //    }
//            //    catch (Exception ex)
//            //    {
//            //        log.Error("app domain cfg 配置维护异常 ", ex);
//            //    }
//            //    timer.Change("appDomainCfgFulshTime".GetAppSettings(val => Convert.ToInt32(val), 30000), Timeout.Infinite);
//            //}, null, Timeout.Infinite, Timeout.Infinite);

//            //timer.Change("appDomainCfgFulshTime".GetAppSettings(val => Convert.ToInt32(val), 30000), Timeout.Infinite);
//            #endregion
//            syncThread = new Thread(o =>
//            {
//                while (runing)
//                {
//                    Thread.Sleep("appDomainCfgFulshTime".GetAppSettings(val => Convert.ToInt32(val), 30000));
//                    using (var monitor = new MethodMonitor(log, 1000, "appdomian sync "))
//                    {
//                        TrySyncAppdomainCfg();
//                        log.Info("sync appdomain cfg end,status ok.run {0} ms", monitor.GetRunTime.TotalMilliseconds);
//                    }
//                }
//            }) { IsBackground = true };
//            syncThread.Start();
//            log.Info("MQ app domain 配置维护启动成功,{0} 毫秒同步一次配置", "appDomainCfgFulshTime".GetAppSettings(val => Convert.ToInt32(val), 30000));
//        }

//        private void TrySyncAppdomainCfg()
//        {
//            try
//            {
//                AppdomainCfgSyncWork();
//            }
//            catch (AggregateException ex)
//            {
//                log.Error("app domain cfg 配置维护异常(AggregateException) ", ex);
//            }
//            catch (Exception ex)
//            {
//                log.Error("app domain cfg 配置维护异常(Exception) ", ex);
//            }
//        }
//        public void Stop()
//        {
//            runing = false;
//            //timer.Dispose();
//            log.Debug("app domain cfg service stop success");
//        }
//        private void InitAppdomainCfgSyncWork()
//        {
//            var cfgString = LoadLocalConfiguration(file_path);
//            var cfgInfo = cfgString.JSONDeserializeFromString<Dictionary<string, AppdomainConfiguration>>();
//            if (cfgInfo != null && cfgInfo.Any())
//            {
//                cache_appdomain_cfg = cfgInfo;
//                log.Info("InitAppdomainCfgSyncWork load appAomainCfg count {0},Adapter {1}", cfgInfo.Count(), cache_appdomain_cfg.Count);
//            }
//            else
//            {
//                AppdomainCfgSyncWork(true);
//            }
//        }
//        private void AppdomainCfgSyncWork(bool isCtor = false)
//        {
//            var cfg = LoadAppdomainCfg();
//            YmtSystemAssert.AssertArgumentNotNull(cfg.Item1, "无法获取到配置");
//            //如果是构造函数，则执行初始化
//            if (isCtor)
//            {
//                cache_appdomain_cfg = cfg.Item1;
//                if (cfg.Item2 == CfgTypeEnum.Server)
//                {
//                    DumpMQConfigurationFile(cfg.Item1.JSONSerializationToString());
//                }
//            }
//            else
//            {
//                if (cache_appdomain_cfg == null || !cache_appdomain_cfg.Any())
//                {
//                    cache_appdomain_cfg = cfg.Item1;
//                }
//                //如果是服务段配置则进行版本比较
//                if (cfg.Item2 == CfgTypeEnum.Server && CfgVersionCompare(cfg.Item1, cache_appdomain_cfg))
//                {
//                    Task.Factory.StartNew(() => DumpMQConfigurationFile(cfg.Item1.JSONSerializationToString()));
//                    UpdateCacheCfg(cfg.Item1);
//                    if (callback != null)
//                        callback(cfg.Item1);
//                }
//            }
//        }
//        private void UpdateCacheCfg(Dictionary<string, AppdomainConfiguration> newCfg)
//        {
//            rwLock.EnterUpgradeableReadLock();
//            try
//            {
//                rwLock.EnterWriteLock();
//                try
//                {
//                    //更新配置                                  
//                    cache_appdomain_cfg = newCfg;
//                    log.Info("获取到新配置,覆盖内存中配置,更新完成");
//                }
//                finally
//                {
//                    rwLock.ExitWriteLock();
//                }
//            }
//            finally
//            {
//                rwLock.ExitUpgradeableReadLock();
//            }
//        }
//        private bool CfgVersionCompare(Dictionary<string, AppdomainConfiguration> _serverCfg, Dictionary<string, AppdomainConfiguration> localCfg)
//        {
//            //服务端配置项大于缓存配置项，或者服务端配置版本大于缓存版本
//            return _serverCfg.Count != localCfg.Count || _serverCfg.Values.Where(s_cfg => localCfg.Values.Where(c_cfg => s_cfg.DomainName == c_cfg.DomainName && s_cfg.Version > c_cfg.Version).Any()).Any();
//        }
//        private void DumpMQConfigurationFile(string cfgInfo)
//        {
//            if (cfgInfo == null) return;
//            log.Debug("dump 远程获取的配置到本地磁盘");
//            lock (olock)
//            {
//                if (File.Exists(file_path)) File.Delete(file_path);
//                FileAsync.WriteAllText(file_path, cfgInfo).WithHandleException(log, null, "{0}", "dump appdomian cfg error");
//            }
//        }
//        public Tuple<Dictionary<string, AppdomainConfiguration>, CfgTypeEnum> LoadAppdomainCfg()
//        {
//            CfgTypeEnum cfgType = CfgTypeEnum.Server;
//            Tuple<Dictionary<string, AppdomainConfiguration>, CfgTypeEnum> result;
//            var url = CreateRequestUrl();
//            var cfgString = RequestMQConfigurationServer(url);
//            if (cfgString.IsEmpty())
//            {
//                log.Info("无法从配置服务获取MQ应用配置，使用本地{0}配置", file_path);
//                cfgString = LoadLocalConfiguration(file_path);
//                cfgType = CfgTypeEnum.LocalDisk;
//            }
//            if (cfgString.IsEmpty())
//            {
//                log.Info("本地磁盘无AppdomainConfiguration 配置使用，程序默认appdomain配置");
//                cfgType = CfgTypeEnum.LocalMemory;
//                result = Tuple.Create(Adapter(new[] { AppdomainConfiguration.DefaultAppdomainCfg }), cfgType);
//            }
//            else
//            {
//                var cfgInfo = cfgString.JSONDeserializeFromString<IEnumerable<AppdomainConfiguration>>();
//                log.Info("load app domain cfg,count  {0}", cfgInfo.Count());
//                result = Tuple.Create(Adapter(cfgInfo), cfgType);
//            }
//            return result;
//        }

//        private string CreateRequestUrl()
//        {
//            //从MQ配置中心，获取配置
//            var domainNames = AppDomain.CurrentDomain.FriendlyName.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

//            var appid = AppDomain.CurrentDomain.FriendlyName.StartsWith("ad.") && domainNames.Length > 2 ? domainNames[1] : null;
//            var code = AppDomain.CurrentDomain.FriendlyName.StartsWith("ad.") && domainNames.Length > 2 ? domainNames[2] : null;
//            var domainName = AppDomain.CurrentDomain.FriendlyName.StartsWith("ad.") ? "ad_{0}".Fomart(appid) : null;
//            var url = "{0}?domainName={1}&appid={2}&code={3}".Fomart(ConfigurationUri.domain_Cfg, domainName, appid, code);
//            return url;
//        }
//        private string LoadLocalConfiguration(string path)
//        {
//            if (!File.Exists(path))
//            {
//                log.Info("文件{0}不存在", path);
//                return string.Empty;
//            }
//            using (var fileStream = FileAsync.OpenRead(path))
//            using (var streamRead = new StreamReader(fileStream, Encoding.GetEncoding("utf-8")))
//            {
//                return streamRead.ReadToEnd();
//            }
//        }
//        private string RequestMQConfigurationServer(string url)
//        {
//            try
//            {
//                var request = (HttpWebRequest)WebRequest.Create(url);
//                request.Method = "GET";
//                request.Timeout = 5000;
//                request.ServicePoint.ConnectionLimit = UInt16.MaxValue;
//                request.ServicePoint.ReceiveBufferSize = 10000000;
//                return request
//                    .DownloadDataAsync(Encoding.GetEncoding("utf-8"), ex => log.Error("appdomaincfg DownloadDataAsync error {0}".Fomart(ex.ToString())))
//                    .GetResultSync(true, log, defReturn: string.Empty, descript: "执行 {0} 请求".Fomart(url));
//            }
//            catch (AggregateException ex)
//            {
//                log.Error("从配置服务获取appdomain配置异常0", ex);
//                return string.Empty;
//            }
//            catch (Exception ex)
//            {
//                log.Error("从配置服务获取appdomain配置异常1", ex);
//                return string.Empty;
//            }
//        }
//        private Dictionary<string, AppdomainConfiguration> Adapter(IEnumerable<AppdomainConfiguration> cfgList)
//        {
//            Dictionary<string, AppdomainConfiguration> dic = new Dictionary<string, AppdomainConfiguration>();
//            var t = cfgList.Where(e => e.Status == DomainAction.Normal).ToArray();
//            cfgList.Where(e => e.Status == DomainAction.Normal)
//                   .EachAction(i => i.Items.EachAction(sub => dic["{0}.{1}".Fomart(sub.AppId, sub.Code)] =
//                       new AppdomainConfiguration
//                       {
//                           AppId = sub.AppId,
//                           Code = sub.Code,
//                           Host = i.Host,
//                           DomainName = i.DomainName,
//                           Status = i.Status,
//                           Version = i.Version,
//                           Items = new List<DomainItem> { { new DomainItem { AppId = sub.AppId, Code = sub.Code, _Status = sub._Status, ConnectionPoolSize = sub.ConnectionPoolSize } } }
//                       }));
//            return dic;
//        }
//    }
//}
