using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using YmatouMQ.Common;

namespace YmatouMQ.Log
{
    [Serializable]
    public class ConsoleLog : ILog
    {
        private string fullName;
        public ConsoleLog()
            : this(null)
        {

        }
        public ConsoleLog(string fullName)
        {
            this.fullName = fullName;
        }

        public void Debug(string s)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("D#{0} threadid {1} fullName {2} message {3}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff"), Thread.CurrentThread.ManagedThreadId, fullName, s);
            Console.ResetColor();
        }

        public void Debug(string format, params object[] args)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            var msg = string.Format(format, args);
            Console.WriteLine("D#{0} threadid {1} fullName {2} message {3}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff"), Thread.CurrentThread.ManagedThreadId, fullName, msg);
            Console.ResetColor();
        }

        public void Info(string s)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("I#{0} threadid {1} fullName {2} message {3}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff"), Thread.CurrentThread.ManagedThreadId, fullName, s);
            Console.ResetColor();
        }

        public void Info(string format, params object[] args)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            var msg = string.Format(format, args);
            Console.WriteLine("I#{0} threadid {1} fullName {2} message {3}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff"), Thread.CurrentThread.ManagedThreadId, fullName, msg);
            Console.ResetColor();
        }

        public void Warning(string s)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(s);
            Console.ResetColor();
        }
        public void Warning(string format, params object[] args)
        {
            var msg = string.Format(format, args);
            Console.WriteLine("I#{0} threadid {1} fullName {2} message {3}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff"), Thread.CurrentThread.ManagedThreadId, fullName, msg);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(msg);
            Console.ResetColor();
        }
        public void Warning(string s, Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(string.Format("{0},{1}", s, ex.ToString()));
            Console.ResetColor();
        }

        public void Error(string s)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(s);
            Console.ResetColor();
        }

        public void Error(string message, Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(string.Format("{0},{1}", message, ex.ToString()));
            Console.ResetColor();
        }

        public void Error(string format, params object[] args)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(format, args);
            Console.ResetColor();
        }

        public void Fatal(string s, Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(string.Format("{0}{1}", s, ex.ToString()));
            Console.ResetColor();
        }

        public void Fatal(string s)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(s);
            Console.ResetColor();
        }

        public void Fatal(string format, object[] args)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(format, args);
            Console.ResetColor();
        }


        public void Error2(string appid, string s)
        {
            throw new NotImplementedException();
        }

        public void Error2(string appid, string message, Exception ex)
        {
            throw new NotImplementedException();
        }

        public void Error2(string appid, string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Error2(string[] appid, string s)
        {
            //Ymatou.CommonService.ApplicationLog.Error(appid, s);
        }

        public void Error2(string[] appid, string message, Exception ex)
        {
            //Ymatou.CommonService.ApplicationLog.Error(appid, message, ex);
        }

        public void Error2(string[] appid, string format, params object[] args)
        {
            //Ymatou.CommonService.ApplicationLog.Error(appid, format.Fomart(args));
        }
    }
}
