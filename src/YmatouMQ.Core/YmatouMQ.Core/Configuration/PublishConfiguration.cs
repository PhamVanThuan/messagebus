using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace YmatouMQNet4.Configuration
{
    /// <summary>
    /// 发布消息属性配置
    /// </summary>
    [DataContract(Name = "pubCfg")]
    public class PublishConfiguration
    {
        /// <summary>
        /// 发布消息是否需要确认
        /// </summary>
        [DataMember(Name = "pubConfirms")]
        public bool? PublisherConfirms { get; set; }
        /// <summary>
        /// 路由关键字
        /// </summary>
        [DataMember(Name = "routeKey")]
        public string RouteKey { get; set; }
        /// <summary>
        /// 使用事务提交
        /// </summary>
        [DataMember(Name = "useTranCommit")]
        public bool? UseTransactionCommit { get; set; }     
        /// <summary>
        /// 重试次数
        /// </summary>
        [DataMember(Name = "retryCount")]
        public uint? RetryCount { get; set; }
        /// <summary>
        /// 重试间隔毫秒
        /// </summary>
        [DataMember(Name = "retryMs")]
        public uint? RetryMillisecond { get; set; }
        /// <summary>
        /// 内存队列大小
        /// </summary>
        [DataMember(Name = "mQueueLimit")]
        public uint? MemoryQueueLimit { get; set; }       
    }
}
