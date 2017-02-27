using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace YmatouMQNet4.Configuration
{
    /// <summary>
    /// 消息属性配置
    /// </summary>
    [DataContract(Name = "msgCfg")]
    public class MessageConfiguration
    {
        /// <summary>
        /// code
        /// </summary>
        [DataMember(Name = "code")]
        public string Code { get; set; }
        /// <summary>
        /// 是否启用
        /// </summary>
        [DataMember(Name = "enable")]
        public bool Enable { get; set; }
        /// <summary>
        /// 消息特性配置
        /// </summary>
        [DataMember(Name = "msgPropert")]
        public MessagePropertiesConfiguration MessagePropertiesCfg { get; set; }
        /// <summary>
        /// 发布属性配置
        /// </summary>
        [DataMember(Name = "pubCfg")]
        public PublishConfiguration PublishCfg { get; set; }
        /// <summary>
        /// 消费属性配置
        /// </summary>
        [DataMember(Name = "consumeCfg")]
        public ConsumeConfiguration ConsumeCfg { get; set; }
        /// <summary>
        /// 交换机属性配置
        /// </summary>
        [DataMember(Name = "exchangeCfg")]
        public ExchangeConfiguration ExchangeCfg { get; set; }
        /// <summary>
        /// 队列属性配置
        /// </summary>
        [DataMember(Name = "queueCfg")]
        public QueueConfiguration QueueCfg { get; set; }
        /// <summary>
        /// 回调业务配置
        /// </summary>
        [DataMember(Name = "callbackCfgs")]
        public IEnumerable<CallbackConfiguration> CallbackCfgList { get; set; }
    }
}
