using System;

namespace YmatouMQ.Common.MessageHandleContract
{
    /// <summary>
    /// 消息推送状态
    /// </summary>
    public enum MessagePublishStatus
    {
        NoPush = 0,
        PushOk = 1,
        PushFail=2
    }
    /// <summary>
    /// 消息来源
    /// </summary>
    public enum MessageSource
    {
        /// <summary>
        /// 补偿消息来源-总线接收服务
        /// </summary>
        bus_publish=0,
        /// <summary>
        /// 补偿消息来源-总线推送服务
        /// </summary>
        bus_consumer=1,
        /// <summary>
        /// 补偿消息来源-总线消息调度服务
        /// </summary>
        bus_scheduler=2,
        /// <summary>
        /// 总线补单服务
        /// </summary>
        bus_retry=3,
        /// <summary>
        /// 来源RabbitMQ
        /// </summary>
        rabbitmq=4
    }

    public class _MessageSource
    {
        /// <summary>
        /// 补偿消息来源-总线接收服务
        /// </summary>
        public static readonly string MessageSource_Publish = "bus_publish";
   
        /// <summary>
        /// 补偿消息来源-总线消息调度服务
        /// </summary>
        public static readonly string MessageSource_MessageScheduler = "bus_scheduler";
        /// <summary>
        /// 总线补单服务
        /// </summary>
        public static readonly string MessageSource_Retry = "bus_retry";
        /// <summary>
        /// 消息来源 rabbitMQ
        /// </summary>
        public static readonly string MessageSource_RabbitMQ = "rabbitmq";
    }

    [Obsolete]
    public enum Status
    {
        Normal = 0,
        Exception = 1,
        MemoryQueueGtLimit = 2,
        HandleException = 3,
        HandleSuccess = 4
    }
}
