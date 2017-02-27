using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YmatouMQ.Common.Dto;
using YmatouMQNet4.Utils;
using YmatouMQ.Common.Extensions.Serialization;
using YmatouMQ.Common.Extensions;

namespace YmatouMQNet4.Core
{
    /// <summary>
    /// 洋码头MQ BUS
    /// </summary>
    public class MessageBus
    {
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="msg">消息主体</param>
        /// <param name="appId">应用ID</param>
        /// <param name="code">业务Id</param>
        /// <param name="msgId">消息ID</param>
        /// <param name="ip">来源IP</param>
        public static void Publish<TMessage>(TMessage msg, string appId, string code, string msgId, string ip = null)
        {
            YmtSystemAssert.AssertArgumentNotNull(msg, "消息主体不能为空");
            YmtSystemAssert.AssertArgumentNotNull(appId, "appid 不能为空");
            YmtSystemAssert.AssertArgumentNotNull(code, "消息ID不能为空");

            Bus.Builder.Publish(new PublishMessageContext { appid = appId, body = msg, code = code, ip = ip, messageid = msgId, uuid = Guid.NewGuid().ToString("N") });
        }        
        /// <summary>
        /// 同步批量发送消息
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="appId"></param>
        /// <param name="code"></param>
        /// <param name="ip"></param>
        public static void PublishBatch(IEnumerable<MessageItemDto> items, string appId, string code, string ip)
        {
            YmtSystemAssert.AssertArgumentNotNull(items, "消息主体不能为空");
            YmtSystemAssert.AssertArgumentNotNull(appId, "appid 不能为空");
            YmtSystemAssert.AssertArgumentNotNull(code, "消息ID不能为空");

            Bus.Builder.Publish(
                items.CopyTo(
                    m =>
                        new PublishMessageContext
                        {
                            appid = appId,
                            code = code,
                            body = m.Body,
                            ip = ip,
                            messageid = m.MsgUniqueId,
                            uuid = Guid.NewGuid().ToString("N")
                        }));
        }
        public static Task PublishBatchAsync(IEnumerable<MessageItemDto> items, string appId, string code, string ip)
        {          
            YmtSystemAssert.AssertArgumentNotNull(items, "消息主体不能为空");
            YmtSystemAssert.AssertArgumentNotNull(appId, "appid 不能为空");
            YmtSystemAssert.AssertArgumentNotNull(code, "消息ID不能为空");

           return Bus.Builder.PublishAsync(
                items.CopyTo(
                    m =>
                        new PublishMessageContext
                        {
                            appid = appId,
                            code = code,
                            body = m.Body,
                            ip = ip,
                            messageid = m.MsgUniqueId,
                            uuid = Guid.NewGuid().ToString("N")
                        }));
        }
        /// <summary>
        /// 异步发送消息
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="msg">消息主体</param>
        /// <param name="appId">应用ID</param>
        /// <param name="code">业务ID</param>
        /// <param name="msgId">消息ID</param>
        public static Task PublishAsync<TMessage>(TMessage msg, string appId, string code, string msgId, string ip = null)
        {
            YmtSystemAssert.AssertArgumentNotNull(msg, "消息主体不能为空");
            YmtSystemAssert.AssertArgumentNotNull(appId, "appid 不能为空");
            YmtSystemAssert.AssertArgumentNotNull(code, "消息ID不能为空");

            return Bus.Builder.PublishAsync(new PublishMessageContext { body = msg, appid = appId, code = code, messageid = msgId, ip = ip, uuid = Guid.NewGuid().ToString("N") });
        }
       
        public static TMessage PullMessage<TMessage>(string appId, string code)
        {
            YmtSystemAssert.AssertArgumentNotNull(appId, "appid 不能为空");
            YmtSystemAssert.AssertArgumentNotNull(code, "业务Id不能为空");

            return Bus.Builder.PullMessage<TMessage>(appId, code);
        }
        public static void PullMessage<TMessage>(string appId, string code, IMessageHandler<TMessage> handle, string msgId)
        {
            YmtSystemAssert.AssertArgumentNotNull(appId, "appid 不能为空");
            YmtSystemAssert.AssertArgumentNotNull(code, "业务Id不能为空");

            Bus.Builder.PullMessage(appId, code, handle);
        }
        public static uint MessageCount(string appId, string code)
        {
            YmtSystemAssert.AssertArgumentNotNull(appId, "appid 不能为空");
            YmtSystemAssert.AssertArgumentNotNull(code, "业务Id不能为空");

            return Bus.Builder.MessageCount(appId, code);
        }
        public static async Task PublishBufferAsync<TMessage>(TMessage msg, string appId, string code, string msgId, string ip = null)
        {
            YmtSystemAssert.AssertArgumentNotNull(msg, "消息主体不能为空");
            YmtSystemAssert.AssertArgumentNotNull(appId, "appid 不能为空");
            YmtSystemAssert.AssertArgumentNotNull(code, "业务Id不能为空");

            await Bus.Builder.PublishBufferAsync(new PublishMessageContext { body = msg, appid = appId, code = code, messageid = msgId, ip = ip, uuid = Guid.NewGuid().ToString("N") }).ConfigureAwait(false);
        }

        public static void PublishToDb<TMessage>(TMessage msg, string appId, string code, string msgId,
            string ip = null)
        {
            YmtSystemAssert.AssertArgumentNotNull(msg, "消息主体不能为空");
            YmtSystemAssert.AssertArgumentNotNull(appId, "appid 不能为空");
            YmtSystemAssert.AssertArgumentNotNull(code, "业务Id不能为空");

            Bus.Builder.PublishToDb(new PublishMessageContext
            {
                appid = appId,
                body = msg,
                code = code,
                ip = ip,
                messageid = msgId,
                uuid = Guid.NewGuid().ToString("N")
            });
        }

        public static void PublishBatchToDb(IEnumerable<MessageItemDto> items, string appId, string code, string ip)
        {
            YmtSystemAssert.AssertArgumentNotNull(appId, "appid 不能为空");
            YmtSystemAssert.AssertArgumentNotNull(code, "业务Id不能为空");
            YmtSystemAssert.AssertArgumentNotNull(items,"消息主体不能为空");
            var list= items.CopyTo(
                m =>
                    new PublishMessageContext
                    {
                        appid = appId,
                        body = m.Body/*._JSONSerializationToString()*/,
                        code = code,
                        ip = ip,
                        messageid = m.MsgUniqueId,
                        uuid = Guid.NewGuid().ToString("N")
                    });
            Bus.Builder.PublishBatchToDb(list, appId, code, ip);
        }

        public static bool RemoveCacheExchang(string appid, string code)
        {
            return Bus.Builder.RemoveCacheExchang(appid, code);
        }
        public static IEnumerable<string> GetConnectionPoolKeys { get { return Bus.Builder.GetConnectionPoolKeys; } }

        public static IEnumerable<string> GetAllChannelStatus
        {
            get { return Bus.Builder.GetChannelsStatus; }
        }

        /// <summary>
        /// 获取BUS应用程序状态
        /// </summary>
        public static BusApplicationStatus BusApplicationStatus { get { return Bus.Builder.BusStatus; } }
        /// <summary>
        /// 停止BUS应用。
        /// </summary>
        public static void StopBusApplication()
        {
            Bus.Builder.StopBusApplication();
        }
        /// <summary>
        /// 启动BUS应用
        /// </summary>
        public static void StartBusApplication()
        {
            Bus.Builder.StartBusApplication();
        }
    }
}
