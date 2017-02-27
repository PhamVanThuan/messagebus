using System;
using System.Threading;
using YmatouMQ.Common.Utils;
using YmatouMQ.Common.Extensions;
using YmatouMQ.Common;

namespace YmatouMQNet4.Logs
{   
    public class GeneralFileLog : ILog
    {
        private readonly string fullName;
        public GeneralFileLog(string fullName)
        {
            this.fullName = fullName;
        }

        public void Debug(string s)
        {
           // if (!MQSystemConfiguration.GetMQSysConfiguration().DebugLogEnable) return;
            //LocalLogHelp.Write(FormatMessage(LogLevel.Debug, s));
            Ymatou.CommonService.ApplicationLog.Debug(s);
        }

        public void Debug(string format, params object[] args)
        {
           // if (!MQSystemConfiguration.GetMQSysConfiguration().DebugLogEnable) return;
            //LocalLogHelp.Write(FormatMessage(LogLevel.Debug, format, args));
            Ymatou.CommonService.ApplicationLog.Debug(format.Fomart(args));
        }

        public void Info(string s)
        {
            //if (!MQSystemConfiguration.GetMQSysConfiguration().InfoLogEnable) return;
            //LocalLogHelp.Write(FormatMessage(LogLevel.Info, s));
            Ymatou.CommonService.ApplicationLog.Info(s);
        }

        public void Info(string format, params object[] args)
        {
            //if (!MQSystemConfiguration.GetMQSysConfiguration().InfoLogEnable) return;
            //LocalLogHelp.Write(FormatMessage(LogLevel.Info, format, args));
            Ymatou.CommonService.ApplicationLog.Info(format.Fomart(args));
        }
        public void Warning(string format, params object[] args)
        {
           // if (!MQSystemConfiguration.GetMQSysConfiguration().InfoLogEnable) return;
            //LocalLogHelp.Write(FormatMessage(LogLevel.Warning, format, args));
            Ymatou.CommonService.ApplicationLog.Warn(format.Fomart(args));
        }
        public void Warning(string s)
        {
            //if (!MQSystemConfiguration.GetMQSysConfiguration().InfoLogEnable) return;
            //LocalLogHelp.Write(FormatMessage(LogLevel.Warning, s));
            Ymatou.CommonService.ApplicationLog.Warn(s);
        }

        public void Warning(string s, Exception ex)
        {
            //if (!MQSystemConfiguration.GetMQSysConfiguration().InfoLogEnable) return;
            //var msg = string.Format("msg->{0},exMessage->{1}", ex.ToString());
            //LocalLogHelp.Write(FormatMessage(LogLevel.Warning, msg));
            Ymatou.CommonService.ApplicationLog.Warn(s, ex);
        }

        public void Error(string s)
        {
            //if (!MQSystemConfiguration.GetMQSysConfiguration().ErrorLogEnable) return;
            // logger.Error(s);
            //LocalLogHelp.Write(FormatMessage(LogLevel.Error, s));
            Ymatou.CommonService.ApplicationLog.Error(s);
        }

        public void Error(string message, Exception ex)
        {
            //if (!MQSystemConfiguration.GetMQSysConfiguration().ErrorLogEnable) return;
            //var msg = string.Format("msg->{0},exMessage->{1}", ex.ToString());
            //LocalLogHelp.Write(FormatMessage(LogLevel.Error, msg));
            Ymatou.CommonService.ApplicationLog.Error(message, ex);
        }

        public void Error(string format, params object[] args)
        {
            //if (!MQSystemConfiguration.GetMQSysConfiguration().ErrorLogEnable) return;
            //var msg = string.Format(format, args);
            //// logger.ErrorFormat(format, args);
            //LocalLogHelp.Write(FormatMessage(LogLevel.Error, format, args));
            Ymatou.CommonService.ApplicationLog.Error(format.Fomart(args));
        }

        public void Fatal(string s, Exception ex)
        {
            //if (!MQSystemConfiguration.GetMQSysConfiguration().ErrorLogEnable) return;
            //var msg = string.Format("msg->{0},exMessage->{1}", ex.ToString());
            //// logger.Fatal(msg);
            //LocalLogHelp.Write(FormatMessage(LogLevel.Fatal, msg));
            Ymatou.CommonService.ApplicationLog.Fatal(s, ex);
        }

        public void Fatal(string s)
        {
            //if (!MQSystemConfiguration.GetMQSysConfiguration().ErrorLogEnable) return;
            // logger.Fatal(s);
            //LocalLogHelp.Write(FormatMessage(LogLevel.Fatal, s));
            Ymatou.CommonService.ApplicationLog.Fatal(s);
        }


        public void Fatal(string format, object[] args)
        {
            //if (!MQSystemConfiguration.GetMQSysConfiguration().ErrorLogEnable) return;
            //var msg = string.Format(format, args);
            ////  logger.FatalFormat(format, args);
            //LocalLogHelp.Write(FormatMessage(LogLevel.Fatal, format, args));
            Ymatou.CommonService.ApplicationLog.Fatal(format.Fomart(args));
        }
        public void Error2(string appid, string s)
        {
            Ymatou.CommonService.ApplicationLog.Error(new string[]{ appid}, s);
        }

        public void Error2(string appid, string message, Exception ex)
        {
            Ymatou.CommonService.ApplicationLog.Error(new string[] { appid }, message, ex);
        }

        public void Error2(string appid, string format, params object[] args)
        {
            Ymatou.CommonService.ApplicationLog.Error(new string[] { appid }, format.Fomart(args));
        }


        public void Error2(string[] appid, string s)
        {
            Ymatou.CommonService.ApplicationLog.Error(appid, s);
        }

        public void Error2(string[] appid, string message, Exception ex)
        {
            Ymatou.CommonService.ApplicationLog.Error(appid, message, ex);
        }

        public void Error2(string[] appid, string format, params object[] args)
        {
            Ymatou.CommonService.ApplicationLog.Error(appid, format.Fomart(args));
        }
    }
}
