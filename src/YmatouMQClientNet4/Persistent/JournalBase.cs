using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ymatou.CommonService;
using YmatouMessageBusClientNet4.Extensions;

namespace YmatouMessageBusClientNet4.Persistent
{
    public abstract class JournalBase
    {
        private readonly object locker = new object();
        private readonly int fileSize = 2;//10485760;//10M;              
        private readonly int buffersize;
        private bool initOk;
        private string _logfullName;
        private string _logType;
        private string _logFileName;
        private bool isruning;
        private SpinLock @lock = new SpinLock();
        protected StreamWriter sw;

        public JournalBase(int journalsize, string journalpath, int journalbuffersize, string logType)
        {
            this.fileSize = journalsize;
            this.buffersize = journalbuffersize;
            this._logfullName = journalpath;
            this._logType = logType;
            this.initOk = false;
        }
        public void Init()
        {
            //fileSize <=0 表示日志关闭
            if (fileSize <= 0) return;
            try
            {
                _logfullName = string.IsNullOrEmpty(_logfullName) || _logfullName == "." ? DefaultJournalName : _logfullName;
                CheckLogName(_logfullName);
                CheckLogDirectory(_logfullName);
                InitStreamWriter();
                initOk = true;               
            }
            catch (Exception ex)
            {
                ApplicationLog.Error("总线日志初始化失败", ex);
                initOk = false;
                isruning = false;
            }
        }
        public virtual Task AppendAsync(string context)
        {
            return Task.Factory.StartNew(() => Append(context)).ContinueWith(t => { /*throw t.Exception;*/ }, TaskContinuationOptions.OnlyOnFaulted);
        }
        public virtual void Append(string context)
        {
            //初始化异常或已停止,则直接退出
            if (!initOk || !isruning) return;
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
            if (sw.BaseStream != null && ((sw.BaseStream.Length / 1024.0) / 1024.0) >= fileSize)
            {
                _TryCloseJournal();
                TryReNameFile();
                ReInitStreamWriter();
            }
        }
        private void _TryCloseJournal()
        {
            try
            {
                sw.Flush();
                sw.Close();
            }
            catch
            {
                //TODO:Ignore
            }
        }
        public void TryCloseJournal()
        {
            //初始化失败，或已停止
            if (!initOk || !isruning) return;
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
                        isruning = false;
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
        private void InitStreamWriter()
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
                            isruning = true;
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
        private void ReInitStreamWriter()
        {
            sw = new StreamWriter(GetJournalFileName(), true, Encoding.UTF8, buffersize);
            sw.AutoFlush = true;
            isruning = true;
        }
        private string GetJournalFileName()
        {
            if (!string.IsNullOrEmpty(_logfullName)) return _logfullName;
            return DefaultJournalName;
        }
        private void CheckLogDirectory(string directory)
        {
            var _directory = Path.GetDirectoryName(directory);
            if (!Directory.Exists(_directory))
                Directory.CreateDirectory(_directory);
        }
        private void CheckLogName(string logname)
        {
            if (string.IsNullOrEmpty(Path.GetExtension(logname))) throw new ArgumentNullException("error journal file name {0}".F(logname));
        }
        private string NewJournalFileName(string oldName)
        {
            return Path.Combine(Path.GetDirectoryName(oldName), "{0}.{1}{2}".F(Path.GetFileNameWithoutExtension(oldName), _logType, DateTime.Now.ToString("yyyyMMddHHmmssffff")));
        }
        private void TryReNameFile()
        {
            try
            {
                File.Move(_logfullName, "{0}.{1}".F(_logfullName, DateTime.Now.ToString("yyyyMMddHHmmssffff")));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                _logfullName = NewJournalFileName(_logfullName);
            }
        }
    }
}
