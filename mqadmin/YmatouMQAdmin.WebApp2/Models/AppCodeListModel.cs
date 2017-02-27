using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace YmatouMQAdmin.WebApp2.Models
{
    public class AppCodeListModel
    {
        public AppCodeListModel() 
        {

            this.CallbackList = new List<CallBackConfig> { }; 
        }
        public string AppId { get; set; }

        public string Code { get; set; }

        public string host { get; set; }

        public string IsEnable { get; set; }

        /// <summary>
        /// 线程池 线程大小
        /// </summary>
        public uint ConsumeCfg_MaxThreadCount { get; set; }

        /// <summary>
        /// 预处理大小
        /// </summary>
        public ushort ConsumeCfg_PrefetchCount { get; set; }

        /// <summary>
        /// 消费者 补偿消息超时时间
        /// </summary>
        public int ConsumeCfg_RetryTimeOut { get; set; }

        /// <summary>
        /// 交换机类型
        /// </summary>
        public string ExchangeCfg_ExchangeType { get; set; }

        /// <summary>
        /// 交换机是否持久化
        /// </summary>
        public string ExchangeCfg_IsDurable { get; set; }

        /// <summary>
        /// 队列 是否持久化
        /// </summary>
        public string QueueCfg_IsDurable { get; set; }

        public IEnumerable<CallBackConfig> CallbackList { get; set; }
    }

    public class CallBackConfig
    {
        /// <summary>
        /// 业务端URL
        /// </summary>
        public string Url { get; set; }

        public string Enable { get; set; }

        /// <summary>
        /// 回调业务端超时时间
        /// </summary>
        public int CallbackTimeOut { get; set; }

        /// <summary>
        /// 回调失败是否补偿消息
        /// </summary>
        public string IsRetry { get; set; }

    }
}