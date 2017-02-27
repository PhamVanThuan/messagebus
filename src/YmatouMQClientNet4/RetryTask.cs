//#define Test

//using System;
//using System.Collections.Generic;
//using System.Collections.Concurrent;
//using System.Linq;
//using System.Text;
//using Ymatou.CommonService;
//using System.Threading;
//using System.Threading.Tasks;
//using YmatouMessageBusClientNet4.Persistent;
//using YmatouMessageBusClientNet4.Extensions;
//using System.Diagnostics;

//namespace YmatouMessageBusClientNet4
//{
//    class RetryTask
//    {
//        private static readonly Lazy<RetryTask> lazy = new Lazy<RetryTask>(() => new RetryTask());
//        private readonly BlockingCollection<RetryMessage> queue = new BlockingCollection<RetryMessage>(MessageBusClientCfg.Instance.DefaultConfigruation<int?>(AppCfgInfo2.queuelimit).Value);
//        private readonly TaskQueue taskQueue = new TaskQueue();
//        private readonly _RetrySuccessMessagePersistent rmp = new _RetrySuccessMessagePersistent();
//        private readonly GtQueueLimitMessagePersistent gtQueueLimitFile = new GtQueueLimitMessagePersistent();
//        private string currentFileFullPath = null;
//        private int queueLimitCount = 0;
//        private int retrySuccessCount = 0;
//        private int currentGtQueueLimitFileIndex = 0;
//        private long gtQueueLimitMessageCount = 0;
//        private int waitSendMessageIndex = 0;
//        private _Message[] cacheMessage;
//        private Timer timerTask;
//        private bool isRuning = false;
//        private bool isExecutedTask = false;
//        private RetryTask() { }

//        public static RetryTask Instance { get { return lazy.Value; } }

//        public void StartRetryTask()
//        {
//#if !Test

//            RetrySendToMessageBusTask();
//#else
//            var cfg = MessageBusClientCfg.Instance.DefaultConfigruation<int?>(AppCfgInfo2.retrytime);
//            if (cfg == null || cfg.Value <= 0)
//            {
//                ApplicationLog.Debug("未开启消息补偿服务");
//                return;
//            }
//            timerTask = new Timer(o =>
//            {
//                TryRetrySendToMessageBusTask();
//                if (!isRuning)
//                {
//                    ApplicationLog.Debug("mq bus client app stop....");
//                    return;
//                }
//                timerTask.Change(cfg.Value, Timeout.Infinite);
//            }, null, Timeout.Infinite, Timeout.Infinite);
//            isRuning = true;
//            timerTask.Change(cfg.Value, Timeout.Infinite);
//            taskQueue.StartTaskQueue(1);
//            var queueLimit = MessageBusClientCfg.Instance.DefaultConfigruation<int?>(AppCfgInfo2.queuelimit);
//            ApplicationLog.Debug("timer task start ok。retrytimespan {0} ms,queue limit {1}".F(cfg.Value, queueLimit.Value));
//#endif
//        }
//        public void StopRetryTask()
//        {
//            //队列标记为不可接受添加
//            queue.CompleteAdding();
//            //保存队列里的数据到磁盘
//            DumpQueueMessage();
//            isRuning = false;
//            var taskOk = false;
//            if (isExecutedTask)
//            {
//                ApplicationLog.Debug("正在执行任务,等待其结束，超时时间3秒");
//                taskOk = SpinWait.SpinUntil(() => !isExecutedTask, 5000);
//            }
//            //释放线程
//            if (timerTask != null)
//                timerTask.Dispose();
//            gtQueueLimitFile.DeleteAllMarkDelFile();
//            ApplicationLog.Debug("MessageBus client stop ok,task timeOut {0},delete all file...".F(taskOk));
//        }
//        public void PostToRetryTask(string appid, string code, string messageid, object body, string ip, DateTime expiredAtTime)
//        {
//            var retrytimeout = MessageBusClientCfg.Instance.Configruation<TimeSpan?>(appid, code, AppCfgInfo2.retrytimeout);
//            //如果未开启消息重试，则直接退出;
//            if (retrytimeout == null && retrytimeout.Value.TotalMinutes <= 0)
//            {
//                ApplicationLog.Debug("appid {0},code {1} 未开启消息重试功能".F(appid, code));
//                return;
//            }
//            //如果超过重试时间，消息直接写入磁盘，不再处理
//            if (expiredAtTime <= DateTime.Now)
//            {
//                ApplicationLog.Debug("消息超过补偿时间:{0},{1},{2},{3},{4}".F(appid, code, messageid, body, expiredAtTime));
//                TrySaveTimeOutMessageToDiskAsync(appid, "{0}#{1}".F(code, ip), messageid, body, expiredAtTime);
//                return;
//            }
//            var queueLimit = MessageBusClientCfg.Instance.DefaultConfigruation<int?>(AppCfgInfo2.queuelimit);
//            if (queue.Count < queueLimit.Value)
//            {
//                var retryMessage = new RetryMessage
//                {
//                    appid = appid,
//                    code = code,
//                    messageid = messageid,
//                    body = body,
//                    ip = ip,
//                    expiredAtTime = expiredAtTime
//                };
//                var resultOk = queue.TryAdd(retryMessage);
//                if (!resultOk)
//                {
//                    var fileName = GenerateGtQueueLimitFileIndex();
//                    TrySaveMessageToDiskAsync(appid, code, messageid, body, expiredAtTime, fileName);
//                }
//                else
//                {
//                    TrySaveMessageToDiskAsync(appid, code, messageid, body, expiredAtTime, "0.gtqueuelimit");
//                }
//                ApplicationLog.Debug("message send error {0},{1},{2}，write file, addqueue {4} ,memory queue count {3}".F(appid, code, messageid, queue.Count, resultOk));
//            }
//            else
//            {
//                var fileName = GenerateGtQueueLimitFileIndex();
//                TrySaveMessageToDiskAsync(appid, code, messageid, body, expiredAtTime, fileName);
//                ApplicationLog.Debug("内存队列到达上限 {0}，current queue count {1}，message append to {2}, count {3}".F(queueLimit, queue.Count, fileName, Interlocked.Increment(ref queueLimitCount)));
//            }
//        }

