using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using ServiceStack;
using ServiceStack.Text;
using YmatouMessageBusClientNet4;
using YmatouMessageBusClientNet4.Dto;

namespace YmqtouMQConsumeDemo.Web
{
    public class PublishMessage : IHttpHandler
    {
        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            Message msg = null;
            using (var stream = context.Request.InputStream)
            {
                msg = JsonSerializer.DeserializeFromStream<Message>(stream);
            }

            var watch = Stopwatch.StartNew();
            Ymatou.CommonService.ApplicationLog.Debug("发送消息  个" + msg.Num);
            for (var i = 0; i < msg.Num; i++)
            {
                var mid = i + "_"+ Guid.NewGuid().ToString("N");
                MessageBusAgent.Publish(new PulbishMessageDto
                {
                    appid = msg.AppId,//必填
                    code = msg.Code, //必填
                    messageid = mid, //消息Id建议填写
                    body = new { _mid = mid, c = Newtonsoft.Json.JsonConvert.DeserializeObject(msg.Body.ToString().Replace("\n", "")) },//消息正文// new { action = "publishNote", userId = 1, noteId = 5, version = "20150910", addTime = 12423434 },
                    requestpath = System.Configuration.ConfigurationManager.AppSettings["path"] ?? "bus/Message/publish/"//web api //请求路径（可选）
                }, errorHandle: err =>
                {
                    context.Response.AddHeader("Context-Type", "application/json");
                    context.Response.Write(err.ToString().ToJson());
                });

            }

            var total = watch.ElapsedMilliseconds;
            watch.Stop();
            var ops = msg.Num * 1000 / (total > 0 ? total : 1);
            var outStr = string.Format("发送完成，耗时 {0} 毫秒,每秒 发送 {1}个消息", total, ops).ToJson();
            context.Response.AddHeader("Context-Type", "application/json");
            context.Response.Write(outStr);
        }
    }
    class HttpHelp
    {
        public static void Post(byte[] by)
        {
            var host = System.Configuration.ConfigurationManager.AppSettings["MQBusServerHost"] ?? "http://api.mq.ymatou.com:2345/bus/Message/";
            // Ymatou.CommonService.ApplicationLog.Debug("mq server host " + host);
            var webRequest = WebRequest.Create(host + System.Configuration.ConfigurationManager.AppSettings["path"]);//
            webRequest.Method = "POST";
            webRequest.ContentType = "application/json; charset=UTF-8";

            var task = webRequest.GetRequestStreamAsync().ContinueWith(r =>
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
              })
              .ContinueWith(r => Ymatou.CommonService.ApplicationLog.Error("发送消息异常 ", r.Exception), TaskContinuationOptions.OnlyOnFaulted);

            //try
            //{
            //    task.Wait(CancellationToken.None);
            //}
            //catch(AggregateException ex )
            //{
            //    Ymatou.CommonService.ApplicationLog.Error("发送消息异常2 "+ ex.ToString());
            //}
        }
    }
    class Message
    {
        public string AppId { get; set; }
        public string Code { get; set; }
        public object Body { get; set; }
        public int Num { get; set; }
        public bool UseWebClient { get; set; }
    }
    public class MessageDto
    {
        public string AppId { get; set; }
        public string Code { get; set; }
        public string Ip { get; set; }
        public string MsgUniqueId { get; set; }
        public object Body { get; set; }
    }
}