using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace YmatouMQ.ClientNet45
{
    class _HttpClientFactory
    {
        private static readonly Dictionary<string, HttpClient> cache = new Dictionary<string, HttpClient>();
        private static SpinLock sLock = new SpinLock();
        
        internal static async Task PostAsync(string requestKey, string url, byte[] data, int timeOut
            , Action<_ResponseCallback> callback, string contextType = "application/json;charset=utf-8")
        {
            try
            {
                var httpClient = Factory(requestKey, contextType: contextType);
                var context = new ByteArrayContent(data);
                context.Headers.Add("Content-Type", "{0}".F(contextType));

                var response = await httpClient.PostAsync(url, context, new CancellationTokenSource(timeOut).Token).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                callback(new _ResponseCallback(responseStream, null));
            }
            catch (AggregateException ex)
            {
                callback(new _ResponseCallback(ex.InnerException));
            }
            catch (OperationCanceledException ex)
            {
                callback(new _ResponseCallback(ex.InnerException));
            }
            catch (Exception ex)
            {
                callback(new _ResponseCallback(ex.InnerException));
            }
        }

        internal static HttpClient Factory(string requestKey, int bufferSize = 1048576, string contextType = "application/json")
        {
            var key = "{0}_{1}".F(AppDomain.CurrentDomain.FriendlyName, requestKey);
            if (cache.ContainsKey(key))
            {
                return cache[key];
            }
            else
            {
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
                                httpClient = CreateHttpClient(bufferSize, contextType);
                                cache[key] = httpClient;
                                //log.Debug("create httpClient success {0}".Fomart(key));
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
                return httpClient;
            }
        }

        private static HttpClient CreateHttpClient(int bufferSize, string contextType)
        {
            var httpClient = new HttpClient();
            httpClient.MaxResponseContentBufferSize = bufferSize;
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(contextType/*"application/json"*/));
            httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("YmatouMQ", "1.0"));
            httpClient.DefaultRequestHeaders.Connection.Add("keep-alive");
            return httpClient;
        }
        public static void ClearHttpClient(string key)
        {
            var _key = "{0}_{1}".F(AppDomain.CurrentDomain.FriendlyName, key);
            if (cache.ContainsKey(_key))
            {
                cache[_key].Dispose();
                cache.Remove(_key);
                //log.Debug("释放 {0} httpClient ", key);
            }
        }
    }
    class _ResponseCallback
    {
        public Stream ResponseStream { get; private set; }
        public Exception Exception { get; private set; }
        public Object State { get; set; }
        public _ResponseCallback(Stream responseStream, object state)
        {
            ResponseStream = responseStream;
            State = state;
        }
        public _ResponseCallback(Exception exception)
        {
            Exception = exception;
        }
    }
}