//        private void TryRetrySendToMessageBusTask()
//        {
//            try
//            {
//                //taskQueue.AddWorkToQueue(() => FillNeedRetryMessageToQueue(), ex => ApplicationLog.Error("queue FillNeedRetryMessageToQueue work.1", ex));
//                isExecutedTask = true;
//                if (queue.Count <= 0)
//                    FillNeedRetryMessageToQueue();
//                if (queue.Count <= 0)
//                {
//                    isExecutedTask = false;
//                    return;
//                }
//                var batchMessageLimit = MessageBusClientCfg.Instance.DefaultConfigruation<int?>(AppCfgInfo2.batchMessageLimit);
//                //从队列获取消息
//                var list = PullMessageFromMemoryQueue(batchMessageLimit.Value);
//                //获取重试发送成功的消息
//                var messages = ExecutedPublishToMessageBus(list);
//                //存在发送成功的消息，追加到retry_1.message
//                AppendSendSuccessMessageFlagToDisk(messages);
//                //刷新磁盘上的消息文件
//                DumpQueueMessage();
//                isExecutedTask = false;
//            }
//            catch (Exception ex)
//            {
//                ApplicationLog.Error("补发消息异常 {0}", ex);
//            }
//        }
//        private HashSet<RetryMessage> PullMessageFromMemoryQueue(int limit)
//        {
//            RetryMessage rm;
//            int index = 0;
//            var list = new HashSet<RetryMessage>(new MessageEqualityComparer());
//            while (queue.TryTake(out rm))
//            {
//                var addSuccess = list.Add(rm);
//                if (addSuccess == false)
//                    ApplicationLog.Debug("消息添加到hashSet失败 {0},{1},{2}".F(rm.appid, rm.code, rm.messageid));
//                if (Interlocked.Increment(ref index) == limit)
//                    break;
//            }
//            return list;
//        }
//        //保存重试发送成功的消息的消息ID
//        private void AppendSendSuccessMessageFlagToDisk(ConcurrentDictionary<string, List<_RetrySuccessMessage>> messages)
//        {
//            taskQueue.AddWorkToQueue(() =>
//            {
//                var storePath = MessageBusClientCfg.Instance.DefaultConfigruation<string>(AppCfgInfo2.storepath, AppCfgInfo2.default_Store_Path);
//                rmp.SetFilePath(storePath, "_retry");
//                foreach (var item in messages)
//                {
//                    foreach (var message in item.Value)
//                    {
//                        rmp.SetMessage(new _RetrySuccessMessage(message.messageid));
//                    }
//                    rmp.TryAppend();
//                    ApplicationLog.Debug("{0} 条消息补发成功，追加到_retry_1文件".F(retrySuccessCount));
//                }
//            }, ex => ApplicationLog.Error("queue AppendSendSuccessMessageFlagToDisk work.2", ex));
//        }
//        //获取重试发送成功的消息
//        private ConcurrentDictionary<string, List<_RetrySuccessMessage>> ExecutedPublishToMessageBus(HashSet<RetryMessage> list)
//        {
//            var messages = new ConcurrentDictionary<string, List<_RetrySuccessMessage>>();
//            //foreach (var item in list)
//            Parallel.ForEach(list, item =>
//            {
//                //执行补发消息
//                var result = MessageBusAgent._PublishSyncToPrimaryMessageBus(item.appid, item.code, item.messageid, item.body);
//                ApplicationLog.Debug("补发消息结果 {0},{1},{2},result:{3}".F(item.appid, item.code, item.messageid, result));
//                //重试是否成功,如果重试成功则记录
//                if (result == "ok")
//                {
//                    Interlocked.Increment(ref retrySuccessCount);
//                    if (!messages.ContainsKey(item.appid))
//                    {
//                        messages[item.appid] = new List<_RetrySuccessMessage> { { new _RetrySuccessMessage(item.messageid) } };
//                    }
//                    else
//                    {
//                        messages[item.appid].Add(new _RetrySuccessMessage(item.messageid));
//                    }
//                }
//            });
//            return messages;
//        }
//        //对内存Queue填充需要补发的消息
//        private void FillNeedRetryMessageToQueue()
//        {
//            var messages = LoadMessageAndResetCount();
//            if (!messages.Any())
//                return;
//            if (queue.Count <= 0)
//            {
//                Parallel.ForEach(messages, m => queue.TryAdd(new RetryMessage { appid = m.appid, body = m.message, code = m.code, messageid = m.messageid, expiredAtTime = m.expiredAtTime.ToDateTime() }));
//                ApplicationLog.Debug("add message to queue, queue count {0},message source :{1}".F(queue.Count, messages.Count()));
//            }
//        }
//        private IEnumerable<_Message> LoadMessageAndResetCount()
//        {
//            //加载全部消息 & 加载已经补发成功的消息       
//            var allMessageTask = LoadGtQueueLimitMessage();
//            var successMessageIdTask = LoadSendSuccessMessage();
//            if (!WaitTaskComplete(new Task[2] { allMessageTask, successMessageIdTask }))
//                return Enumerable.Empty<_Message>();

