using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using YmatouMessageBusClientNet4.Extensions;

namespace YmatouMessageBusClientNet4.Persistent
{
    public class MessageLocalJournal : JournalBase
    {
        public MessageLocalJournal()
            : base(MessageBusClientCfg.Instance.DefaultConfigruation<int>(AppCfgInfo2.journalsize)
                , MessageBusClientCfg.Instance.DefaultConfigruation<string>(AppCfgInfo2.journalpath)
                , MessageBusClientCfg.Instance.DefaultConfigruation<int>(AppCfgInfo2.journalbuffersize)
                ,"journal")
        {

        }
        protected override string DefaultJournalName
        {
            get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log", "message.journal"); }
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
    #region
    //public class MessageLocalJournal
    //{
    //    private static readonly object locker = new object();
    //    private static int fileSize = 2;//10485760;//10M;
    //    private static StreamWriter sw;
    //    private static string _logfullName;
    //    private static SpinLock @lock = new SpinLock();

    //    public static void InitJournal()
    //    {
    //        fileSize = MessageBusClientCfg.Instance.DefaultConfigruation<int>(AppCfgInfo2.journalsize);
    //        var journalPath = MessageBusClientCfg.Instance.DefaultConfigruation<string>(AppCfgInfo2.journalpath);
    //        _logfullName = string.IsNullOrEmpty(journalPath) || journalPath == "." ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log", "messagebus_.log") : journalPath;
    //        CheckLogName(_logfullName);
    //        CheckLogDirectory(_logfullName);
    //        InitStreamWriterl();
    //    }
    //    public static void AppendAsync(string context)
    //    {
    //        Task.Factory.StartNew(() => Append(context)).ContinueWith(t => { /*throw t.Exception;*/ }, TaskContinuationOptions.OnlyOnFaulted);
    //    }
    //    public static void AppendAsync(string context, string descript)
    //    {
    //        AppendAsync("{0} {1} {2}".F(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff"), descript, context));
    //    }
    //    public static void Append(string context, string descript)
    //    {
    //        Append("{0} {1} {2}".F(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff"), descript, context));
    //    }

    //    public static void Append(string context)
    //    {
    //        if (string.IsNullOrEmpty(context)) return;

    //        bool _locked = false;
    //        //lock (locker)
    //        try
    //        {
    //            @lock.TryEnter(5000, ref _locked);
    //            if (_locked)
    //            {
    //                CheckJournal();
    //                sw.WriteLine(context);
    //            }
    //        }
    //        finally
    //        {
    //            if (_locked) @lock.Exit();
    //        }
    //    }

    //    private static void CheckJournal()
    //    {
    //        if (((sw.BaseStream.Length / 1024.0) / 1024.0) >= fileSize)
    //        {
    //            TryCloseJournal();
    //            TryReNameFile();
    //            InitStreamWriterl();
    //        }
    //    }
    //    public static void TryCloseJournal()
    //    {
    //        try
    //        {
    //            if (sw != null)
    //            {
    //                sw.Flush();
    //                sw.Close();
    //            }
    //        }
    //        catch
    //        {
    //            //TODO:Ignore
    //        }
    //    }
    //    private static void InitStreamWriterl()
    //    {
    //        sw = new StreamWriter(GetJournalFileName(), true, Encoding.UTF8, MessageBusClientCfg.Instance.DefaultConfigruation<int>(AppCfgInfo2.journalbuffersize));
    //    }
    //    private static string GetJournalFileName()
    //    {
    //        if (!string.IsNullOrEmpty(_logfullName)) return _logfullName;
    //        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "messagebus_.log");
    //    }
    //    private static void CheckLogDirectory(string directory)
    //    {
    //        var _directory = Path.GetDirectoryName(directory);
    //        if (!Directory.Exists(_directory))
    //            Directory.CreateDirectory(_directory);
    //    }
    //    private static void CheckLogName(string logname)
    //    {
    //        if (logname == ".") return;
    //        if (string.IsNullOrEmpty(Path.GetExtension(logname))) throw new ArgumentNullException("error journal file name");
    //    }
    //    private static string NewJournalFileName(string oldName)
    //    {
    //        return Path.Combine(Path.GetFullPath(oldName), "{0}.{1}.log".F(Path.GetFileNameWithoutExtension(oldName), DateTime.Now.ToString("yyyyMMddHHmmss")));
    //    }
    //    private static void TryReNameFile()
    //    {
    //        try
    //        {
    //            File.Move(_logfullName, "{0}.{1}".F(_logfullName, DateTime.Now.ToString("yyyyMMddHHmmss")));
    //        }
    //        catch
    //        {
    //            _logfullName = NewJournalFileName(_logfullName);
    //        }
    //    }
    //}
    #endregion
}
