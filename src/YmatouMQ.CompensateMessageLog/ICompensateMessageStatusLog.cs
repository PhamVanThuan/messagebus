using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YmatouMQ.Common;
using YmatouMQ.Common.Extensions;
using YmatouMQ.Common.MessageHandleContract;
using YmatouMQ.Common.Utils;
using YmatouMQ.ConfigurationSync;
using YmatouMQ.Log;
using YmatouMQMessageMongodb.AppService;
using YmatouMQMessageMongodb.AppService.Configuration;
using YmatouMQMessageMongodb.Domain.IRepository;
using YmatouMQMessageMongodb.Domain.Module;
using YmatouMQMessageMongodb.Domain.Specifications;
using YmatouMQMessageMongodb.Repository;

namespace YmatouMQ.CompensateMessageLog
{
    public class CompensateMessageStatusLog
    {
        private static IMessageRepository mesageRepository = new MQMessageRepository();
        private static readonly MQAppConfigurationAppService cfgAppService = new MQAppConfigurationAppService();
        private static readonly ILog log = LogFactory.GetLogger(LogEngineType.RealtimelWriteFile,
            "YmatouMQ.CompensateMessageLog.MessageLog");
        private static readonly IMessageStatusRepository statusRepository = new MessageStatusRepository();
        private static readonly RetryMessageCompensateAppService retryMessageAppService =
            new RetryMessageCompensateAppService();
        private static Thread th;
        private static bool running;
        public static void Start()
        {
            if ("MessageStatusBC_Open".GetAppSettings("1") == "0")
            {
                log.Debug("[CompensateMessagePushStatusLog] MessageStatusBC_Open:{0}", "MessageStatusBC_Open".GetAppSettings("1"));
                return;
            }
            running = true;
            var delayMillisecond = "MessageStatusBC_DelayMillisecond".GetAppSettings("30000").ToInt32(30000);
            log.Debug("[CompensateMessagePushStatusLog] task DelayMillisecond {0}", delayMillisecond);
            th = new Thread(async () =>
            {
                while (running)
                {
                    try
                    {
                        var watch = Stopwatch.StartNew();
                        log.Debug("[CompensateMessagePushStatusLog] begin run ExecuteCompensateMessage");
                        await ExecuteCompensateMessage().ConfigureAwait(false);
                        watch.Stop();
                        log.Debug(
                            "[CompensateMessagePushStatusLog] ExecuteCompensateMessage run end，Execute {0:N0} ms",
                            watch.ElapsedMilliseconds);
                        Thread.Sleep(delayMillisecond);
                    }
                    catch (Exception ex)
                    {
                        log.Error("[CompensateMessagePushStatusLog] execute ExecuteCompensateMessage  error", ex);
                    }                    
                }
            }) {IsBackground = true};
            th.Start();
        }

        public static void Stop()
        {
            running = false;
            //TODO 安全停止线程
        }

