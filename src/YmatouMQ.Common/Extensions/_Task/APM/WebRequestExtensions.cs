using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using System.Net;
using System;

namespace YmatouMQ.Common.Extensions._Task
{
    /// <summary>Extension methods for working with WebRequest asynchronously.</summary>
    public static class WebRequestExtensions
    {
        /// <summary>Creates a Task that represents an asynchronous request to GetResponse.</summary>
        /// <param name="webRequest">The WebRequest.</param>
        /// <returns>A Task containing the retrieved WebResponse.</returns>
        public static Task<WebResponse> GetResponseAsync(this WebRequest webRequest)
        {
            if (webRequest == null) throw new ArgumentNullException("webRequest");
            return Task<WebResponse>.Factory.FromAsync(
                webRequest.BeginGetResponse
                , webRequest.EndGetResponse
                , webRequest /* object state for debugging */);
        }

        /// <summary>Creates a Task that represents an asynchronous request to GetRequestStream . remark: Method set "POST"</summary>
        /// <param name="webRequest">The WebRequest.</param>
        /// <returns>A Task containing the retrieved Stream.</returns>
        public static Task<Stream> GetRequestStreamAsync(this WebRequest webRequest)
        {
            if (webRequest == null) throw new ArgumentNullException("webRequest");
            return Task<Stream>.Factory.FromAsync(
                webRequest.BeginGetRequestStream
                , webRequest.EndGetRequestStream
                , webRequest /* object state for debugging */);
        }

        /// <summary>Creates a Task that respresents downloading all of the data from a WebRequest.</summary>
        /// <param name="webRequest">The WebRequest.</param>
        /// <returns>A Task containing the downloaded content.</returns>
        public static Task<byte[]> DownloadDataAsync(this WebRequest webRequest)
        {
            // Asynchronously get the response.  When that's done, asynchronously read the contents.
            return webRequest.GetResponseAsync().ContinueWith(response =>
            {
                var stream = response.Result.GetResponseStream();
                var memorStream = new MemoryStream();
                //var streamRead = new StreamReader(stream, coding);

                Task.WaitAll(stream.CopyStreamIterator(memorStream).ToArray());

                return Task.Factory.StartNew(() => memorStream.ToArray());
            }, TaskContinuationOptions.OnlyOnRanToCompletion).Unwrap();
        }

        public static Task<string> DownloadDataAsync(this WebRequest webRequest, Encoding coding, Action<AggregateException> errorHandler = null)
        {
            // Asynchronously get the response.  When that's done, asynchronously read the contents.
            return webRequest
                    .GetResponseAsync()
                    .ContinueWith(response =>
                                {
                                    if (response.IsFaulted)
                                    {
                                        if (errorHandler != null)
                                            errorHandler(response.Exception);
                                        return string.Empty;
                                    }
                                    else
                                    {
                                        var result = string.Empty;
                                        using (var stream = response.Result.GetResponseStream())
                                        {
                                            var reader = new StreamReader(stream, coding);
                                            result = reader.ReadToEnd();
                                            response.Result.Close();
                                        }
                                        return result;
                                    }
                                });
        }

        public static Task Post(byte[] by, string uri, Action<AggregateException> errorHandle)
        {
            var webRequest = WebRequest.Create(uri);//
            webRequest.Method = "POST";
            webRequest.ContentType = "application/json; charset=UTF-8";

            return webRequest.GetRequestStreamAsync().ContinueWith(r =>
              {
                  r.Result.Write(by, 0, by.Length);
              }).ContinueWith(_r =>
              {
                  webRequest.GetResponseAsync().ContinueWith(r =>
                  {
                      var stream = r.Result.GetResponseStream();
                      return new StreamReader(stream, Encoding.GetEncoding("utf-8"));
                  }, TaskContinuationOptions.OnlyOnRanToCompletion);
              }).ContinueWith(r =>
              {
                  if (r.IsFaulted)
                      errorHandle(r.Exception);
              }, TaskContinuationOptions.NotOnFaulted);
        }
    }
}