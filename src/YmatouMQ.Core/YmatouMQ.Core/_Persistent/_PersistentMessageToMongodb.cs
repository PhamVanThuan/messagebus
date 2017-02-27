using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YmatouMQMessageMongodb.AppService;
using YmatouMQMessageMongodb.Domain.Module;
using YmatouMQNet4.Core;
using YmatouMQNet4.Utils;
using YmatouMQNet4.Extensions;

namespace YmatouMQNet4._Persistent
{
    public class _PersistentMessageToMongodb
    {
        private static readonly ILog log = LogFactory.GetLogger(LogEngineType.RealtimelWriteFile, "YmatouMQNet4._Persistent._PersistentMessageToMongodb");
        private static readonly MQMessageAppService messageAppService = new MQMessageAppService();
        private static readonly TimerBatchBlockWrapper<MQMessage> tbatch = new TimerBatchBlockWrapper<MQMessage>(2000, 1000, async m => await BatchInsert(m).ConfigureAwait(false), 100000, sendTimeOutCallback: TimeOut);

        /// <summary>
        /// 标记为完成批处理
        /// </summary>
        public static void StopJob()
        {
            tbatch.Complete(TimeSpan.FromSeconds(3));
        }
        public static void StartJob()
        {
            tbatch.ReceiveAsync();
        }
        public static async Task PostMessageAsync(MQMessage message)
        {
            try
            {
                await tbatch.SendAsync(message);
            }
            catch (Exception ex)
            {
                log.Error("批量写入mongodb 异常 {0},{1},{2},{3}", message.AppId, message.Code, message.MsgId, ex.ToString());
            }
        }
        /// <summary>
        /// 执行批量写入
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private static async Task BatchInsert(IEnumerable<MQMessage> message)
        {
            log.Info("batch insert message {0} to mongodb ", message.Count());
            message
                .GroupBy(e => e.AppId)
                .EachAction(e =>
                {
                    e.GroupBy(c => c.Code).EachAction(async _c =>
                    {
                        await messageAppService
                            .BatchAddMessageAsync(_c.Select(__ => __), e.Key, _c.Key)
                            .ContinueWith(ex => ex.Exception.Handle(log, "BatchInsert error"), TaskContinuationOptions.OnlyOnFaulted).ConfigureAwait(false);
                    });
                });
        }
        private static void TimeOut()
        {
            Console.WriteLine("超时了");
        }
    }
}