//            if (!allMessageTask.Result.Any())
//            {
//                RsetCount();
//                ApplicationLog.Debug("未加载到需要补发的消息，重置计数器");
//                return Enumerable.Empty<_Message>();
//            }
//            //获取真实需要补发的消息
//            var tempRealRetryMessage = allMessageTask.Result.Where(e => !successMessageIdTask.Result.Contains(e.messageid));
//            //没有可补发的消息，则清空磁盘上的文件            
//            if (!tempRealRetryMessage.Any())
//            {
//                RsetCount();
//                taskQueue.AddWorkToQueue(() =>
//               {
//                   var messagelocalstorepath = MessageBusClientCfg.Instance.DefaultConfigruation<string>(AppCfgInfo2.storepath, AppCfgInfo2.default_Store_Path);
//                   rmp.SetMessageDirectory(messagelocalstorepath);
//                   rmp.ClearFile();
//               }, ex => ApplicationLog.Error("清理文件异常2"));
//                ApplicationLog.Debug("没有可补发的消息清空全部已使用的文件,重置计数器");
//            }
//            ApplicationLog.Debug("{0} 条消息等待补发".F(tempRealRetryMessage.Count()));
//            return tempRealRetryMessage;
//        }

//        private void RsetCount()
//        {
//            waitSendMessageIndex = 0;
//            queueLimitCount = 0;
//            retrySuccessCount = 0;
//        }
//        private bool WaitTaskComplete(Task[] task)
//        {
//            try
//            {
//                if (!Task.WaitAll(task, 5000))
//                {
//                    ApplicationLog.Debug("加载文件超时");
//                    return false;
//                }
//                return true;
//            }
//            catch (AggregateException ex)
//            {
//                ApplicationLog.Error("加载文件异常（AggregateException）{0}".F(ex.ToString()));
//                return false;
//            }
//        }
//        private Task<ParallelQuery<_Message>> LoadGtQueueLimitMessage()
//        {
//            gtQueueLimitFile.SetMessageDirectory(MessageBusClientCfg.Instance.DefaultConfigruation<string>(AppCfgInfo2.storepath, AppCfgInfo2.default_Store_Path));
//            currentFileFullPath = gtQueueLimitFile.GetMinIndexFilePath();
//            if (string.IsNullOrEmpty(currentFileFullPath))
//                return Task.Factory.StartNew(() => Enumerable.Empty<_Message>().AsParallel());
//            var gtQueueLimitMessageTask = Task.Factory.StartNew(() =>
//            {
//                var queulimitfileSize = MessageBusClientCfg.Instance.DefaultConfigruation<int?>(AppCfgInfo2.queuelimitfileSize);
//                var gtQueueLimitMessage = gtQueueLimitFile
//                                            .LoadSpecifyMessage(currentFileFullPath, queulimitfileSize.Value, (msg, ex) => ApplicationLog.Error("加载(gtQueueLimitFile)文件异常{0}".F(msg), ex))
//                                            .AsParallel().
//                                            SelectMany(e => e.Value);
//                ApplicationLog.Debug("load message count {0} ,file Path {1}".F(gtQueueLimitMessage.Count(), currentFileFullPath));
//                return gtQueueLimitMessage;
//            });
//            return gtQueueLimitMessageTask;
//        }
//        private Task<ParallelQuery<string>> LoadSendSuccessMessage()
//        {
//            var successMessageIdTask = Task.Factory.StartNew(() =>
//            {
//                var messagelocalstorepath = MessageBusClientCfg.Instance.DefaultConfigruation<string>(AppCfgInfo2.storepath, AppCfgInfo2.default_Store_Path);
//                rmp.SetMessageDirectory(messagelocalstorepath);
//                var successMessageId = rmp
//                                                                .LoadAllMessage(0, (msg, ex) => ApplicationLog.Error("加载文件异常{0}".F(msg), ex))
//                                                                .AsParallel()
//                                                                .SelectMany(e => e.Value)
//                                                                .Select(r => r.messageid);
//                ApplicationLog.Debug("load file _retry_1,message count:{0}".F(successMessageId.Count()));
//                return successMessageId;
//            });
//            return successMessageIdTask;
//        }
//        //private Task<ParallelQuery<_Message>> LoadWaitSendToBusMessage()
//        //{
//        //    var allMessageTask = Task.Factory.StartNew(() =>
//        //    {
//        //        mp.SetMessageDirectory(cfg.messagelocalstorepath);
//        //        var allMessage2 = mp
//        //                            .LoadAllMessage(0, (msg, ex) => ApplicationLog.Error("加载文件异常{0}".F(msg), ex))
//        //                            .AsParallel();

