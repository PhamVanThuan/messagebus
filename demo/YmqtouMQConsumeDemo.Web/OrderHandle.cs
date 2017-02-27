using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Text;
using ServiceStack.Text;
namespace YmqtouMQConsumeDemo.Web
{
    public class OrderHandle : IHttpHandler
    {
        public static readonly string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log", "mq.log");
        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            //using (var stream = context.Request.InputStream)
            //using (var rRead = new StreamReader(stream))
            //{
            //    var _context = rRead.ReadToEnd();
            //    //File.AppendAllText(logPath, string.Format("{0} 接收到消息 {1} \r\n", DateTime.Now, _context), Encoding.GetEncoding("utf-8"));
            //    //Ymatou.CommonService.ApplicationLog.Info(string.Format("接收到消息 {0}", _context));


            //}
            context.Response.AddHeader("Context-Type", "application/json");
            context.Response.Write("ok");
        }
    }


}