using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using YmatouMQ.Common;
using YmatouMQ.Log;
using YmatouMQ.Common.Extensions;
namespace YmatouMQ.SubscribeAppDomain
{
    [Serializable]
    public class _YmatouMQAppdomainManager
    {
        private readonly Dictionary<string, DomainInfo> adPool = new Dictionary<string, DomainInfo>();
        private readonly ILog log = LogFactory.GetLogger(LogFactory._LogType, "YmatouMQNet4._Appdomain._YmatouMQAppdomainManager");

        public _YmatouMQAppdomainManager()
        {

        }

        public void CreateDomain(string domainName, string assemblyName, string typeName, object[] ctorArgs)
        {
            if (string.IsNullOrEmpty(domainName))
            {
                log.Debug("domainName 为空不能创建domain;assemblyName: {0},typeName:{1}".Fomart(assemblyName, typeName));
                return;
            }
            if (adPool.ContainsKey(domainName)) return;

            var ads = new AppDomainSetup();
            ads.ConfigurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
            var adInfo = AppDomain.CreateDomain(domainName, null, ads);
            //adInfo.TypeResolve += adInfo_TypeResolve;
            adInfo.UnhandledException += adInfo_UnhandledException;
            System.Threading.Tasks.TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            var obj = adInfo.CreateInstanceAndUnwrap(assemblyName, typeName, false, BindingFlags.Default, null, ctorArgs, null, null);
            var domainInfo = new DomainInfo { domain = adInfo, instance = obj };
            adPool[domainName] = domainInfo;
            //log.Debug("domain 创建完成 {0}", domainName);
        }

        void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();
            log.Error("task 异常 {0} {1}", sender, e.Exception.ToString());
        }

        void adInfo_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            log.Error("adInfo_UnhandledException {0}", e.ExceptionObject.ToString());
        }

        //Assembly adInfo_TypeResolve(object sender, ResolveEventArgs args)
        //{
        //    log.Error("{0}->{1}", args.Name, args.RequestingAssembly.FullName);
        //    return args.RequestingAssembly;
        //}

        public object LoadDomainInstance(string domainName)
        {
            DomainInfo domainInfo;
            if (adPool.TryGetValue(domainName, out domainInfo))
            {
                return domainInfo.instance;
            }
            return null;
        }
        public void TryUnLoadAppDomain()
        {
            foreach (var item in adPool)
                TryUnLoadAppDomain(item.Key);
            ClearAllAppDomain();
        }
        public void TryUnLoadAppDomain(string domainName)
        {
            DomainInfo ad;
            if (adPool.TryGetValue(domainName, out ad))
            {
                try
                {
                    AppDomain.Unload(ad.domain);                 
                }
                catch (Exception ex)
                {
                    log.Error("domain {1} unload error {0}", domainName, ex);
                }

            }
        }
        public void RemoveAppDomain(string domainName)
        {
            adPool.Remove(domainName);
        }
        public void ClearAllAppDomain()
        {
            adPool.Clear();
        }
        public IEnumerable<string> Domains { get { return adPool.Keys; } }
        public int AppdomainCount { get { return adPool.Keys.Count; } }

        [Serializable]
        class DomainInfo
        {
            public AppDomain domain { get; set; }
            public object instance { get; set; }
        }
    }
}
