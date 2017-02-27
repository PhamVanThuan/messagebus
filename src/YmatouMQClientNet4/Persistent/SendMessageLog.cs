using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Ymatou.CommonService;
using YmatouMessageBusClientNet4.Extensions;

namespace YmatouMessageBusClientNet4.Persistent
{
    public class MessageSendLog : JournalBase
    {
        public MessageSendLog()
            : base(MessageBusClientCfg.Instance.DefaultConfigruation<int>(AppCfgInfo2.messagesendlogsize)
                , MessageBusClientCfg.Instance.DefaultConfigruation<string>(AppCfgInfo2.messagesendlogpath)
                , MessageBusClientCfg.Instance.DefaultConfigruation<int>(AppCfgInfo2.messagesendlogbuffersize), "log")
        {

        }
        protected override string DefaultJournalName
        {
            get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log", "message.log"); }
        }
        public void AppendAsync2(string context, string descript)
        {
            AppendAsync("{0}|{1}|{2}| {3}".F(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff"), Thread.CurrentThread.ManagedThreadId, descript, context)).WithHandlerSuccess(() => { }/*sw.Flush()*/);
        }
        public void AppendAsync2(string context, Exception ex)
        {
            ApplicationLog.Error(context, ex);
            AppendAsync("{0}|{1}|{2}| {3}".F(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff"), Thread.CurrentThread.ManagedThreadId, context, ex.ToString())).WithHandlerSuccess(() => { }/*sw.Flush()*/);
        }
        public void Append2(string context, string descript)
        {
            base.Append("{0}|{1}|{2}| {3}".F(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff"), Thread.CurrentThread.ManagedThreadId, descript, context));
            //sw.Flush();
        }
        public void Append2(string context, Exception ex)
        {
            ApplicationLog.Error(context, ex);
            base.Append("{0}|{1}|{2}|{3}| {4}".F(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff"), Thread.CurrentThread.ManagedThreadId, "fail", context, ex.ToString()));
        }
    }
}
