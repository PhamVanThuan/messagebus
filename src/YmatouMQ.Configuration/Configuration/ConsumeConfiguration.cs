using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace YmatouMQNet4.Configuration
{
    /// <summary>
    /// 消费消息属性配置
    /// </summary>
    [DataContract(Name = "consumeCfg")]
    public class ConsumeConfiguration
    {
        /// <summary>
        /// 路由关键字
        /// </summary>
        [DataMember(Name = "routKey")]
        public string RoutingKey { get; set; }
        /// <summary>
        /// 是否自动发送ACK，true 表示：客户端队列接到消息及发送ACK，不等客户端消息处理结束
        /// </summary>
        [DataMember(Name = "autoAck")]
        public bool? IsAutoAcknowledge { get; set; }
        /// <summary>
        /// 客户端消息处理失败是否ack
        /// </summary>
        [DataMember(Name = "handleFailAck")]
        public bool? HandleFailAcknowledge { get; set; }
        /// <summary>
        /// 处理失败重新入队列（0：表示关闭该功能）
        /// </summary>
        [DataMember(Name = "handleFailRQueue")]
        [Obsolete]
        public bool? HandleFailRQueue { get; set; }
        /// <summary>
        /// 预处理大小（流量限制）
        /// </summary>
        [DataMember(Name = "qos")]
        public ushort? PrefetchCount { get; set; }
        /// <summary>
        /// 是否使用多线程回调EventHandler
        /// </summary>
        [DataMember(Name = "useMultipleTh")]
        [Obsolete]
        public bool? UseMultipleThread { get; set; }
        /// <summary>
        /// 每个消费者最大线程数量
        /// </summary>
        [DataMember(Name = "maxTh")]
        public uint? MaxThreadCount { get; set; }
        /// <summary>
        /// 回调地址（http://)
        /// </summary>
        [DataMember(Name = "cUrl")]
        [Obsolete]
        public string CallbackUrl { get; set; }
        /// <summary>
        /// 回调超时时间
        /// </summary>
        [DataMember(Name = "cTimeOut")]
        [Obsolete]
        public int? CallbackTimeOut { get; set; }
        /// <summary>
        /// 回调方式(GET,POST)
        /// </summary>
        [DataMember(Name = "cmType")]
        [Obsolete]
        public string CallbackMethodType { get; set; }
        /// <summary>
        /// 回调超时是否ACK
        /// </summary>
        [DataMember(Name = "ctAck")]
        [Obsolete]
        public bool? CallbackTimeOutAck { get; set; }
        /// <summary>
        /// 重试次数
        /// </summary>
        [DataMember(Name = "cRCount")]
        [Obsolete]
        public uint? RetryCount { get; set; }
        /// <summary>
        /// 重试间隔毫秒
        /// </summary>
        [DataMember(Name = "cRMs")]
        [Obsolete]
        public uint? RetryMillisecond { get; set; }
        /// <summary>
        /// 补偿消息超时时间（分钟，0（默认）表示不补偿）
        /// </summary>        
        [DataMember(Name = "cRTimeOut")]        
        public int? RetryTimeOut { get; set; }
        /// <summary>
        /// 超过重试次数，任然失败消息持久化存储（mongodb）
        /// </summary>
        [DataMember(Name = "cHfps")]
        [Obsolete]
        public bool? HandleFailPersistentStore { get; set; }
        /// <summary>
        /// 处理结果（成功，失败）是否保存到mongodb
        /// </summary>
        [DataMember(Name = "cHssn")]
        public bool? HandleSuccessSendNotice { get; set; }
        /// <summary>
        /// 处理失败情况是否发送消息到Mongodb
        /// </summary>
        [DataMember(Name = "HandleFailMessageToMongo")]
        [Obsolete]
        public bool? HandleFailMessageToMongo { get; set; }
        /// <summary>
        /// 其他特性配置
        /// </summary>
        [DataMember(Name = "consumeArgs")]
        public IDictionary<string, object> Args { get; set; }
        /// <summary>
        /// 处理失败是否重新放入队列
        /// </summary>
        /// <returns></returns>
        public bool HandleFailIsRQueue()
        {
            return this.HandleFailRQueue.HasValue && this.HandleFailRQueue.Value;
        }
    }
}
