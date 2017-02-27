using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.IO;

namespace YmqtouMQConsumeDemo.Web
{
    public class ShowConsumeLog : IHttpHandler
    {
        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            //var str = new StringBuilder();
            //str.Append("<script type='text/javascript' src='http://mqdemo.ymatou.com/script/jquery-2.1.4.min.js'></script>");

            //str.Append("<script type='text/javascript'>");
            //str.Append("   setInterval($.get('http://mqdemo.ymatou.com/OrderHandle/', function (data){");
            //str.Append(" $('#info').html(data);");
            //str.Append(" })'), 3000);");
            //str.Append("</script>");

            //str.Append("<div id='info'>");

            //str.Append("</div>");
            //context.Response.Write(str);
            var logpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log", "info.txt");
            if (!File.Exists(logpath))
            {
                context.Response.Write(logpath + " log not exists..." + DateTime.Now);
                return;
            }
            var by = new byte[4096];
            using (var fs = new FileStream(logpath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var rs = new StreamReader(fs, Encoding.GetEncoding("gb2312")))
            {
                //var len = fs.Length - 1008;
                //if (len > 0)
                //    fs.Seek(len, SeekOrigin.Current);
                //int r = 0;
                //var end = (r = fs.Read(by, 0, by.Length));
                //{
                //    var _context = Encoding.GetEncoding("utf-8").GetString(by).Replace("\r\n", "</br>");
                //    var _context_formart = string.Format("当前时间：{0}</br>{1}", DateTime.Now, _context);
                //    context.Response.Write(_context_formart);
                //}
                var _context = rs.ReadToEnd().Replace("\r\n", "</br>");
                var _context_formart = string.Format("当前时间：{0}</br>{1}", DateTime.Now, _context);
                context.Response.ContentEncoding = Encoding.GetEncoding("utf-8"); 
                context.Response.Write(_context_formart);
            }
        }
    }
}