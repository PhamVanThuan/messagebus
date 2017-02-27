using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace YmatouMQAdminTest
{
    class HttpHelp
    {
        public static async Task Post(byte[] by, string requestPath)
        {
            var host = System.Configuration.ConfigurationManager.AppSettings["mqadmin"] ?? "http://mqadmin.ymatou.com/";
            var uri = string.Format("{0}/{1}", host, requestPath);
            var webRequest = WebRequest.Create(uri);//
            webRequest.Method = "POST";
            webRequest.ContentType = "application/json; charset=UTF-8";

            await webRequest.GetRequestStreamAsync().ContinueWith(r =>
             {
                 r.Result.Write(by, 0, by.Length);
             })
             .ContinueWith(_r =>
             {
                 webRequest.GetResponseAsync().ContinueWith(r =>
                 {
                     var stream = r.Result.GetResponseStream();
                     return new StreamReader(stream, Encoding.GetEncoding("utf-8"));
                 }, TaskContinuationOptions.OnlyOnRanToCompletion);
             }).ContinueWith(r =>
             {
                 if (r.IsFaulted)
                     Console.WriteLine(r.Exception.ToString());
             }, TaskContinuationOptions.NotOnFaulted);
        }

        public static void _Post(byte[] by, string requestPath)
        {
            var host = System.Configuration.ConfigurationManager.AppSettings["mqadmin"] ?? "http://mqadmin.ymatou.com/";
            var uri = string.Format("{0}/{1}", host, requestPath);
            var webRequest = WebRequest.Create(uri);//
            webRequest.Method = "POST";
            webRequest.ContentType = "application/json; charset=UTF-8";
            webRequest.GetRequestStream().Write(by, 0, by.Length);
            using (var stream = webRequest.GetResponse().GetResponseStream())
            using (var read = new StreamReader(stream))
            {
                Console.WriteLine(read.ReadToEnd());
            }
        }
    }
}
