using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YmatouMQ.Common.Utils;
using YmatouMQMessageMongodb.Domain.IRepository;
using YmatouMQMessageMongodb.Domain.Module;
using YmatouMQMessageMongodb.Domain.Specifications;
using YmatouMQMessageMongodb.Dto;
using YmatouMQMessageMongodb.Repository;
using YmatouMQ.Common.Extensions;
using YmatouMQ.Common.MessageHandleContract;
using System.Threading;
using YmatouMQ.Common;
using YmatouMQ.Log;
using YmatouMQ.Common.Extensions.Serialization;
using System.Collections.Concurrent;

namespace YmatouMQMessageMongodb.AppService
{
    /// <summary>
    /// 业务端处理消息结果批量写入mongodb
    /// </summary>
    public class MessageHandleStatusAppService_Batch
    {
        private static readonly Lazy<MessageHandleStatusAppService_Batch> lazy = new Lazy<MessageHandleStatusAppService_Batch>(() => new MessageHandleStatusAppService_Batch());
        private readonly IMessageStatusRepository statusRepo;
        private readonly _TimerBatchQueueWrapper<MQMessageStatus> tbatch;
        private readonly SemaphoreSlim slim;
        private readonly ILog log = LogFactory.GetLogger(LogEngineType.RealtimelWriteFile, "YmatouMQMessageMongodb.AppService.MQMessageAppService");

        public static MessageHandleStatusAppService_Batch Instance { get { return lazy.Value; } }
        private MessageHandleStatusAppService_Batch()
        {
            this.statusRepo = new MessageStatusRepository();
            if ("MessageStatus_Batch_Timer".GetAppSettings("0").ToInt32(0) > 0)
            {
                this.tbatch = new _TimerBatchQueueWrapper<MQMessageStatus>(
                        "MessageStatus_Batch_Timer".GetAppSettings("3000").ToInt32(3000)
                        , "MessageStatus_Batch_Size".GetAppSettings("1000").ToInt32(1000)
                         , "MessageSutatus_MemoryQueue_Size".GetAppSettings("100000").ToInt32(100000)
                        , async (m, token) => await BatchAddMessageStatusAsync(m, token).ConfigureAwait(false)
                        , errorHandle: ex => ErrorHandle(ex)
                        , sendTimeOutMilliseconds: "MessageSutatus_Send_TimeOut".GetAppSettings("5000").ToInt32(5000));
                this.slim = new SemaphoreSlim(1, 1);
                // this.StartBatchAddJob();
            }
        }

        public Task BatchAddMessageStatusAsync(IEnumerable<MQMessageStatus> messages, string appid, string status)
        {
            using (var mm = new MethodMonitor(log, 10, "BatchAddMessageStatusAsync.0"))
                return statusRepo.TryBatchAddAsync(messages, MQMessageStatus.GetDbName()
                    , MQMessageStatus.GetCollectionName(appid), TimeSpan.FromSeconds(5));
        }
        public Task AddMessageStatusAsync(MQMessageStatus messages, string appid)
        {
            return statusRepo.TryAddAsync(messages
                , MQMessageStatus.GetDbName()
                , MQMessageStatus.GetCollectionName(appid)
                , TimeSpan.FromSeconds("SubMessage_WriteMongodbTimeOut_Seconds".GetAppSettings("3").ToInt32(3)));
        }

        public void SaveMQMessageStatus(MQMessageStatus messages)
        {
            statusRepo.Save(messages, MQMessageStatus.GetDbName(),
                MQMessageStatus.GetCollectionName(messages.AppId));
        }

        public void StartBatchAddJob()
        {
            if ("MessageStatus_Batch_Timer".GetAppSettings("0").ToInt32(0) > 0)
                this.tbatch.Start();
            log.Debug("MQMessageHandleStatusAppService 初始化完成");
        }
        public async Task SaveMessageStatusAsync(MQMessageStatus messages)
        {
            try
            {
                //消息重复推送，则直接更新状态，否则批量插入
                if (messages.IsRepeat)
                {
                    await Task.Factory.StartNew(() => SaveMQMessageStatus(messages)).ConfigureAwait(false);
                }
                else
                {
                    if ("MessageStatus_Batch_Timer".GetAppSettings("0").ToInt32(0) > 0)
                    {
                        var result = await tbatch.SendAsync(messages).ConfigureAwait(false);
                        if (!result)
                            log.Debug("PostMessageAsync fail,message {0}", messages.JSONSerializationToString());
                    }
                    else
                    {
                        await AddMessageStatusAsync(messages, messages.AppId);
                    }
                }
            }
            catch (AggregateException ex)
            {
                log.Error("AddMessageStatusAsync error.0 {0},{1}", messages.JSONSerializationToString(), ex.ToString());
            }
            catch (Exception ex)
            {
                log.Error("AddMessageStatusAsync error.1 {0},{1}", messages.JSONSerializationToString(), ex.ToString());
            }
        }
        public void StopBatchAddJob()
        {
            if ("MessageStatus_Batch_Timer".GetAppSettings("0").ToInt32(0) > 0)
                tbatch.Stop();
        }
        private async Task BatchAddMessageStatusAsync(IEnumerable<MQMessageStatus> messages, CancellationToken token)
        {
            using (var mm = new MethodMonitor(log, 10, "BatchAddMessageStatusAsync"))
            {
                if (token.IsCancellationRequested)
                {
                    log.Debug("statusRepo BatchAddMessageStatusAsync timeOut");
                    return;
                }
                await GroupHandleMessage(messages)
                     .EachActionAsync(async e =>
                     {
                         await statusRepo.TryBatchAddAsync(e.Messages
                             , MQMessageStatus.GetDbName()
                             , MQMessageStatus.GetCollectionName(e.AppId)
                             , TimeSpan.FromSeconds(5));
                         log.Info("statusRepo.BatchAddAsync appid {0},count {1}", e.AppId, e.Messages.Count());
                     }, slim);
            }
        }
        private void ErrorHandle(Exception ex)
        {
            log.Error("MQMessageHandleStatusAppService exception timeOut ", ex);
        }
        private IEnumerable<_HandlehMessage> GroupHandleMessage(IEnumerable<MQMessageStatus> message)
        {
            var list = new ConcurrentBag<_HandlehMessage>();
            message.GroupBy(e => e.AppId).EachAction(e =>
            {
                list.Add(new _HandlehMessage
                {
                    AppId = e.Key,
                    Messages = e.Select(_m => _m)
                });
            });
            return list;
        }
        class _HandlehMessage
        {
            public string AppId { get; set; }
            public IEnumerable<MQMessageStatus> Messages { get; set; }
        }
    }
}
