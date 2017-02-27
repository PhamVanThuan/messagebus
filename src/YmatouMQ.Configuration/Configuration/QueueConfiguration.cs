using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace YmatouMQNet4.Configuration
{
    /// <summary>
    /// 队列属性配置
    /// </summary>
    [DataContract(Name = "queueCfg")]
    public class QueueConfiguration
    {
        /// <summary>
        /// 队列名称
        /// </summary>
        [DataMember(Name = "qName")]
        public string QueueName { get; set; }
        /// <summary>
        /// 是否持久化
        /// </summary>
        [DataMember(Name = "isDurable")]
        public bool? IsDurable { get; set; }
        /// <summary>
        /// 是否自动删除
        /// </summary>
        [DataMember(Name = "autoDelete")]
        public bool? IsAutoDelete { get; set; }
        /// <summary>
        /// 是否独占。
        /// 注意：vhost 为 '/' 则不要设置该值
        /// </summary>
        [DataMember(Name = "exclusive")]
        public bool? IsQueueExclusive { get; set; }
        /// <summary>
        /// 其他属性
        /// </summary>
        [DataMember(Name = "qArgs")]
        public IDictionary<string, object> Args { get; set; }
        /// <summary>
        /// 队列头部属性
        /// </summary>
        [DataMember(Name = "headArgs")]
        public IDictionary<string, object> HeadArgs { get; set; }
    }
}
