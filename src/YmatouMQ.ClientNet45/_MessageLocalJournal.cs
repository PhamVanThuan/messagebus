using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YmatouMQ.ClientNet45
{
    class _MessageLocalJournal : _JournalBase
    {
        private static readonly Lazy<_MessageLocalJournal> localJournal = new Lazy<_MessageLocalJournal>(() => new _MessageLocalJournal());        

        private _MessageLocalJournal()
            : base(MessageBusClientCfg.Instance.DefaultConfigruation<int>(AppCfgInfo2.journalsize)
                , MessageBusClientCfg.Instance.DefaultConfigruation<string>(AppCfgInfo2.journalpath)
                , MessageBusClientCfg.Instance.DefaultConfigruation<int>(AppCfgInfo2.journalbuffersize))
        {

        }
        public static _MessageLocalJournal Instance { get { return localJournal.Value; } }
        protected override string DefaultJournalName
        {
            get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log", "messagebus.journal", "message.journal"); }
        }
        public void AppendAsync2(string context, string descript)
        {
            AppendAsync("{0}#{1}".F(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff"), context));
        }
        public void Append2(string context, string descript)
        {
            Append("{0}#{1}".F(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff"), context));
        }
    }
}
