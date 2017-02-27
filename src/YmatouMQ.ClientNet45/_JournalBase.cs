using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ymatou.CommonService;

namespace YmatouMQ.ClientNet45
{
    internal abstract class _JournalBase
    {
        private readonly object locker = new object();
        private readonly int fileSize = 2;//10485760;//10M;              
        private readonly int buffersize;
        private bool initOk;
        private string _logfullName;
        private SpinLock @lock = new SpinLock();
        protected StreamWriter sw;

        public _JournalBase(int journalsize, string journalpath, int journalbuffersize)
        {
            fileSize = journalsize;
            var journalPath = journalpath;
            buffersize = journalbuffersize;
            _logfullName = string.IsNullOrEmpty(journalPath) || journalPath == "." ? DefaultJournalName : journalPath;
            initOk = false;
        }
        public void Init()
        {
            if (fileSize <= 0) return;
            try
            {
                CheckLogName(_logfullName);
                CheckLogDirectory(_logfullName);
                InitStreamWriterl();
                initOk = true;
            }
            catch (Exception ex)
            {
                ApplicationLog.Error("总线日志初始化失败", ex);
                initOk = false;
            }
        }
        public virtual Task AppendAsync(string context)
        {
            return Task.Factory.StartNew(() => Append(context)).ContinueWith(t => { /*throw t.Exception;*/ }, TaskContinuationOptions.OnlyOnFaulted);
        }
        public virtual void Append(string context)
        {
            if (!initOk) return;
            if (string.IsNullOrEmpty(context)) return;

            bool _locked = false;
            try
            {
                @lock.TryEnter(5000, ref _locked);
                if (_locked)
                {
                    CheckJournal();
                    sw.WriteLine(context);
                }
            }
            finally
            {
                if (_locked) @lock.Exit();
            }
        }
        protected abstract string DefaultJournalName { get; }

        private void CheckJournal()
        {
            if (((sw.BaseStream.Length / 1024.0) / 1024.0) >= fileSize)
            {
                _TryCloseJournal();
                TryReNameFile();
                InitStreamWriterl();
            }
        }
        public void TryCloseJournal()
        {
            if (!initOk) return;
            bool _locked = false;
            try
            {
                if (sw != null)
                {
                    @lock.TryEnter(5000, ref _locked);
                    if (_locked)
                    {
                        sw.Flush();
                        sw.Close();
                    }
                }
            }
            catch
            {
                //TODO:Ignore
            }
            finally
            {
                if (_locked) @lock.Exit(_locked);
            }
        }
        private void _TryCloseJournal()
        {
            if (sw != null)
            {
                sw.Flush();
                sw.Close();
            }
        }
        private void InitStreamWriterl()
        {
            bool _locked = false;
            try
            {
                if (sw == null)
                {
                    @lock.TryEnter(5000, ref _locked);
                    if (_locked)
                    {
                        if (sw == null)
                        {
                            sw = new StreamWriter(GetJournalFileName(), true, Encoding.UTF8, buffersize);
                            sw.AutoFlush = true;
                        }
                    }
                }
            }
            finally
            {
                if (_locked)
                    @lock.Exit(_locked);
            }
        }
        private string GetJournalFileName()
        {
            if (!string.IsNullOrEmpty(_logfullName)) return _logfullName;
            return DefaultJournalName;
        }
        private static void CheckLogDirectory(string directory)
        {
            var _directory = Path.GetDirectoryName(directory);
            if (!Directory.Exists(_directory))
                Directory.CreateDirectory(_directory);
        }
        private static void CheckLogName(string logname)
        {
            if (logname == ".") return;
            if (string.IsNullOrEmpty(Path.GetExtension(logname))) throw new ArgumentNullException("error journal file name {0}".F(logname));
        }
        private static string NewJournalFileName(string oldName)
        {
            return Path.Combine(Path.GetDirectoryName(oldName), "{0}.{1}log".F(Path.GetFileNameWithoutExtension(oldName), DateTime.Now.ToString("yyyyMMddHHmmss")));
        }
        private void TryReNameFile()
        {
            try
            {
                File.Move(_logfullName, "{0}.{1}".F(_logfullName, DateTime.Now.ToString("yyyyMMddHHmmss")));
            }
            catch
            {
                _logfullName = NewJournalFileName(_logfullName);
            }
        }
    }
}
