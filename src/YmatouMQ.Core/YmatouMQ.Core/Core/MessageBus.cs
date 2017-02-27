using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YmatouMQNet4.Connection;
using YmatouMQNet4.Utils;
using YmatouMQNet4.Extensions;

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

            Bus.Builder.Publish(new PublishMessageContext { appid = appId, body = msg, code = code, ip = ip, messageid = msgId });
        }
        /// <summary>
        /// 批量发布消息
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="msg">消息主体</param>
        /// <param name="appId">应用ID</param>
        /// <param name="code">业务ID</param>
        /// <param name="msgId">消息ID</param>
        /// <param name="ip">来源IP</param>
        public static void Publish<TMessage>(IEnumerable<TMessage> msg, string appId, string code, string msgId, string ip = null)
        {
            YmtSystemAssert.AssertArgumentNotNull(msg, "消息主体不能为空");
            YmtSystemAssert.AssertArgumentNotNull(appId, "appid 不能为空");
            YmtSystemAssert.AssertArgumentNotNull(code, "消息ID不能为空");

            Bus.Builder.Publish(msg.CopyTo(m => new PublishMessageContext { appid = appId, code = code, body = msg, ip = ip, messageid = msgId }));
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

            return Bus.Builder.PublishAsync(new PublishMessageContext { body = msg, appid = appId, code = code, messageid = msgId, ip = ip });
        }
        /// <summary>
        /// 异步发送消息
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="msg">消息主体</param>
        /// <param name="appId">应用ID</param>
        /// <param name="code">业务ID</param>
        /// <param name="msgId">消息ID</param>
        /// <param name="ip">来源IP</param>
        public static void PublishAsync<TMessage>(IEnumerable<TMessage> msg, string appId, string code, string msgId, string ip = null)
        {
            YmtSystemAssert.AssertArgumentNotNull(msg, "消息主体不能为空");
            YmtSystemAssert.AssertArgumentNotNull(appId, "appid 不能为空");
            YmtSystemAssert.AssertArgumentNotNull(code, "消息ID不能为空");

            Bus.Builder.PublishAsync(msg.CopyTo(m => new PublishMessageContext { appid = appId, code = code, body = msg, ip = ip, messageid = msgId }));
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

            await Bus.Builder.PublishBufferAsync(new PublishMessageContext { body = msg, appid = appId, code = code, messageid = msgId, ip = ip }).ConfigureAwait(false);
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
