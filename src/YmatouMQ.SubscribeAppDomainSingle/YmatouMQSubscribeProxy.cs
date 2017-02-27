using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using YmatouMQSubscribe;
using YmatouMQ.Subscribe;
using System.Runtime.Remoting.Activation;

namespace YmatouMQ.SubscribeAppDomainSingle
{
    class YmatouMQSubscribeProxy
    {
        private readonly ConcurrentDictionary<string, ISubscribe> subscribePool = new ConcurrentDictionary<string, ISubscribe>();
        public ISubscribe CreateSubscribe(string subscribeName, string assemblyName, string typeName, object[] ctorArgs)
        {
            if (!subscribePool.ContainsKey(subscribeName))
            {
                var subscribe = (ISubscribe)Activator.CreateInstance(typeof(_Subscribe), ctorArgs);
                subscribePool[subscribeName] = subscribe;
            }
            return subscribePool[subscribeName];
        }
        public ISubscribe CreateSubscribe(string subscribeName, string appid, string code)
        {
            if (!subscribePool.ContainsKey(subscribeName))
            {
                var subcribe = new _Subscribe(appid, code);
                subscribePool[subscribeName] = subcribe;
            }
            return subscribePool[subscribeName];
        }
        public ISubscribe this[string subscribeName]
        {
            get
            {
                ISubscribe sub;
                if (subscribePool.TryGetValue(subscribeName, out sub))
                {
                    return sub;
                }
                return null;
            }
        }
        public IEnumerable<string> AllSubscribeNames
        {
            get { return subscribePool.Keys; }
        }
        public void StopSubcribe(string subscribeName)
        {
            ISubscribe subInfo;
            if (subscribePool.TryRemove(subscribeName, out subInfo))
            {
                subInfo.Stop();
            }
        }
        public void StopSubcribeAll()
        {
            foreach (var subItem in subscribePool)
            {               
                subItem.Value.Stop();
            }
            subscribePool.Clear();
        }
        public void RemoveSubcribe(string subscribeName) 
        {
            ISubscribe subscribe;
            subscribePool.TryRemove(subscribeName, out subscribe);
        }
        public int SuscribeInstanceCount { get { return subscribePool.Count; } }
    }
}
