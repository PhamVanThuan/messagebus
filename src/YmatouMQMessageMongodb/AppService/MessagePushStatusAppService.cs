using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YmatouMQ.Common;
using YmatouMQ.Common.Utils;
using YmatouMQ.Log;
using YmatouMQMessageMongodb.Domain.Module;

namespace YmatouMQMessageMongodb.AppService
{
    public class MessagePushStatusAppService
    {
        private static readonly ILog log = LogFactory.GetLogger(LogEngineType.RealtimelWriteFile, "YmatouMQMessageMongodb.AppService.MessagePushStatusAppService");
        private static readonly _TimerBatchRetryEnQueueWrapper2<MessagePushStatus2> mcQueueWrapper2 =
            new _TimerBatchRetryEnQueueWrapper2<MessagePushStatus2>(
                new _TimerBatchRetryEnQueueWrapper2<MessagePushStatus2>.Strategy
                {
                    action = async (msg, token) => await Handle(msg, token).ConfigureAwait(false),
                    addTimeOutMillisecondes = 3000,
                    batch_Size = 100,
                    concurrent = 1,
                    errorHandle = ex => log.Error("update message status error ", ex.ToString()),
                    max = 10000000,
                    timer_CycleMilliseconds = 2000
                });

        public static _TimerBatchRetryEnQueueWrapper2<MessagePushStatus2> QueueWrapper
        {
            get { return mcQueueWrapper2; }
        }

        public static void RunTask()
        {
           mcQueueWrapper2.Start();
        }

        public static void Stop()
        {
            mcQueueWrapper2.Stop();
        }

        private static Task<IEnumerable<MessagePushStatus2>> Handle(IEnumerable<MessagePushStatus2> arr,
            CancellationToken token)
        {
            return Task.Factory.StartNew(() =>
            {
                var stopwatch = Stopwatch.StartNew();
                var updateResult = MessageAppService.TryUpdateMessagePushStatus(arr);             
                stopwatch.Stop();
                log.Debug("![MessagePushStatusAppService] update message push status done，run：{0:N0} ms，count:{1},retry enqueu count:{2}",
                    stopwatch.ElapsedMilliseconds,
                    arr.Count(), updateResult.Count());
                return updateResult;
            }, token);

            #region

            //            var stopwatch = Stopwatch.StartNew();
            //            var updateResult = MessageAppService.TryFindAndUpdateMultipleMessageStatus(arr, MQMessage.ClientAlreadyPush);
            //            var subIds = arr.Except(updateResult);
            //            stopwatch.Stop();
            //            log.Debug("update message status done，run：{0:N0} ms，message count:{1}", stopwatch.ElapsedMilliseconds,
            //            arr.Count());
            //             return Task.FromResult(subIds);

            #endregion
        }
    }
}
