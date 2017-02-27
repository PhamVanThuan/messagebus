using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace YmatouMessageBusClientNet4.Extensions
{
    public static class WebRequestExtensions_
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
                webRequest.BeginGetRequestStream, webRequest.EndGetRequestStream, webRequest /* object state for debugging */);
        }
        public static Task WithHandlerException(this Task task, Action<AggregateException> action = null)
        {
            if (task.Status == TaskStatus.RanToCompletion) return task;
            return task.ContinueWith(r =>
            {
                if (action != null) action(r.Exception);
            }, TaskContinuationOptions.OnlyOnFaulted /*| TaskContinuationOptions.ExecuteSynchronously*/);
        }
        public static Task WithHandler(this Task task, Action<AggregateException> errorAction, Action successAction)
        {
            //if (task.Status == TaskStatus.RanToCompletion) return task;
            return task.ContinueWith(r =>
            {
                if (r.Status == TaskStatus.Faulted || r.Status == TaskStatus.Canceled || r.Exception != null) errorAction(r.Exception);
                if (r.Status == TaskStatus.RanToCompletion) successAction();
            }/*, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously*/);
        }
        public static Task WithHandlerSuccess(this Task task, Action successAction = null)
        {
            return task.ContinueWith(r =>
            {
                if (successAction != null) successAction();
            }, TaskContinuationOptions.OnlyOnRanToCompletion /*| TaskContinuationOptions.ExecuteSynchronously*/);
        }
    }
}
