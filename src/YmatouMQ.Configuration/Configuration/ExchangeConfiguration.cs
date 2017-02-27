using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace YmatouMQNet4.Configuration
{
    /// <summary>
    /// 交换机属性配置
    /// </summary>
    [DataContract(Name="exchangeCfg")]
    public class ExchangeConfiguration
    {
        /// <summary>
        /// 交换机名称
        /// </summary>
        [DataMember(Name = "exchangeName")]
        public string ExchangeName { get; set; }
        /// <summary>
        /// 交换机类型
        /// </summary>
        [DataMember(Name = "type")]
        public ExchangeType? _ExchangeType { get; set; }
        /// <summary>
        /// 自动删除
        /// </summary>
        [DataMember(Name = "autoDel")]
        public bool? IsExchangeAutoDelete { get; set; }
        /// <summary>
        /// 交换机是否持久化
        /// </summary>
        [DataMember(Name = "durable")]
        public bool? Durable { get; set; }
        /// <summary>
        /// 其他属性。如 Alternate Exchanges 
        /// </summary>
        [DataMember(Name = "exArgs")]
        public IDictionary<string, object> Arguments { get; set; }
    }
}
