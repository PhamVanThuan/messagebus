using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using YmatouMessageBusClientNet4.Extensions;
using Ymatou.CommonService;
using System.IO;
using System.Threading;

namespace YmatouMessageBusClientNet4
{
    internal class WebRequestWrap
    {
        public const string Content_Json = "application/json; charset=UTF-8";
        [ThreadStatic]
        private static WebRequestWrap webRequestWarp;
        public static WebRequestWrap Builder { get { if (webRequestWarp == null)webRequestWarp = new WebRequestWrap(); return webRequestWarp; } }

        public static void SetConnectionLimit(int limit = 32767)
        {
            //设置并发链接数
            ServicePointManager.DefaultConnectionLimit = limit;
        }

        public Task PostAsync(string uri, string context, byte[] data, int timeOut)
        {
            var request = (HttpWebRequest)WebRequest.Create(uri);
            //request.AllowWriteStreamBuffering = false;
            request.ServicePoint.UseNagleAlgorithm = false;
            request.ServicePoint.ConnectionLimit = UInt16.MaxValue;
            //request.ServicePoint.Expect100Continue = false;
            //request.ServicePoint.ConnectionLimit = UInt16.MaxValue;
            //request.ContentLength = data.Length;
            request.Method = "POST";
            request.ContentType = context;
            request.Timeout = timeOut;
            return request
                     .GetRequestStreamAsync()
                     .ContinueWith(r =>
                     {
                         if (r.Exception != null)
                         {
                             throw r.Exception;
                         }
                         else
                         {
                             r.Result.Write(data, 0, data.Length);
                         }
                     }).ContinueWith(r =>
                     {
                         if (r.Exception != null) throw r.Exception;
                         else
                         {
                             request.GetResponseAsync().ContinueWith(response =>
                             {
                                 if (response.Exception != null)
                                 {
                                     throw response.Exception;
                                 }
                                 else
                                 {
                                     var _response = (HttpWebResponse)response.Result;
                                     try
                                     {
                                         var responseCode = (int)(((HttpWebResponse)_response).StatusCode);
                                         if (responseCode != 200)
                                             throw new WebException("response error {0}".F(responseCode));

                                         using (var streamReader = new StreamReader(_response.GetResponseStream()))
                                         {
                                             if (!streamReader.ReadToEnd().StartsWith("Code=200")) throw new Exception();
                                         }
                                     }
                                     finally
                                     {
                                         AbortRequest(request, _response);
                                     }
                                 }
                             });
                         }
                     });
        }

        public Task PostAsync(string url, string contentType, byte[] data, int requestTimeOut, Action<HttpWebRequestCallbackState> responseCallback, object state = null, int httpConnectionLimit = 500)
        {
            var httpWebRequest = CreateHttpWebRequest(url, "POST", contentType);
            httpWebRequest.Timeout = requestTimeOut;
            httpWebRequest.ContentLength = data.Length;
            httpWebRequest.ServicePoint.UseNagleAlgorithm = false;
            httpWebRequest.ServicePoint.ConnectionLimit = httpConnectionLimit;

            var asyncState = new HttpWebRequestAsyncState()
            {
                RequestBytes = data,
                HttpWebRequest = httpWebRequest,
                ResponseCallback = responseCallback,
                State = state
            };

            return Task.Factory.FromAsync<Stream>(httpWebRequest.BeginGetRequestStream, httpWebRequest.EndGetRequestStream, asyncState, TaskCreationOptions.None)
                    .ContinueWith<HttpWebRequestAsyncState>(task =>
                    {
                        var asyncState2 = (HttpWebRequestAsyncState)task.AsyncState;
                        using (var requestStream = task.Result)
                        {
                            requestStream.Write(asyncState2.RequestBytes, 0, asyncState2.RequestBytes.Length);
                        }
                        return asyncState2;
                    })
                    .ContinueWith(task =>
                    {
                        var httpWebRequestAsyncState2 = (HttpWebRequestAsyncState)task.Result;
                        var hwr2 = httpWebRequestAsyncState2.HttpWebRequest;
                        Task.Factory.FromAsync<WebResponse>(hwr2.BeginGetResponse, hwr2.EndGetResponse, httpWebRequestAsyncState2, TaskCreationOptions.None)
                        .ContinueWith(task2 =>
                        {
                            WebResponse webResponse = null;
                            Stream responseStream = null;
                            try
                            {
                                var asyncState3 = (HttpWebRequestAsyncState)task2.AsyncState;
                                webResponse = task2.Result;
                                var responseCode = (int)(((HttpWebResponse)webResponse).StatusCode);
                                if (responseCode != 200)
                                    throw new WebException("response error {0}".F(responseCode));
                                responseStream = webResponse.GetResponseStream();
                                responseCallback(new HttpWebRequestCallbackState(responseStream, asyncState3));
                            }
                            catch (Exception ex)
                            {
                                responseCallback(new HttpWebRequestCallbackState(ex));
                            }
                            finally
                            {
                                if (responseStream != null)
                                    responseStream.Close();
                                if (webResponse != null)
                                    webResponse.Close();
                            }
                        });
                    });
        }

