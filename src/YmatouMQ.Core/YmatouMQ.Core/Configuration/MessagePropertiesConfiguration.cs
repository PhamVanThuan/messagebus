using System;
using System.Runtime.Serialization;

namespace YmatouMQNet4.Configuration
{
    /// <summary>
    /// 消息特性配置
    /// </summary>
    [DataContract(Name = "msgPropert")]
    public class MessagePropertiesConfiguration
    {
        /// <summary>
        /// 消息媒体类型（JSON，Binary）。      
        /// </summary>
        /// <remarks>  
        /// 支持：application/x-dotnet-serialized-object （二进制）
        ///       application/json (JSON)
        ///       </remarks>
        [DataMember(Name = "cType")]
        public string ContextType { get; set; }
        /// <summary>
        /// 消息编码
        /// </summary>
        [DataMember(Name = "encoding")]
        public string ContentEncoding { get; set; }
        /// <summary>
        /// 消息过期时间（毫秒）
        /// </summary>
        [DataMember(Name = "ttl")]
        public long? Expiration { get; set; }
        /// <summary>
        /// 消息是在RabbitMQ否持化
        /// </summary>
        [DataMember(Name = "persistentMsg")]
        public bool? PersistentMessages { get; set; }
        /// <summary>
        /// 消息是否持久到本地（异常情况）[功能未实现]
        /// </summary>
        [DataMember(Name = "persistentMsgLocal")]
        public bool? PersistentMessagesLocal { get; set; }
        /// <summary>
        ///  消息是否持久到（mongodb）
        /// </summary>
        [DataMember(Name = "persistentMsgMongo")]
        public bool? PersistentMessagesMongo { get; set; }
        /// <summary>
        /// 消息优先级别（0-9）
        /// </summary>
        [DataMember(Name = "priority")]
        public byte? Priority { get; set; }
    }
}
