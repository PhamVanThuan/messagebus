using System;
using System.Collections.Generic;
using System.Runtime;
using System.Runtime.Serialization;

namespace YmatouMQNet4.Configuration
{
    /// <summary>
    /// MQ 配置（入口）
    /// </summary>
    [DataContract(Name = "mqCfgMain")]
    public class MQMainConfiguration
    {
        /// <summary>
        /// 应用ID（唯一）
        /// </summary>
        [DataMember(Name = "appId")]
        public string AppId { get; set; }
        /// <summary>
        /// 配置版本
        /// </summary>
        [DataMember(Name = "version")]
        public int Version { get; set; }
        /// <summary>
        /// 链接属性配置
        /// </summary>
        [DataMember(Name = "connCfg")]
        public ConnectionConfigureation ConnCfg { get; set; }
        /// <summary>
        /// 消息配置
        /// </summary>
        [DataMember(Name = "msgCfgs")]
        public IEnumerable<MessageConfiguration> MessageCfgList { get; set; }

        #region [ 默认配置 ]
        /// <summary>
        /// 默认策略配置
        /// </summary>
        public static MQMainConfiguration DefaultMQCfg { get { return _defaultMQCfg; } }

        private static readonly MQMainConfiguration _defaultMQCfg = new MQMainConfiguration
        {
            AppId = "default",
            Version = 3,
            ConnCfg = new ConnectionConfigureation
            {
                ConnectionString = "host=172.16.100.104;port=5672;vHost=/;uNmae=guest;pas=guest;heartbeat=5000;recoveryInterval=5;channelMax=100;useBackgroundThreads=true;pooMinSize=1;pooMaxSize=10",
            },
            MessageCfgList = new List<MessageConfiguration> 
            {
                new MessageConfiguration
                {
                    Code="default",
                    Enable =true,                   
                    ConsumeCfg=new ConsumeConfiguration
                    {
                        UseMultipleThread=false,
                        MaxThreadCount=Convert.ToUInt32(Environment.ProcessorCount-1),
                        PrefetchCount=null,
                        RoutingKey="#.#",
                        Args=null,
                        IsAutoAcknowledge=false,
                        HandleFailAcknowledge=true,
                        HandleFailRQueue=false,
                        CallbackMethodType="POST",
                        CallbackTimeOut=3000,
                        CallbackTimeOutAck=false,
                        CallbackUrl=null,
                        RetryCount=0,
                        RetryMillisecond=2000,
                        HandleFailPersistentStore=false,
                        HandleSuccessSendNotice=true ,
                        //ConsumeCount=1,
                        HandleFailMessageToMongo=false,                        
                    },
                    ExchangeCfg=new ExchangeConfiguration
                    {
                        _ExchangeType= ExchangeType.topic,
                        Durable=true, 
                        IsExchangeAutoDelete=false,
                        ExchangeName="ymatou",
                        Arguments=null,                        
                    },
                    MessagePropertiesCfg=new MessagePropertiesConfiguration
                    {
                        PersistentMessages=false,
                        ContextType="application/json",
                        PersistentMessagesLocal=false,
                        PersistentMessagesMongo=true,
                        ContentEncoding="utf-8", 
                        Expiration=null,    
                        Priority=null                  
                    },
                    PublishCfg=new PublishConfiguration
                    {
                        MemoryQueueLimit=100000,
                        PublisherConfirms=false,
                        RetryCount=3,
                        RetryMillisecond=500,
                        RouteKey="#.#",
                        UseTransactionCommit=false 
                    },
                    QueueCfg=new QueueConfiguration
                    {
                        IsAutoDelete=false ,   
                        IsDurable=false ,
                        IsQueueExclusive=false,
                        QueueName="ymatoumq",
                        Args=null ,
                        HeadArgs=null
                    }
                }
            }
        };
        #endregion
    }
}