        public string Post(string uri, string context, byte[] data, int requestTimeOut = 3000, string responseEncoding = "utf-8", Action<WebException> requestError = null, Action<Stream> responseAction = null, int httpConnectionLimit = 500)
        {
            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = "POST";
            request.ContentType = context;
            request.Timeout = requestTimeOut;
            request.ServicePoint.UseNagleAlgorithm = false;
            request.ServicePoint.ConnectionLimit = httpConnectionLimit;
            WebResponse response = null;
            Stream responseStream = null;
            try
            {

                using (var requestStream = request.GetRequestStream())
                {
                    requestStream.Write(data, 0, data.Length);
                    using (response = request.GetResponse())
                    {
                        EnsureRequestSuccess(response);
                        responseStream = response.GetResponseStream();
                        if (responseAction != null)
                            responseAction(responseStream);
                        response.Close();
                        return "ok";
                    }
                }
            }
            catch (WebException ex)
            {
                //AbortRequest(request, response);
                requestError(ex);
                return "fail";
            }
            finally
            {
                if (responseStream != null)
                    responseStream.Close();
                AbortRequest(request, response);
            }
        }
        private static HttpWebRequest CreateHttpWebRequest(string url, string httpMethod, string contentType)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.ContentType = contentType;
            httpWebRequest.Method = httpMethod;
            return httpWebRequest;
        }

        private static void AbortRequest(HttpWebRequest request, WebResponse response)
        {
            try
            {
                if (request != null)
                    request.Abort();
                if (response != null)
                    response = null;
            }
            catch
            { }
        }

        private void EnsureRequestSuccess(WebResponse response)
        {
            var responseCode = (int)(((HttpWebResponse)response).StatusCode);
            if (responseCode != 200)
                throw new WebException("response error {0}".F(responseCode));
        }
        #region
        //private string TryReadResponse(string responseEncoding, Action<WebException, int> responseError = null)
        //{
        //    int responseCode = 500;
        //    try
        //    {
        //        using (response = request.GetResponse())
        //        {
        //            EnsureRequestSuccess(response);
        //            response.Close();
        //            return "ok";
        //        }
        //    }
        //    catch (WebException ex)
        //    {
        //        if (responseError != null)
        //            responseError(ex, responseCode);

        //        return "fail";
        //    }
        //    finally
        //    {
        //        CloseRequest();
        //    }
        //}
        //private bool TrySendRequest(byte[] data, Action<WebException> requestError = null)
        //{
        //    try
        //    {
        //        using (var requestStream = request.GetRequestStream())
        //        {
        //            requestStream.Write(data, 0, data.Length);
        //        }
        //        return true;
        //    }
        //    catch (WebException ex)
        //    {
        //        if (requestError != null)
        //            requestError(ex);
        //        return false;
        //    }
        //}
        #endregion
        private WebRequestWrap()
        { }
    }
    class HttpWebRequestCallbackState
    {
        public Stream ResponseStream { get; private set; }
        public Exception Exception { get; private set; }
        public Object State { get; set; }

        public HttpWebRequestCallbackState(Stream responseStream, object state)
        {
            ResponseStream = responseStream;
            State = state;
        }

        public HttpWebRequestCallbackState(Exception exception)
        {
            Exception = exception;
        }
    }
    class HttpWebRequestAsyncState
    {
        public byte[] RequestBytes { get; set; }
        public HttpWebRequest HttpWebRequest { get; set; }
        public Action<HttpWebRequestCallbackState> ResponseCallback { get; set; }
        public Object State { get; set; }
    }
}
