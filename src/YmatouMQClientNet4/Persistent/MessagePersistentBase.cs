//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using YmatouMessageBusClientNet4.Extensions;

//namespace YmatouMessageBusClientNet4.Persistent
//{
//    public abstract class MessagePersistentBase<TMessage> where TMessage : class
//    {
//        //private readonly ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
//        private ConcurrentBag<TMessage> list = new ConcurrentBag<TMessage>();
//        private object fileLock = new object();
//        protected string filePath;
//        protected string dir;

//        protected virtual void SetFilePath(string path, string fileName, MessageType mType = MessageType.NotRetry)
//        {
//            if (mType == MessageType.NotRetry)
//                this.filePath = Path.Combine(path, "{0}.message".F(fileName));
//            else
//                this.filePath = Path.Combine(path, "{0}_{1}.message".F(fileName, 1));

//            EnsureDirectory();
//        }
//        public virtual void SetMessageDirectory(string dir)
//        {
//            _Assert.AssertArgumentNotNull(dir, "消息目录不能为空");
//            this.dir = dir;
//        }
//        public virtual MessagePersistentBase<TMessage> SetMessage(TMessage dto)
//        {
//            list.Add(dto);
//            return this;
//        }
//        public virtual int MemoryMessageCount { get { return list.Count; } }
//        public virtual void TryAppend(string fileFullPath)
//        {
//            TrySave(FileMode.Append, fileFullPath);
//        }
//        public virtual void TryAppend()
//        {
//            TrySave(FileMode.Append, this.filePath);
//        }
//        public virtual void TryTruncate(string fileFullPath)
//        {
//            if (File.Exists(fileFullPath))
//                TrySave(FileMode.Truncate, fileFullPath);
//        }
//        public virtual void TryTruncate()
//        {
//            if (File.Exists(this.filePath))
//                TrySave(FileMode.Truncate, filePath);
//        }
//        private void TrySave(FileMode fileMode, string fileFullPath)
//        {
//            try
//            {
//                lock (fileLock)
//                {
//                    using (var fsStream = new FileStream(fileFullPath, fileMode, FileAccess.Write, FileShare.Read))
//                    {
//                        if (list.Count > 0)
//                        {
//                            var by = list.AsParallel().ToArray().ToProtoBuf();
//                            fsStream.Write(by, 0, by.Length);
//                        }
//                        else
//                        {
//                            var _0by = new byte[0];
//                            fsStream.Write(_0by, 0, _0by.Length);
//                        }
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                Ymatou.CommonService.ApplicationLog.Error("write file error {0},".F(filePath), ex);
//            }
//            finally
//            {
//                list = new ConcurrentBag<TMessage>();
//            }
//        }
//        public abstract string[] GetAllMessageBusFile();
//        /// <summary>
//        /// 加载所有消息
//        /// </summary>
//        /// <param name="fileSizeLimit">文件大小上限（M）。超过此值则不加载文件</param>
//        /// <param name="errorHandler">异常处理</param>
//        /// <returns></returns>
//        public virtual ConcurrentDictionary<string, TMessage[]> LoadAllMessage(int fileSizeLimit = 0, Action<string, Exception> errorHandler = null)
//        {
//            string appid = string.Empty;
//            var allMessageFile = GetAllMessageBusFile();
//            if (!allMessageFile.Any())
//                return new ConcurrentDictionary<string, TMessage[]>();
//            return LoadMessage(fileSizeLimit, errorHandler, allMessageFile);
//        }


//        public virtual ConcurrentDictionary<string, TMessage[]> LoadSpecifyMessage(string fullPath, int fileSizeLimit = 0, Action<string, Exception> errorHandler = null)
//        {
//            return LoadMessage(fileSizeLimit, errorHandler, new string[] { fullPath });
//        }
//        public virtual void ClearFile()
//        {
//            var filePath = GetAllMessageBusFile();
//            lock (fileLock)
//            {
//                foreach (var path in filePath)
//                {
//                    using (var fsStream = new FileStream(path, FileMode.Truncate, FileAccess.Write, FileShare.None))
//                    {
//                        var _0by = new byte[0];
//                        fsStream.Write(_0by, 0, _0by.Length);
//                    }
//                }
//            }
//        }
//        private ConcurrentDictionary<string, TMessage[]> LoadMessage(int fileSizeLimit, Action<string, Exception> errorHandler, string[] allMessageFile)
//        {
//            lock (fileLock)
//            {
//                var dic = new ConcurrentDictionary<string, TMessage[]>();

//                foreach (var path in allMessageFile)
//                {
//                    if (string.IsNullOrEmpty(path)) return dic;
//                    try
//                    {
//                        TMessage[] messageArray;
//                        using (var fsStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Write))
//                        {
//                            if (fileSizeLimit > 0)
//                            {
//                                if (fsStream.Length < fileSizeLimit * 1024 * 1024)
//                                    messageArray = fsStream.FromProtoBuf<TMessage[]>();
//                                else
//                                {
//                                    messageArray = null;
//                                    errorHandler("file {0} size {1:N0} byte".F(path, fsStream.Length), new FileLoadException());
//                                }
//                            }
//                            else
//                            {
//                                messageArray = fsStream.FromProtoBuf<TMessage[]>();
//                            }
//                        }
//                        if (messageArray != null && messageArray.Length > 0)
//                            dic.TryAdd(Path.GetFileNameWithoutExtension(path), messageArray);
//                    }
//                    catch (Exception ex)
//                    {
//                        if (errorHandler != null)
//                            errorHandler(Path.GetFileNameWithoutExtension(path), ex);
//                    }
//                }
//                return dic;
//            }
//        }
//        private void EnsureDirectory()
//        {
//            var dirPath = Path.GetDirectoryName(filePath);
//            if (!Directory.Exists(dirPath))
//                Directory.CreateDirectory(dirPath);
//        }
//    }
//}
