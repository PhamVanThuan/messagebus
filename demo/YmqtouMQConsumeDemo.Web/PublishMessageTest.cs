using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.Text;
using YmatouMessageBusClientNet4;
using YmatouMessageBusClientNet4.Dto;
using ServiceStack;
using System.Diagnostics;

namespace YmqtouMQConsumeDemo.Web
{
    public class PublishMessageTest : IHttpHandler
    {
        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            Message msg = new Message
            {
                AppId = "test2",
                Code = "liguo",
                Body = "a"
            };
            var watch = Stopwatch.StartNew();
            //方式1，声明 PulbishMessageDto 结构体
            MessageBusAgent.Publish(new PulbishMessageDto
            {
                appid = "test2",//必填
                code = "liguo", //必填
                messageid = Guid.NewGuid().ToString("N"), //消息Id建议填写
                body = new { a = "1" },//消息正文（能序列化的都支持）
                requestpath = System.Configuration.ConfigurationManager.AppSettings["path"] ?? "bus/Message/publish/"//web api //请求路径（可选）
            }, errorHandle: err =>
            {
                //TODO:记录日志或其他操作
                context.Response.AddHeader("Context-Type", "application/json");
                context.Response.Write(err.ToString().ToJson());
            });

            //方式2，使用参数列表
            MessageBusAgent.Publish("test2", "liguo", Guid.NewGuid().ToString("N"), new { a = "1" });

            var total = watch.ElapsedMilliseconds;
            watch.Stop();
            var ops = msg.Num * 1000 / (total > 0 ? total : 1);
            var outStr = string.Format("发送完成，耗时 {0} 毫秒,每秒 发送 {1}个消息", total, ops).ToJson();
            context.Response.AddHeader("Context-Type", "application/json");
            context.Response.Write(outStr);
        }
    }
}