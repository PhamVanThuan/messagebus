using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Configuration;
using System.IO;
using YmatouMQ.Common.Extensions;
using YmatouMQMessageMongodb.AppService;
using YmatouMQ.Common;
using YmatouMQ.Common.Extensions._Task;
using YmatouMQ.Common.Extensions.Serialization;
using YmatouMQNet4.Configuration;
using YmatouMQ.Common.MessageHandleContract;
using YmatouMQ.MessageScheduler;
using YmatouMQ.Log;
using YmatouMQMessageMongodb.Domain.Module;
using YmatouMQ.ConfigurationSync;
using YmatouMQ.Common.Extensions;
using YmatouMQ.Common.Utils;

namespace YmatouMQ.MessageCompensateService
{
    public class MessageCompensateTaskService
    {
        private static readonly RetryMessageCompensateAppService compensate = new RetryMessageCompensateAppService();        
        private static readonly List<TimerTaskInfo> timertask = new List<TimerTaskInfo>();
        private static readonly ConcurrentDictionary<string, byte> cache = new ConcurrentDictionary<string, byte>();

        private static readonly string timerTaskPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config",
            "timertask.config");

        private static readonly IMessageRetryHandle<byte[]> handle = new MessageHandlerScheduler();

        private static readonly ILog log = LogFactory.GetLogger(LogEngineType.RealtimelWriteFile,
            "YmatouMQ.MessageCompensateService.MessageCompensateTaskService");

        private const int defaultlbatchsize = 100;

        public static void Start()
        {
            var service = PrivateStart().WithHandleException(log,"MQ消息补偿服务启动异常");
        }

