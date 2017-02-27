using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
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

namespace YmatouMQMessageMongodb.AppService
{
    public class MessageAppService_TimerBatch
    {
        private static readonly Lazy<MessageAppService_TimerBatch> lazy = new Lazy<MessageAppService_TimerBatch>(() => new MessageAppService_TimerBatch());
        private readonly ILog log = LogFactory.GetLogger(LogEngineType.RealtimelWriteFile, "YmatouMQMessageMongodb.AppService.MQMessageAppService");
        private readonly IMessageRepository messageRepo;
        private readonly _TimerBatchQueueWrapper<MQMessage> tbatch;
        private readonly SemaphoreSlim slim;
        public static MessageAppService_TimerBatch Instance { get { return lazy.Value; } }
        public MessageAppService_TimerBatch()
        {
            this.messageRepo = new MQMessageRepository();

            if ("MessageBatch_Insert_Mongo_Timer".GetAppSettings("0").ToInt32(0) > 0)
            {
                this.tbatch = new _TimerBatchQueueWrapper<MQMessage>(
                    "MessageBatch_Insert_Mongo_Timer".GetAppSettings("2000").ToInt32(2000)
                    , "MessageBatch_Insert_Mongo_Szie".GetAppSettings("1000").ToInt32(1000)
                    , "MemoryQueueSize".GetAppSettings("100000").ToInt32(100000)
                    , async (m, token) => await BatchInsert(m, token).ConfigureAwait(false)
                    , errorHandle: ex => ErrorHandle(ex)
                    , sendTimeOutMilliseconds: "SendTimeOut".GetAppSettings("3000").ToInt32(3000)
                    );
            }
            this.slim = new SemaphoreSlim(1, 1);
            this.log.Debug("MessageAppService_TimerBatch (batch write mongodb) init ok,timer:{0}".Fomart("MessageBatch_Insert_Mongo_Timer".GetAppSettings()));
        }
        public Task BatchAddMessageAsync(IEnumerable<MQMessage> messages, string appid, string code)
        {
            return messageRepo.BatchAddAsync(messages, MessageDbCollections.GenerateDbName(appid)
                , MessageDbCollections.GenerateCollectionsName(code));
        }
        public void StopJob()
        {
            if ("MessageBatch_Insert_Mongo_Timer".GetAppSettings("0").ToInt32(0) > 0)
                tbatch.Stop();
        }
        public void StartJob()
        {
            if ("MessageBatch_Insert_Mongo_Timer".GetAppSettings("0").ToInt32(0) > 0)
                tbatch.Start();
            log.Debug("timer batch start success,batch Write Size {0}", "MessageBatch_Insert_Mongo_Timer".GetAppSettings("0"));
        }
        public int Count
        {
            get { return tbatch == null ? 0 : tbatch.Count; }
        }
        public async Task PostMessageAsync(MQMessage message)
        {
            try
            {
                if ("MessageBatch_Insert_Mongo_Timer".GetAppSettings("0").ToInt32(0) > 0)
                {
                    var result = await tbatch.SendAsync(message).ConfigureAwait(false);
                    log.Debug("[MessageAppService_TimerBatch] _TimerBatchQueueWrapper return:{0},delay:{1} ms,to mongodb,message id:{2}", result,
                        "MessageBatch_Insert_Mongo_Timer".GetAppSettings("0"), message.MsgId);
                }
                else
                {
                    var db = MessageDbCollections.GenerateDbName(message.AppId);
                    var tb = MessageDbCollections.GenerateCollectionsName(message.Code);
                    await messageRepo.AddAsync(message
                        , db
                        , tb
                        , TimeSpan.FromSeconds("PubMessage_WriteMongodbTimeOut_Seconds".GetAppSettings("3").ToInt32(3)))
                        .ConfigureAwait(false);
                    log.Debug("[MessageAppService_TimerBatch] message write to mongodb success,db:{0},tb:{1},mid:{2}",
                        db, tb, message.MsgId);
                }
            }
            catch (AggregateException ex)
            {
                log.Error("insert mongodb (AggregateException) {0},{1},{2},{3}", message.AppId, message.Code,
                    message.MsgId, ex.ToString());
            }
            catch (Exception ex)
            {
                log.Error("insert mongodb (Exception){0},{1},{2},{3}", message.AppId, message.Code, message.MsgId,
                    ex.ToString());
            }
        }
        private async Task BatchInsert(IEnumerable<MQMessage> message, CancellationToken token)
        {
            using (var mm = new MethodMonitor(null, 10))
            {
                if (token.IsCancellationRequested)
                {
                    log.Debug("BatchInsert timeOut");
                    return;
                }
                await PublishMessageGroup(message).EachActionAsync(async m => await BatchAddMessageAsync(m.Message, m.AppId, m.Code), slim);
                log.Info("batch insert message count:{0} to mongodb,run:{1:N0} ms", message.Count(), mm.GetRunTime2);
            }
        }
        private void TimeOut()
        {
            log.Error("批量写入mongodb超时");
        }
        private void ErrorHandle(Exception ex)
        {
            log.Error("MQMessageAppService_TimerBatch exception timeOut ", ex);
        }
        private IEnumerable<_PublishMessage> PublishMessageGroup(IEnumerable<MQMessage> messages)
        {
            var list = new ConcurrentBag<_PublishMessage>();
            var codes = messages.AsParallel().Select(e => e.Code).Distinct();
            codes.EachAction(c =>
            {
                var messge = messages.AsParallel().Where(_c => _c.Code == c);
                var m = new _PublishMessage
                {
                    AppId = messge.First().AppId,
                    Code = c,
                    Message = messge.ToList()
                };
                list.Add(m);
            });
            return list;
        }
        class _PublishMessage
        {
            public string AppId { get; set; }
            public string Code { get; set; }
            public List<MQMessage> Message { get; set; }
        }       
    }
}
