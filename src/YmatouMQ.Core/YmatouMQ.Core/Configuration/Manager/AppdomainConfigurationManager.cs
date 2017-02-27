using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using YmatouMQNet4.Extensions._Task;
using YmatouMQNet4.Extensions;
using YmatouMQNet4.Extensions.Serialization;
using YmatouMQNet4.Utils;

namespace YmatouMQNet4.Configuration
{
    /// <summary>
    /// mq appdomain 配置管理
    /// </summary>
    public class AppdomainConfigurationManager
    {
        private static readonly string file_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "YamtouMQAppdomain.dump.Config");
        private static readonly Lazy<AppdomainConfigurationManager> lazy = new Lazy<AppdomainConfigurationManager>(() => new AppdomainConfigurationManager(), true);
        private readonly ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        private readonly ILog log = LogFactory.GetLogger(LogEngineType.RealtimelWriteFile, "YmatouMQNet4.Configuration.AppdomainConfigurationManager");
        private readonly object olock = new object();
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private Action<Dictionary<string, AppdomainConfiguration>> callback;
        private Dictionary<string, AppdomainConfiguration> cache_appdomain_cfg = null;
        private Timer timer;
        private bool runing;

        public static AppdomainConfigurationManager Builder { get { return lazy.Value; } }

        private AppdomainConfigurationManager()
        {
            AppdomainCfgSyncWork(true);
        }
        public void RegisterUpdateCallback(Action<Dictionary<string, AppdomainConfiguration>> callback)
        {
            this.callback = callback;
        }
        public Dictionary<string, AppdomainConfiguration> GetAllAppdomain()
        {
            YmtSystemAssert.AssertArgumentNotNull(cache_appdomain_cfg, "AppdomainConfiguration 为空");
            return cache_appdomain_cfg;
        }
        public DomainItem GetAppDomain(string appid, string code)
        {
            YmtSystemAssert.AssertArgumentNotEmpty(appid, "domainName 不能为空");
            YmtSystemAssert.AssertArgumentNotEmpty(code, "code 不能为空");
            var cfg = cache_appdomain_cfg.TryGetVal("ad_{0}".Fomart(appid));
            YmtSystemAssert.AssertArgumentNotNull(cfg, string.Format("{0} 不存在", "{0}_{1}".Fomart(appid, code)));
            var domainInfo = cfg.Items.SingleOrDefault(i => i.AppId == appid && i.Code == code);
            YmtSystemAssert.AssertArgumentNotNull(domainInfo, string.Format("{0} 不存在", "{0}_{1}".Fomart(appid, code)));
            return domainInfo;
        }
        public void Start()
        {
            if (runing) return;
            runing = true;
            timer = new Timer(o =>
            {
                if (!runing) return;
                try
                {
                    AppdomainCfgSyncWork();
                }
                catch (Exception ex)
                {
                    log.Error("app domain cfg 配置维护异常 ", ex);
                }
                timer.Change("appDomainCfgFulshTime".GetAppSettings(val => Convert.ToInt32(val), 3000), Timeout.Infinite);
            }, null, Timeout.Infinite, Timeout.Infinite);

            timer.Change(0, Timeout.Infinite);
            log.Debug("MQ app domain 配置维护启动成功");
        }
        public void Stop()
        {
            runing = false;
            timer.Dispose();
        }
        private void AppdomainCfgSyncWork(bool isCtor = false)
        {
            var cfg = LoadAppdomainCfg();
            YmtSystemAssert.AssertArgumentNotNull(cfg.Item1, "无法获取到配置");
            //如果是构造函数，则执行初始化
            if (isCtor)
            {
                cache_appdomain_cfg = cfg.Item1;
                if (cfg.Item2 == CfgTypeEnum.Server)
                {
                    DumpMQConfigurationFile(cfg.Item1.JSONSerializationToString());
                }
            }
            else
            {
                if (cache_appdomain_cfg == null || !cache_appdomain_cfg.Any())
                {
                    cache_appdomain_cfg = cfg.Item1;
                }
                if (CfgVersionCompare(cfg.Item1, cache_appdomain_cfg))
                {
                    Task.Factory.StartNew(() => DumpMQConfigurationFile(cfg.Item1.JSONSerializationToString()));
                    UpdateCacheCfg(cfg.Item1);
                    if (callback != null)
                        callback(cfg.Item1);
                }
            }
        }
        private void UpdateCacheCfg(Dictionary<string, AppdomainConfiguration> newCfg)
        {
            rwLock.EnterUpgradeableReadLock();
            try
            {
                rwLock.EnterWriteLock();
                try
                {
                    //更新配置                                  
                    cache_appdomain_cfg = newCfg;
                    log.Debug("获取到新配置,覆盖内存中配置,更新完成");
                }
                finally
                {
                    rwLock.ExitWriteLock();
                }
            }
            finally
            {
                rwLock.ExitUpgradeableReadLock();
            }
        }
        private bool CfgVersionCompare(Dictionary<string, AppdomainConfiguration> _serverCfg, Dictionary<string, AppdomainConfiguration> localCfg)
        {
            //服务端配置项大于缓存配置项，或者服务端配置版本大于缓存版本
            return _serverCfg.Count != localCfg.Count || _serverCfg.Values.Where(s_cfg => localCfg.Values.Where(c_cfg => s_cfg.DomainName == c_cfg.DomainName && s_cfg.Version > c_cfg.Version).Any()).Any();
        }
        private void DumpMQConfigurationFile(string cfgInfo)
        {
            if (cfgInfo == null) return;
            log.Debug("dump 远程获取的配置到本地磁盘");
            lock (olock)
            {
                if (File.Exists(file_path)) File.Delete(file_path);
                FileAsync.WriteAllText(file_path, cfgInfo).GetResult(true);
            }
        }
        private Tuple<Dictionary<string, AppdomainConfiguration>, CfgTypeEnum> LoadAppdomainCfg()
        {
            CfgTypeEnum cfgType = CfgTypeEnum.Server;
            Tuple<Dictionary<string, AppdomainConfiguration>, CfgTypeEnum> result;
            var cfgString = RequestMQConfigurationServer(ConfigurationUri.domain_Cfg);
            if (cfgString.IsNull())
            {
                log.Debug("无法从配置服务获取MQ应用配置，使用本地{0}配置", file_path);
                cfgString = LoadLocalConfiguration(file_path);
                cfgType = CfgTypeEnum.LocalDisk;
            }
            if (cfgString.IsNull())
            {
                log.Debug("本地磁盘无AppdomainConfiguration 配置使用，程序默认appdomain配置");
                cfgType = CfgTypeEnum.LocalMemory;
                result = Tuple.Create(Adapter(new[] { AppdomainConfiguration.DefaultAppdomainCfg }), cfgType);
            }
            else
            {
                var cfgInfo = cfgString.JSONDeserializeFromString<IEnumerable<AppdomainConfiguration>>();
                log.Debug("加载到 {0} 个app domain cfg 配置", cfgInfo.Count());
                result = Tuple.Create(Adapter(cfgInfo), cfgType);
            }
            return result;
        }
        private string LoadLocalConfiguration(string path)
        {
            if (!File.Exists(path))
            {
                log.Error("文件{0}不存在", path);
                return null;
            }
            using (var fileStream = FileAsync.OpenRead(path))
            using (var streamRead = new StreamReader(fileStream, Encoding.GetEncoding("utf-8")))
            {
                return Task.Factory.StartNew(() => streamRead.ReadToEnd())
                        .ContinueWith(str => str.Result, TaskContinuationOptions.ExecuteSynchronously)
                        .GetResult(true, descript: "加载本地文件 {0}".Fomart(path));
            }
        }
        private string RequestMQConfigurationServer(string url)
        {
            var request = WebRequest.Create(url);
            request.Method = "GET";
            request.Timeout = 3000;
            return request.DownloadDataAsync(Encoding.GetEncoding("utf-8")).GetResult(true, descript: "执行 {0} 请求".Fomart(url));
        }
        private Dictionary<string, AppdomainConfiguration> Adapter(IEnumerable<AppdomainConfiguration> cfgList)
        {
            Dictionary<string, AppdomainConfiguration> dic = new Dictionary<string, AppdomainConfiguration>();
            cfgList.EachAction(i => dic[i.DomainName] = i);
            return dic;
        }
    }
}
