using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Threading;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using YmatouMQMessageMongodb.Domain.Domain;
using YmatouMQMessageMongodb.Repository;
using YmatouMQMessageMongodb.Domain.Module;
using YmatouMQMessageMongodb.Domain.Specifications;
using YmatouMQ.Common.Utils;
using YmatouMQ.Common;
using YmatouMQ.Log;
using YmatouMQ.Common.Extensions;
using YmatouMQ.Common.Extensions.Serialization;
using System.Collections.Concurrent;

namespace YmatouMQMessageMongodb.AppService
{
    /// <summary>
    /// 重试消息mongodb 批量操作
    /// </summary>
    public class RetryMessageAppService_Batch
    {
        private static readonly Lazy<RetryMessageAppService_Batch> lazy = new Lazy<RetryMessageAppService_Batch>(() => new RetryMessageAppService_Batch());
        private readonly ILog log = LogFactory.GetLogger(LogEngineType.RealtimelWriteFile, "YmatouMQMessageMongodb.AppService.RetryMessageAppService_Batch");
        private readonly IRetryMessageRepository repo = new RetryMessageRepository();
        private readonly SemaphoreSlim slim;
        private _TimerBatchQueueWrapper<RetryMessage> tbatch;
        public static RetryMessageAppService_Batch Instance { get { return lazy.Value; } }
        private RetryMessageAppService_Batch()
        {           
            if ("ReptryMessage_Batch_Timer".GetAppSettings("0").ToInt32(0) > 0)
            {
                this.tbatch = new _TimerBatchQueueWrapper<RetryMessage>(
                        "ReptryMessage_Batch_Timer".GetAppSettings("3000").ToInt32(3000)
                       , "ReptryMessage_Batch_Size".GetAppSettings("1000").ToInt32(1000)
                       , "ReptryMessage_MemoryQueue_Size".GetAppSettings("100000").ToInt32(100000)
                       , async (m, token) => await BatchAddRetryMessageAsync(m, token).ConfigureAwait(false)
                       , errorHandle: ex => ErrorHandle(ex)
                       , sendTimeOutMilliseconds: "ReptryMessage_Send_TimeOut".GetAppSettings("5000").ToInt32(5000));
                this.slim = new SemaphoreSlim(1, 1);
            }
        }

        public async Task AddRetryMessageAsync(RetryMessage messages)
        {
            try
            {
                if ("ReptryMessage_Batch_Timer".GetAppSettings("0").ToInt32(0) > 0)
                {
                    var result = await tbatch.SendAsync(messages).ConfigureAwait(false);
                    log.Debug("[RetryMessageAppService_Batch.PostMessageAsync] SendAsync result:{0},delay:{1} ms,message id:{2}",
                        result, "ReptryMessage_Batch_Timer".GetAppSettings("0"), messages.MessageId);
                }
                else
                {
                    await InsertCompensateMessage(messages);
                }
            }
            catch (AggregateException ex)
            {
                log.Error("AddRetryMessageAsync err.0", ex);
            }
            catch (Exception ex)
            {
                log.Error("AddRetryMessageAsync err.1", ex);
            }
        }
        public void StartBatchAddJob()
        {
            if ("ReptryMessage_Batch_Timer".GetAppSettings("0").ToInt32(0) > 0)
                this.tbatch.Start();
            this.log.Debug("RetryMessageAppService_Batch init success");
        }
        public void StopBatchAddJob()
        {
            if ("ReptryMessage_Batch_Timer".GetAppSettings("0").ToInt32(0) > 0)
                tbatch.Stop();
        }
        public Task InsertCompensateMessage(RetryMessage msg)
        {
            return repo.AddAsync(msg, RetryMessageSpecifications.GetCompensateMessageDbName()
                 , RetryMessageSpecifications.CollectionName(msg.AppId, msg.Code));
        }
        private async Task BatchAddRetryMessageAsync(IEnumerable<RetryMessage> messages, CancellationToken token)
        {
            using (var mm = new MethodMonitor(null, 10))
            {
                if (token.IsCancellationRequested)
                {
                    log.Debug("retryMessage BatchAddRetryMessageAsync timeOUt");
                    return;
                }
                await RetryMessageGroup(messages)
                     .EachActionAsync(async m => await repo.BatchAddAsync(m.Message
                         , RetryMessageSpecifications.GetCompensateMessageDbName()
                         , RetryMessageSpecifications.CollectionName(m.AppId, m.Code)), slim)
                         .ConfigureAwait(false);
                log.Info("batch retryMessage insert mongodb,count {0},run {1} ms", messages.Count(), mm.GetRunTime2);
            }
        }
        private void ErrorHandle(Exception ex)
        {
            log.Error("RetryMessageAppService_Batch exception ", ex);
        }
        private IEnumerable<_RetryMessage> RetryMessageGroup(IEnumerable<RetryMessage> messages)
        {
            var list = new ConcurrentBag<_RetryMessage>();
            var codes = messages.AsParallel().Select(e => e.Code).Distinct();
            //var codes = messgaeList.AsParallel().Select(e => e.Code).DistinctBy();
            codes.EachAction(c =>
            {
                var messge = messages.AsParallel().Where(_c => _c.Code == c);
                var m = new _RetryMessage
                {
                    AppId = messge.First().AppId,
                    Code = c,
                    Message = messge.ToList()
                };
                list.Add(m);
            });
            return list;
        }
        class _RetryMessage
        {
            public string AppId { get; set; }
            public string Code { get; set; }
            public List<RetryMessage> Message { get; set; }
        }
    }
}