//        //        var allMessage = allMessage2.SelectMany(e => e.Value);
//        //        return allMessage;
//        //    });
//        //    return allMessageTask;
//        //}
//        //未重试发送给messageBus的消息，持久化到磁盘         
//        private void DumpQueueMessage()
//        {
//            taskQueue.AddWorkToQueue(() =>
//           {
//               var fileName = currentFileFullPath;
//               fileName.NullObjectReplace(v => fileName = v, gtQueueLimitFile.GetMinIndexFilePath());
//               if (queue.Count <= 0)
//               {
//                   gtQueueLimitFile.Delete(fileName);
//                   ApplicationLog.Debug("queue is empty,Delete file {0}".F(fileName));
//               }
//               else
//               {
//                   var notRetryMessages = ToMessage(queue.ToList()).AsParallel();
//                   Parallel.ForEach(notRetryMessages, m => gtQueueLimitFile.SetMessage(new _Message(m.appid, m.code, m.message, m.messageid, m.expiredAtTime.ToDateTime())));
//                   var realMessageCount = gtQueueLimitFile.MemoryMessageCount;
//                   gtQueueLimitFile.TryTruncate(fileName);
//                   ApplicationLog.Debug("dump queue message count:{0},append message {1} ,file {2}".F(notRetryMessages.Count(), realMessageCount, fileName));
//               }
//           }, ex => ApplicationLog.Error("queue DumpQueueMessage work.1", ex));
//        }
//        private void TrySaveTimeOutMessageToDiskAsync(string appid, string code, string messageid, object body, DateTime expiredAtTime)
//        {
//            TrySaveMessageToDiskAsync(appid, code, messageid, body, expiredAtTime, "_timeout");
//        }
//        private void TrySaveMessageToDiskAsync(string appid, string code, string messageid, object body, DateTime expiredAtTime, string fileName = null)
//        {
//            taskQueue.AddWorkToQueue(() =>
//            {
//                var messagelocalstorepath = MessageBusClientCfg.Instance.DefaultConfigruation<string>(AppCfgInfo2.storepath, AppCfgInfo2.default_Store_Path);
//                gtQueueLimitFile.SetFilePath(messagelocalstorepath, fileName ?? appid);
//                gtQueueLimitFile.SetMessage(new _Message(appid, code, body.ToJson(), messageid, expiredAtTime));
//                gtQueueLimitFile.TryAppend();
//            }, ex => ApplicationLog.Error("queue TrySaveMessageToDiskAsync work.1", ex));
//        }
//        private void TrySaveMessageToDiskSync(string appid, string code, string messageid, object body, DateTime expiredAtTime, string fileName = null)
//        {
//            taskQueue.AddWorkToQueue(() =>
//            {
//                var messagelocalstorepath = MessageBusClientCfg.Instance.DefaultConfigruation<string>(AppCfgInfo2.storepath, AppCfgInfo2.default_Store_Path);
//                gtQueueLimitFile.SetFilePath(messagelocalstorepath, fileName ?? appid);
//                gtQueueLimitFile.SetMessage(new _Message(appid, code, body.ToJson(), messageid, expiredAtTime));
//                gtQueueLimitFile.TryAppend();
//            }, ex => ApplicationLog.Error("queue TrySaveMessageToDiskSync work.0", ex));
//        }
//        private string GenerateGtQueueLimitFileIndex()
//        {
//            if (currentGtQueueLimitFileIndex <= 0)
//                currentGtQueueLimitFileIndex = gtQueueLimitFile.GetMaxIndex() + 1;