        private static async Task ExecuteCompensateMessage()
        {
            //获取所有库名
            var dbNames = GetMessageLogDbNames();
            if (dbNames.IsEmptyEnumerable())
            {
                log.Debug("[CompensateMessagePushStatusLog] GetMessageLogDbNames is null.");
                return;
            }
            //获取需要补单的库（配置）
            var includeDb = ConfigurationManager.AppSettings["Include_db"];
            if (!includeDb.IsEmpty() && includeDb != "all")
            {
                log.Debug("[CompensateMessagePushStatusLog] include db->{0}", includeDb);
                var includeDbArray = includeDb.Split(new char[] {','});
                //过滤出需要补单的库
                dbNames = dbNames.Where(d => includeDbArray.Contains(d));
            }
            if (dbNames.IsEmptyEnumerable())
            {
                log.Debug("[CompensateMessagePushStatusLog] dbNames is null.");
                return;
            }
            //遍历所有需要补单的库
            await dbNames.ForEachAsync(Environment.ProcessorCount, async db =>
            {
                //查找当前库下的所有表
                var allCollections = mesageRepository.FindAllCollections(db)
                    .Where(cName => cName.StartsWith("Message_"));
                if (allCollections.Any())
                    log.Debug("[CompensateMessagePushStatusLog] db {0} collections->{1}", db, allCollections.Count());
                //filter tables(排除不需要补单的表)
                var excludeTables = ConfigurationManager.AppSettings["exclude_table"];
                if (!string.IsNullOrEmpty(excludeTables))
                {
                    log.Debug("[CompensateMessagePushStatusLog] exclude tables->{0}", excludeTables);
                    var excludeTable = excludeTables.Split(new char[] {','});
                    allCollections = allCollections.Where(_c => !excludeTable.Contains(_c));
                }
                //遍历所有需要补单的表
                await allCollections.ForEachAsync(Environment.ProcessorCount, async c =>
                {
                    var time = "scan_time".GetAppSettings("00:03:00,10,60").Split(new char[] { ',' });
                    //补单开始时间，当前时间向前推配置时间
                    var startTime = DateTime.Now.Subtract(TimeSpan.Parse(time[0]));
                    //补单结束时间，当前时间减去延迟更新推送状态时间
                    var endTime = DateTime.Now.AddSeconds(-Convert.ToInt32(time[1]));
                    //翻页数据大小
                    var pageSize = Convert.ToInt32(time[2]);
                    //补偿30s的数据
                    var totalSeconds = Convert.ToInt32(endTime.Subtract(startTime).TotalSeconds);
                    var pageCount = (totalSeconds / pageSize) + (totalSeconds % pageSize > 0 ? 1 : 0);
                    log.Debug("pageCount:{0},startTime:{1},endTime:{2},db:{3},tableName:{4}",pageCount,startTime,endTime,db,c);
                    for (var h = 0; h < pageCount; Interlocked.Increment(ref h))
                    {
                        var etime = startTime.AddSeconds(h * pageSize);
                        var stime = etime.AddSeconds(-pageSize);
                        if (h == pageCount - 1)
                            etime = endTime;
                        //查找规定时间段内没有推送的消息（pushstatus:0）& 消息创建超过指定的时间（默认10s）
                        IEnumerable<MQMessage> message = Enumerable.Empty<MQMessage>();
                        try
                        {
                           message = mesageRepository.Find(
                           MQMessageSpecifications.MatchMessagePushStatus(MQMessage.Init, stime, etime), db, c)
                           .SetLimit(pageSize)
                           .ToList().Where(m => DateTime.Now.Subtract(m.CreateTime.ToLocalTime()).TotalSeconds > Convert.ToInt32(time[1]));
                        }
                        catch (Exception ex)
                        {
                            log.Error("[CompensateMessagePushStatusLog] findMessage exception,db:{0},table:{1},ex:{2} ",db,c,ex.ToString());
                        }
                      
                        if (message.Any())
                            log.Debug("[CompensateMessagePushStatusLog] [NoPushMessage] db->{0},table->{1},MessageCount->{2},scan time->{3}~{4}", db, c,
                                message.Count(),
                                stime,
                                etime);
                        if (message.Any())
                        {                          
                            //查找状态表中存在的消息
                            var _messageStatusIdArray = statusRepository.Find(
                                MQMessageSpecifications.MatchInMessageStatusId(message.Select(q => q.MsgId))
                                , "MQ_Message_Status_{0}".Fomart(DateTime.Now.ToString("yyyyMM")),
                                "mq_subscribe_ok_{0}".Fomart(message.First().AppId)
                                ).SetFields("_id").SetLimit(pageSize);
                            if (_messageStatusIdArray.Any())
                            {
                                log.Debug("[CompensateMessagePushStatusLog] [AlreadyPushMessage] message status info: db->{0},tb->{1},appid->{2},code->{3},status count->{4}",
                                    "MQ_Message_Status_{0}".Fomart(DateTime.Now.ToString("yyyyMM")),
                                    "mq_subscribe_ok_{0}".Fomart(message.First().AppId),
                                    message.First().AppId,
                                    message.First().Code,
                                    _messageStatusIdArray.Count());
                            }
                            //如果状态表存在ID则获取差积 ,否则则全部补偿 
                            var messageStatusIds = Enumerable.Empty<string>();
                            var exceptMessageId = Enumerable.Empty<string>();
                            if (_messageStatusIdArray.Any())
                            {
                                messageStatusIds = _messageStatusIdArray.Select(m => m._sid);
                                //获取不存在状态表的消息Id
                                exceptMessageId = message.Select(_m => _m._id).Except(messageStatusIds);
                                message = message.Where(_m => exceptMessageId.Contains(_m._id)).ToList();                                
                            }
                            //获取需要补偿的消息
                            if (exceptMessageId.Any())
                            {
                                log.Debug(
                                    "[CompensateMessagePushStatusLog] [needRetryMessageCount] db->{0},table->{1},needRetryMessageCount->{2},message count->{3},status count->{4}",
                                    db, c, exceptMessageId.Count(), message.Count(), messageStatusIds.Count());
                            }
                            //如果存在需要补偿的消息则写入补单库
                            if (message.Any())
                            {
                                var listMessage = new ConcurrentBag<RetryMessage>();
                                message.EachAction(__m =>
                                {                                    
                                    //获取配置
                                    var cfg = MQMainConfigurationManager.Builder.GetConfiguration(__m.AppId, __m.Code);
                                    var callbackList =
                                        cfg.CallbackCfgList.Where(__ => __.Enable != null && __.Enable.Value == true);
                                    //如果存在需要会调的业务端则写入补单库
                                    if (callbackList.Any())
                                    {
                                        if (retryMessageAppService.CheckExists(__m._id, __m.AppId, __m.Code))
                                        {
                                            log.Debug("[CompensateMessagePushStatusLog] [MessageAlreadyRetry] db:{0},tableName:{1}message mid:{1},uuid:{2} already exists return.",db,c, __m.MsgId, __m._id);
                                        }
                                        else
                                        {
                                            var retryMessageInfo = new RetryMessage(__m.AppId, __m.Code, __m.MsgId, __m.Body,
                                           DateTime.Now.AddMinutes(cfg.ConsumeCfg.RetryTimeOut.Value),
                                           callbackList.Select(_c => _c.CallbackKey).ToList()
                                           , desc: "NoPush"
                                           , uuid: __m._id
                                           , messageSource:_MessageSource.MessageSource_Publish);
                                            listMessage.Add(retryMessageInfo);
                                        }                                       
                                    }
                                });
                                //如果存在消息则写入
                                if (listMessage.Any())
                                {
                                    //对未推送的消息计数
//                                    using (var mm = Ymatou.PerfMonitorClient.MethodMonitor.New("NoPushTotal"))
//                                    {
//
//                                    }
                                    //消息加入补单库
                                    retryMessageAppService.BatchAddMessage(listMessage, message.First().AppId,
                                        message.First().Code);
                                    //消息状态更新为已补单状态
                                    await MessageAppService.TryUpdateMultipleMessageStatusTask(
                                        message.Select(__m => __m._id)
                                        , MQMessage.AlreadyRetry, message.First().AppId, message.First().Code)
                                        .ConfigureAwait(false);
                                    //记录日志
                                    log.Debug(
                                        "**[CompensateMessagePushStatusLog] [NoPushMessage] save success. db->{0},tables->{1},RealNeedRetryMessage Count->{2},appid->{3},code->{4} db save ok",
                                        db, c,
                                        listMessage.Count, message.First().AppId, message.First().Code);
                                }
                            }
                        }
                    }
                });
            });
        }

        private static IEnumerable<string> GetMessageLogDbNames()
        {
            var allAppIds = cfgAppService.FindAllAppId();
            log.Debug("[CompensateMessagePushStatusLog] get all appid count {0}", allAppIds.Count());
            return allAppIds.Select(a => "MQ_Message_{0}_{1}".Fomart(a, DateTime.Now.ToString("yyyyMM")));
        }
    }
}
