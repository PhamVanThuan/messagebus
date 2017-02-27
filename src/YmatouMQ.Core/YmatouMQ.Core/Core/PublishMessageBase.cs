using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using RabbitMQ.Client;
using YmatouMQNet4.Configuration;
using YmatouMQNet4.Extensions;
using System.Threading.Tasks;

namespace YmatouMQNet4.Core
{
    /// <summary>
    /// 发布消息基类
    /// </summary>
    internal abstract class PublishMessageBase
    {
        private readonly ConcurrentDictionary<string, ExceptionMessageQueue> exQueue;

        public PublishMessageBase()
        {
            exQueue = new ConcurrentDictionary<string, ExceptionMessageQueue>();
        }
        /// <summary>
        /// 发布消息(同步模式）
        /// </summary>      
        public abstract void PublishMessage(PublishMessageContextSync message);
        /// <summary>
        /// 发布消息（异步模式）
        /// </summary>       
        public abstract Task PublishMessageAsync(PublishMessageContextAsync message);
        /// <summary>
        /// 添加发布异常的消息
        /// </summary>
        /// <param name="messageContext"></param>
        public virtual void AddMessageToExceptionQueue(ExceptionMessageContext messageContext)
        {           
            var q = exQueue.GetOrAdd(messageContext.appId, new ExceptionMessageQueue());
            q.TryAdd(messageContext);
        }
        /// <summary>
        /// 获取发布异常的消息
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public virtual ExceptionMessageQueue GetExceptionMessageContext(string appId)
        {
            ExceptionMessageQueue q;
            if (exQueue.TryGetValue(appId, out  q))
            {
                return q;
            }
            else
            {
                return null;
            }
        }
        protected virtual ConcurrentDictionary<string, ExceptionMessageQueue> ExceptionMessage { get { return this.exQueue; } }
        /// <summary>
        /// 声明发布属性
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="exchangeCfg"></param>
        /// <param name="msgProperties"></param>
        /// <returns></returns>
        protected virtual IBasicProperties PublishDeclare(IModel channel, ExchangeConfiguration exchangeCfg, MessagePropertiesConfiguration msgProperties, string msgid = null)
        {
            //声明特性
            var pubProper = channel.CreateBasicProperties();
            pubProper.ContentEncoding = msgProperties.ContentEncoding;
            pubProper.ContentType = msgProperties.ContextType;
            pubProper.SetPersistent(msgProperties.PersistentMessages.Value);
            msgProperties.Priority.NullAction(v => pubProper.Priority = v);
            msgProperties.Expiration.NullAction(v => pubProper.Expiration = v.ToString());
            msgid.NullObjectReplace(v =>
            {
                pubProper.Headers = new Dictionary<string, object>();
                pubProper.Headers["msgid"] = v;
            }, null);
            return pubProper;
        }
    }
}