        private static async Task PrivateStart()
        {
            //log4net.GlobalContext.Properties["LogFileName"] = AppDomain.CurrentDomain.FriendlyName.Replace(":", "");
            log4net.Config.XmlConfigurator.Configure(
                new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "log4net.config")));
            MQMainConfigurationManager.Builder.Start();

            var timerTask = await BuilderTimerTask();

            //foreach (var task in timerTask)
            //await timerTask.ForEachAsync(Environment.ProcessorCount, async task =>
            timerTask.EachAction(async task =>
            {
                var timerKey = task.CreateTimerKey(task.id);
                log.Debug("timer task cycle : {0},id : {1},get data size: {2}", task.cycletime, timerKey, task.size);
                await task.SetTimer(new Timer(async o =>
                {
                    if (!task.id.StartsWith("t_check"))
                        await
                            RunRetryTask(timerKey, task.size, task.scan)
                                .WithHandleException(log, null, "补偿消息异常,taskid {0}", timerKey);
                    else
                        await
                            RunCheckStatusTask(timerKey, task.size, task.scan)
                                .WithHandleException(log, null, "check消息状态异常,taskid {0}", timerKey);
                    task.timer.Change(task.cycletime, TimeSpan.FromMilliseconds(Timeout.Infinite));
                    log.Info("timer task {0} run {1}", timerKey, task.cycletime);
                }, null, Timeout.Infinite, /* task.cycletime, TimeSpan.FromMilliseconds(Timeout.Infinite)*/
                    Timeout.Infinite));
                task.timer.Change(task.cycletime, TimeSpan.FromMilliseconds(Timeout.Infinite));
                timertask.Add(task);
            });
        }

        public static void Stop()
        {
            try
            {
                MQMainConfigurationManager.Builder.Stop();
                timertask.ForEach(t => t.timer.Dispose());
            }
            catch (Exception ex)
            {
                log.Error("MQ消息补偿服务停止异常", ex);
            }
        }

        //执行检查补单状态任务
        private static async Task RunCheckStatusTask(string taskKey, int batchSize, TimeSpan scan)
        {
            if (!taskKey.StartsWith("t_check")) return;
            try
            {
                //check补偿次数为0，且补偿过期的消息
                var allCollections = compensate.FindAllCollection();
                await allCollections.ForEachAsync(Environment.ProcessorCount, async tbName =>
                {
                    var result =
                        await
                            compensate.UpdateRetryTimeOutStatus(tbName, DateTime.Now.Subtract(scan))
                                .ConfigureAwait(false);
                    if (result.DocumentsAffected > 0)
                        log.Debug("[RMRS] reset message retry status,count: {0}".Fomart(result.DocumentsAffected));
                });
            }
            catch (Exception ex)
            {
                log.Error("补偿消息异常(RunCheckStatusTask)", ex);
            }
        }

        //执行补单任务
        private static async Task RunRetryTask(string taskKey, int batchSize, TimeSpan scan)
        {
            try
            {
                //获取所有表 
                var allCollections = compensate.FindAllCollection();
                await allCollections.ForEachAsync(Environment.ProcessorCount, async tbName =>
                    //foreach (var tbName in allCollections)
                {
                    //查找未超过补偿时间的消息&且未补偿或者补发失败的消息&创建消息时间在scan范围内
                    var ids = compensate.FindAwaitRetryMessageIds(0, batchSize <= 0 ? defaultlbatchsize : batchSize,
                        scan, tbName);
                    if (!ids.IsEmptyEnumerable())
                    {
                        //标记为当前timerID&状态为补偿中
                        var upResult = compensate.Update_AppKeyAndStatus(taskKey, ids, tbName, RetryStatus.Retrying);
                        log.Debug("timerid {0}, 等待补单 {1} 个,更新成功 {2},tbName {3}", taskKey, ids.Count(), upResult, tbName);
                        if (upResult > 0)
                        {
                            //获取真实需要补发的消息（当前timerID&未超过补偿时间&状态为非补偿成功）
                            var message =
                                compensate.FindRealAwaitRetryMessage(taskKey, 0, Convert.ToInt32(upResult), tbName)
                                    /*.AsParallel()*/.ToList();
                            if (!message.IsEmptyEnumerable())
                            {
                                log.Debug("timerid {0},real need retry message,count {1},tbName {2}", taskKey,
                                    message.Count(), tbName);
                                //超过一定数量的不单数，则发出预警邮件
                                var alarmCount = "RetryMessageAlarm".GetAppSettings("1000").ToInt32(1000);
                                if (alarmCount > 0 && message.Count > alarmCount)
                                {
                                    log.Error("消息总线补单服务,补单数量:{0},扫描数据时间：{1},table:{2}", message.Count, scan, tbName);
                                }
                                if (message.Count>0)
                                    log.Info("消息总线补单服务,补单数量:{0},扫描数据时间：{1},table:{2},timerId:{3}", message.Count, scan, tbName, taskKey);
                                //执行补单
                                //foreach (var msg in message)                               
                                await message.ForEachAsync(Environment.ProcessorCount, async msg =>
                                {
                                    using (var mm = new YmatouMQ.Common.Utils.MethodMonitor(log, descript:"[RetryDone] mid:{0},timerId:{1}".Fomart(msg.MessageId,taskKey)))
                                    {
                                        //构建消息处理上下文
                                        var messageContext = BuildMessageHandleContext(msg);
                                        if (messageContext.RetryCallbackKey.IsEmptyEnumerable())
                                        {
                                            await
                                                compensate.UpdateStatus(msg._id, tbName, RetryStatus.RetryOk)
                                                    .ConfigureAwait(false);
                                            log.Debug(
                                                "[RetryOK] mid:{0},timerId:{1}，all retry ok，update retry status ok.",
                                                msg.MessageId, taskKey);
                                        }
                                        else
                                        {                                           
                                            //执行补发消息&更新重试状态
                                            await
                                                handle.Handle(messageContext,
                                                    async callbackDic =>
                                                        await UpdaeMessageReterStatus(tbName, msg, callbackDic,taskKey));                                           
                                        }                                      
                                    }
                                });
                                log.Info("message retry done,count {0}", message.Count());
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                log.Error("补偿消息异常(RunTask)", ex);
            }
        }

        public static void SaveTimerTaskInfoCfg(IEnumerable<TimerTaskInfo> cfgInfo)
        {
            var cfgString = cfgInfo.JSONSerializationToString();
            using (var fileStream = FileAsync.OpenWrite(timerTaskPath))
            using (var write = new StreamWriter(fileStream, Encoding.GetEncoding("utf-8")))
            {
                write.Write(cfgString);
            }
        }

        public static async Task<IEnumerable<TimerTaskInfo>> BuilderTimerTask()
        {
            //
            //格式：key：timertask,value：{{cycletime:10,size:10,id:1},{cycletime=60,size=60,id=2},{cycletime=300,size=300,id=3}}
            //cycletime 单位：毫秒，size 每次拉取数据大小 , id 为机器ip+timer编号
            //TODO:反序列化为 TimerTaskInfo
            var cfgString = await ReadTimerTaskCfg();
            if (cfgString.IsEmpty()) return TimerTaskInfo.Default;

            var taskInfo = cfgString.JSONDeserializeFromString<IEnumerable<TimerTaskInfo>>();
            return taskInfo;
        }

        private static async Task<string> ReadTimerTaskCfg()
        {
            if (!File.Exists(timerTaskPath)) return string.Empty;
            using (var fileStream = FileAsync.OpenRead(timerTaskPath))
            using (var streamRead = new StreamReader(fileStream, Encoding.GetEncoding("utf-8")))
            {
                return await streamRead.ReadToEndAsync();
            }
        }

        private static MessageHandleContext<byte[]> BuildMessageHandleContext(RetryMessage msg)
        {
            var messageContext =
                new MessageHandleContext<byte[]>(Encoding.GetEncoding("utf-8").GetBytes(msg.Body.ToString())
                    , true
                    , msg.AppId
                    , msg.Code
                    , msg.MessageId
                    , _MessageSource.MessageSource_Retry);
            if (msg.MessageSource == _MessageSource.MessageSource_Publish
                && !cache.ContainsKey("{0}__".Fomart(msg._id)))
            {
               var check= MessageAppService.IsCehckMessageRetry(msg._id, msg.AppId, msg.Code);
               log.Debug("[IsCehckMessageRetry] result :{0}",check);
                messageContext.SetIsCheckEableRetry(check);
                AddCache("{0}__".Fomart(msg._id));
            }          
            messageContext.SetCallback(msg
                .CallbackKey
                .Where(c => c.Status != RetryStatus.RetryOk)
                .Select(c => c.CallbackKey)
                .ToArray());
            return messageContext;
        }

        private static async Task UpdaeMessageReterStatus(string tbName, RetryMessage msg,
            IDictionary<string, bool> callbackDic,string timerKey)
        {
            //根据补发结果，则更新对应的业务段补发状态&增加补发计数器

            msg.CallbackKey.EachAction(c =>
            {
                if (callbackDic.ContainsKey(c.CallbackKey))
                {
                    c.Status = callbackDic[c.CallbackKey]
                        ? RetryStatus.RetryOk
                        : RetryStatus.RetryFail;
                    c.RetryCount = c.AddRetryCount(c.RetryCount);
                }
            });
            //更新mongodb消息补单状态
            var status = msg.CallbackKey.Any(c => c.Status == RetryStatus.RetryFail)
                ? RetryStatus.RetryFail
                : RetryStatus.RetryOk;
            var retryCountMax = msg.CallbackKey.Select(c => c.RetryCount).Max();
            await compensate.UpdateStatusAndIncrementRetryCount(
                msg._id
                , tbName
                , status
                , msg.CallbackKey
                , retryCountMax)
                .WithHandleException(log, "update mongodb error {0}", msg._id)
                .ConfigureAwait(false);
            //如果补偿消息来源为，总线接收服务，则保存该消息推送状态
            if (msg.MessageSource == _MessageSource.MessageSource_Publish  && !cache.ContainsKey(msg._id))
            {
                AddCache(msg._id);
                var callbackResult = new List<string>();
                var _callbackCfg =
                    MQMainConfigurationManager.Builder.GetConfiguration(msg.AppId, msg.Code).CallbackCfgList;
                msg.CallbackKey.EachAction(_c =>
                {
                    var _result = _c.Status == RetryStatus.RetryOk ? "ok" : "fail";
                    var _url = _callbackCfg.FirstOrDefault(c => c.CallbackKey == _c.CallbackKey);
                    callbackResult.Add("{0}，{1}".Fomart(_result, _url == null ? string.Empty : _url.Url));
                });
                //add message status async
                //消息状态表不存在则写入消息
                if (!MessageAppService.ExistsPushMessageStatus(msg._id, msg.AppId, msg.Code))
                {
                    try
                    {
                        MessageAppService.SaveMessageStatus(
                            new MQMessageStatus(msg.MessageId, MessagePublishStatus.PushOk, msg.AppId,
                                _MessageSource.MessageSource_Retry, callbackResult.ToArray(), msg._id), msg.AppId,
                            msg.Code);
                    }
                    catch (Exception e)
                    {

                    }
                }
                log.Debug("[UpdaeMessagePushStatus] message source MessageSource_Publish,save push status success,mid:{0},appid:{1},code:{2}",
                    msg.MessageId, msg.AppId, msg.Code);
            }
            log.Debug("[UpdaeMessageReterStatus] message retry done，update status，id:{0},status:{1},timerKey:{2}", msg.MessageId, status,timerKey);
        }

        private static void AddCache(string key)
        {
            if (cache.Count >= 1000000) cache.Clear();
            cache.TryAdd(key, 1);
        }
    }
}
