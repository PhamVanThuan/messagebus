using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using RabbitMQ.Client;
using YmatouMQNet4.Configuration;
using YmatouMQ.Common.Extensions;
using System.Threading.Tasks;

namespace YmatouMQNet4.Core
{
    /// <summary>
    /// 发布消息基类
    /// </summary>
    internal abstract class PublishMessageBase
    {       

        /// <summary>
        /// 发布消息(同步模式）
        /// </summary>      
        public abstract void PublishMessage(PublishMessageContextSync message);

        /// <summary>
        /// 发布消息（异步模式）
        /// </summary>       
        public abstract Task PublishMessageAsync(PublishMessageContextAsync message);
      
        /// <summary>
        /// 声明发布属性
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="exchangeCfg"></param>
        /// <param name="msgProperties"></param>
        /// <returns></returns>
        protected virtual IBasicProperties PublishDeclare(IModel channel
            , MessagePropertiesConfiguration msgProperties, string msgid = null, string uuid = null)
        {
            //声明特性
            var pubProper = channel.CreateBasicProperties();
            pubProper.ContentEncoding = msgProperties.ContentEncoding;
            pubProper.ContentType = msgProperties.ContextType;
            pubProper.SetPersistent(msgProperties.PersistentMessages.Value);
            //msgProperties.Priority.NullAction(v => pubProper.Priority = v.ToByte(0));
            msgProperties.Expiration.NullAction(v => pubProper.Expiration = v.ToString());
            pubProper.Headers = new Dictionary<string, object>();
            msgid.NullObjectReplace(v => pubProper.Headers["msgid"] = v, null);
            pubProper.Headers["uuid"] = uuid ?? Guid.NewGuid().ToString("N");
            return pubProper;
        }
    }
}
