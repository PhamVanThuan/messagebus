using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace YmatouMQConsume.AppConsole
{
    [Serializable]
    public class YmatouMQAppdomainManager
    {
        [Serializable]
        class DomainInfo
        {
            public AppDomain domain { get; set; }
            public object instance { get; set; }
        }
        private readonly Dictionary<string, DomainInfo> adPool = new Dictionary<string, DomainInfo>();
        public void CreateDomain(string domainName, string assemblyName, string typeName, object[] ctorArgs)
        {
            if (adPool.ContainsKey(domainName)) return;
            var ads = new AppDomainSetup();
            ads.ApplicationBase = System.Environment.CurrentDirectory;
            ads.DisallowBindingRedirects = false;
            ads.DisallowCodeDownload = true;
            ads.ConfigurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;

            var adInfo = AppDomain.CreateDomain(domainName, null, null);
            adInfo.TypeResolve += adInfo_TypeResolve;
            adInfo.UnhandledException += adInfo_UnhandledException;
            var obj = adInfo.CreateInstanceAndUnwrap(assemblyName, typeName, false, BindingFlags.Default, null, ctorArgs, null, null);
            var domainInfo = new DomainInfo { domain = adInfo, instance = obj };
            adPool[domainName] = domainInfo;
        }

        void adInfo_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.ExceptionObject.ToString());
        }

        Assembly adInfo_TypeResolve(object sender, ResolveEventArgs args)
        {
            Console.WriteLine(args.Name);
            return args.RequestingAssembly;
        }

        public object LoadDomainInstance(string domainName)
        {
            DomainInfo domainInfo;
            if (adPool.TryGetValue(domainName, out domainInfo))
            {
                return domainInfo.instance;
            }
            return null;
        }
        public void UnLoadAppDomain()
        {
            foreach (var item in adPool)
                AppDomain.Unload(item.Value.domain);
            adPool.Clear();
        }
        public void UnLoadAppDomain(string domainName)
        {
            DomainInfo ad;
            if (adPool.TryGetValue(domainName, out ad))
            {
                AppDomain.Unload(ad.domain);
                adPool.Remove(domainName);
            }
        }
    }
}
