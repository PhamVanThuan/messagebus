using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YmatouMQ.Common;
using YmatouMQ.Common.Extensions;
using YmatouMQ.Log;

namespace YmatouMQ.MessageScheduler
{
    class _HttpClientFactory
    {
        private static readonly Dictionary<string, HttpClient> cache = new Dictionary<string, HttpClient>();
        private static readonly ILog log = LogFactory.GetLogger(LogEngineType.RealtimelWriteFile, "YmatouMQMessageHandlerScheduler._HttpClientFactory");
        private static SpinLock sLock = new SpinLock();

        internal static HttpClient Factory(int bufferSize, string contextType, string callbackKey, int? timeOut)
        {
            var key = "{0}_{1}".Fomart(AppDomain.CurrentDomain.FriendlyName, callbackKey);
            if (cache.ContainsKey(key))
            {
                return cache[key];
            }
            HttpClient httpClient = null;
            if (!cache.ContainsKey(key))
            {
                bool @lockToken = false;
                try
                {
                    sLock.TryEnter(2000, ref @lockToken);
                    if (@lockToken)
                    {
                        if (!cache.ContainsKey(key))
                        {
                            httpClient = CreateHttpClient(bufferSize, contextType, timeOut);
                            cache[key] = httpClient;
                            log.Debug("create httpClient success,cache key:{0}".Fomart(key));
                        }
                        else
                        {
                            httpClient = cache[key];
                        }
                    }
                }
                finally
                {
                    if (@lockToken) sLock.Exit();
                }
            }
            //再次检查防止httpClient为空
            if (httpClient == null)
            {
                httpClient = CreateHttpClient(bufferSize, contextType, timeOut);
                cache[key] = httpClient;
            }
            return httpClient;
        }

        private static HttpClient CreateHttpClient(int bufferSize, string contextType, int? timeOut)
        {
            var httpClient = new HttpClient();
            httpClient.MaxResponseContentBufferSize = bufferSize;
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(contextType/*"application/json"*/));
            httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("YmatouMQ", "1.0"));
            httpClient.DefaultRequestHeaders.Connection.Add("keep-alive");
            //timeOut.NullAction(v => httpClient.Timeout = TimeSpan.FromMilliseconds(v));
            return httpClient;
        }
        public static void ClearHttpClient(string key)
        {
            var _key = "{0}_{1}".Fomart(AppDomain.CurrentDomain.FriendlyName, key);
            if (cache.ContainsKey(_key)) 
            {
                cache[_key].Dispose();
                cache.Remove(_key);
                log.Debug("free httpClient,cache key:{0}", key);          
            }             
        }
    }
}