//            var index = Interlocked.Increment(ref gtQueueLimitMessageCount);
//            var memoryQueueLimit = MessageBusClientCfg.Instance.DefaultConfigruation<int>(AppCfgInfo2.queuelimit);
//            if (index % memoryQueueLimit == 0)
//            {
//                //gtQueueLimitMessageCount = 0;
//                Interlocked.Increment(ref currentGtQueueLimitFileIndex);
//            }
//            return "{0}.gtqueuelimit".F(currentGtQueueLimitFileIndex);
//        }
//        private static IEnumerable<_Message> ToMessage(IEnumerable<RetryMessage> message)
//        {
//            var list = new List<_Message>();
//            foreach (var item in message)
//            {
//                list.Add(new _Message(item.appid, item.code, item.body.ToJson(), item.messageid, item.expiredAtTime));
//            }
//            return list;
//        }
//    }
//    internal struct RetryMessage
//    {
//        public string appid { get; set; }
//        public string code { get; set; }
//        public string messageid { get; set; }
//        public object body { get; set; }
//        public string ip { get; set; }
//        public DateTime expiredAtTime { get; set; }
//    }
//    internal class MessageEqualityComparer : IEqualityComparer<RetryMessage>
//    {

//        public bool Equals(RetryMessage x, RetryMessage y)
//        {
//            return x.appid == y.appid && x.code == y.code && x.messageid == y.messageid;
//        }

//        public int GetHashCode(RetryMessage obj)
//        {
//            return obj.appid.GetHashCode() ^ obj.code.GetHashCode() ^ obj.messageid.GetHashCode();
//        }
//    }
//}
