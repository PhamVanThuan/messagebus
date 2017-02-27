using System;
using MongoDB.Bson.Serialization;
using YmatouMQNet4.Configuration;
using YmtSystem.Repository.Mongodb.Mapping;

namespace YmatouMQ.MessageMongodb.Repository.Mapping
{
    public class MQConfigurationMapping : ModelMappingBase<MQMainConfiguration>
    {
        public MQConfigurationMapping()
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof (MQMainConfiguration)))
            {           
                BsonClassMap.RegisterClassMap<MQMainConfiguration>(map =>
                {
                    map.MapIdProperty(m => m.AppId);
                    map.MapProperty(m => m.Version)/*.SetElementName("V")*/;
                    map.MapProperty(c => c.OwnerHost).SetIgnoreIfNull(true);
                    map.MapProperty(m => m.ConnCfg)/*.SetElementName("conn")*/.SetIgnoreIfNull(true);
                    map.MapProperty(m => m.MessageCfgList)/*.SetElementName("cfgItems")*/.SetIgnoreIfNull(true);
                });
            }
            if (!BsonClassMap.IsClassMapRegistered(typeof(ConnectionConfigureation)))
            {
                BsonClassMap.RegisterClassMap<ConnectionConfigureation>(connMap =>
                {
                    connMap.MapProperty(c => c.ConnectionString) /*.SetElementName("str")*/.SetIgnoreIfNull(true);
                    connMap.MapProperty(c => c.HealthCheck);
                    connMap.MapProperty(c => c.HealthSecond);
                });
            }
            if (!BsonClassMap.IsClassMapRegistered(typeof(MessageConfiguration)))
            {
                BsonClassMap.RegisterClassMap<MessageConfiguration>(msgmap =>
                {
                    msgmap.MapProperty(m => m.Code) /*.SetElementName("code")*/.SetIgnoreIfNull(true);
                    msgmap.MapProperty(m => m.Enable) /*.SetElementName("open")*/.SetIgnoreIfNull(true);
                    msgmap.MapProperty(m => m.ConsumeCfg) /*.SetElementName("C")*/.SetIgnoreIfNull(true);
                    msgmap.MapProperty(m => m.ExchangeCfg) /*.SetElementName("E")*/.SetIgnoreIfNull(true);
                    msgmap.MapProperty(m => m.MessagePropertiesCfg) /*.SetElementName("M")*/.SetIgnoreIfNull(true);
                    msgmap.MapProperty(m => m.QueueCfg) /*.SetElementName("Q")*/.SetIgnoreIfNull(true);
                    msgmap.MapProperty(m => m.PublishCfg) /*.SetElementName("P")*/.SetIgnoreIfNull(true);
                    msgmap.MapProperty(m => m.CallbackCfgList).SetIgnoreIfNull(true);
                });
            }
            if (!BsonClassMap.IsClassMapRegistered(typeof(MessagePropertiesConfiguration)))
            {
                BsonClassMap.RegisterClassMap<MessagePropertiesConfiguration>(mpmap =>
                {
                    mpmap.MapProperty(m => m.ContentEncoding) /*.SetElementName("mCE")*/.SetIgnoreIfNull(true);
                    mpmap.MapProperty(m => m.ContextType) /*.SetElementName("mCType")*/.SetIgnoreIfNull(true);
                    mpmap.MapProperty(m => m.Expiration) /*.SetElementName("mET")*/.SetIgnoreIfNull(true);
                    mpmap.MapProperty(m => m.PersistentMessages) /*.SetElementName("mPs")*/.SetIgnoreIfNull(true);
                    mpmap.MapProperty(m => m.PersistentMessagesLocal) /*.SetElementName("mPl")*/.SetIgnoreIfNull(true);
                    mpmap.MapProperty(m => m.PersistentMessagesMongo) /*.SetElementName("mPmongo")*/
                        .SetIgnoreIfNull(true);
                    mpmap.MapProperty(m => m.Priority) /*.SetElementName("mP")*/.SetIgnoreIfNull(true);
                });
            }
            if (!BsonClassMap.IsClassMapRegistered(typeof(PublishConfiguration)))
            {
                BsonClassMap.RegisterClassMap<PublishConfiguration>(pubMap =>
                {
                    pubMap.MapProperty(m => m.MemoryQueueLimit) /*.SetElementName("pqLimit")*/.SetIgnoreIfNull(true);
                    pubMap.MapProperty(m => m.PublisherConfirms) /*.SetElementName("pConfirms")*/.SetIgnoreIfNull(true);
                    pubMap.MapProperty(m => m.RetryCount) /*.SetElementName("pRC")*/.SetIgnoreIfNull(true);
                    pubMap.MapProperty(m => m.RetryMillisecond) /*.SetElementName("pRM")*/.SetIgnoreIfNull(true);
                    pubMap.MapProperty(m => m.RouteKey) /*.SetElementName("pRK")*/.SetIgnoreIfNull(true);
                    pubMap.MapProperty(m => m.UseTransactionCommit) /*.SetElementName("pTC")*/.SetIgnoreIfNull(true);
                });
            }
            if (!BsonClassMap.IsClassMapRegistered(typeof(ConsumeConfiguration)))
            {
                BsonClassMap.RegisterClassMap<ConsumeConfiguration>(consumMap =>
                {
                    //consumMap.MapProperty(m => m.UseMultipleThread)/*.SetElementName("cuMth")*/.SetIgnoreIfNull(true);
                    consumMap.MapProperty(m => m.Args) /*.SetElementName("cArgs")*/.SetIgnoreIfNull(true);
                    consumMap.MapProperty(m => m.HandleFailAcknowledge) /*.SetElementName("cFailAck")*/
                        .SetIgnoreIfNull(true);
                    consumMap.MapProperty(m => m.IsAutoAcknowledge) /*.SetElementName("cAutoAck")*/
                        .SetIgnoreIfNull(true);
                    //consumMap.MapProperty(m => m.HandleFailRQueue)/*.SetElementName("crQueue")*/.SetIgnoreIfNull(true);
                    consumMap.MapProperty(m => m.MaxThreadCount) /*.SetElementName("cMaxTh")*/.SetIgnoreIfNull(true);
                    //consumMap.MapProperty(m => m.UseMultipleThread)/*.SetElementName("cUMt")*/.SetIgnoreIfNull(true);
                    //consumMap.MapProperty(m => m.CallbackUrl)/*.SetElementName("cUrl")*/.SetIgnoreIfNull(true);
                    //consumMap.MapProperty(m => m.CallbackTimeOutAck)/*.SetElementName("ctAck")*/.SetIgnoreIfNull(true);
                    //consumMap.MapProperty(m => m.CallbackMethodType)/*.SetElementName("cmType")*/.SetIgnoreIfNull(true);
                    //consumMap.MapProperty(m => m.CallbackTimeOut)/*.SetElementName("cTimeOut")*/.SetIgnoreIfNull(true);
                    consumMap.MapProperty(m => m.RoutingKey) /*.SetElementName("cRKey")*/.SetIgnoreIfNull(true);
                    //consumMap.MapProperty(m => m.RetryCount)/*.SetElementName("cRCount")*/.SetIgnoreIfNull(true);
                    consumMap.MapProperty(m => m.RetryMillisecond) /*.SetElementName("cRMs")*/.SetIgnoreIfNull(true);
                    consumMap.MapProperty(m => m.HandleSuccessSendNotice) /*.SetElementName("cHssn")*/
                        .SetIgnoreIfNull(true);
                    //consumMap.MapProperty(m => m.HandleFailPersistentStore)/*.SetElementName("cHfps")*/.SetIgnoreIfNull(true);
                    //consumMap.MapProperty(m => m.ConsumeCount)/*.SetElementName("cC")*/.SetIgnoreIfNull(true);
                    consumMap.MapProperty(m => m.PrefetchCount) /*.SetElementName("cPrefetchCount")*/
                        .SetIgnoreIfNull(true);
                    //consumMap.MapProperty(m => m.HandleFailMessageToMongo)/*.SetElementName("HandleFailMessageToMongo")*/.SetIgnoreIfNull(true);
                    consumMap.MapProperty(m => m.RetryTimeOut).SetIgnoreIfNull(true);
                });
            }
            if (!BsonClassMap.IsClassMapRegistered(typeof(ExchangeConfiguration)))
            {
                BsonClassMap.RegisterClassMap<ExchangeConfiguration>(exMap =>
                {
                    exMap.MapProperty(m => m._ExchangeType) /*.SetElementName("exType")*/.SetIgnoreIfNull(true);
                    exMap.MapProperty(m => m.Arguments) /*.SetElementName("exArgs")*/.SetIgnoreIfNull(true);
                    exMap.MapProperty(m => m.Durable) /*.SetElementName("edurable")*/.SetIgnoreIfNull(true);
                    exMap.MapProperty(m => m.ExchangeName) /*.SetElementName("eName")*/.SetIgnoreIfNull(true);
                    exMap.MapProperty(m => m.IsExchangeAutoDelete) /*.SetElementName("eDel")*/.SetIgnoreIfNull(true);
                });
            }
            if (!BsonClassMap.IsClassMapRegistered(typeof(QueueConfiguration)))
            {
                BsonClassMap.RegisterClassMap<QueueConfiguration>(qMap =>
                {
                    qMap.MapProperty(m => m.Args) /*.SetElementName("qArgs")*/.SetIgnoreIfNull(true);
                    qMap.MapProperty(m => m.HeadArgs) /*.SetElementName("hArgs")*/.SetIgnoreIfNull(true);
                    qMap.MapProperty(m => m.IsAutoDelete) /*.SetElementName("qDel")*/.SetIgnoreIfNull(true);
                    qMap.MapProperty(m => m.IsDurable) /*.SetElementName("qdurable")*/.SetIgnoreIfNull(true);
                    qMap.MapProperty(m => m.IsQueueExclusive) /*.SetElementName("qEx")*/.SetIgnoreIfNull(true);
                    qMap.MapProperty(m => m.QueueName) /*.SetElementName("qName")*/.SetIgnoreIfNull(true);
                });
            }
            if (!BsonClassMap.IsClassMapRegistered(typeof(CallbackConfiguration)))
            {
                BsonClassMap.RegisterClassMap<CallbackConfiguration>(cMap =>
                {
                    cMap.MapProperty(m => m.CallbackKey).SetIgnoreIfNull(true);
                    cMap.MapProperty(m => m.Enable).SetIgnoreIfNull(true);
                    cMap.MapProperty(m => m.ContentType).SetIgnoreIfNull(true);
                    cMap.MapProperty(m => m.CallbackTimeOut).SetIgnoreIfNull(true);
                    cMap.MapProperty(m => m.Url).SetIgnoreIfNull(true);
                    cMap.MapProperty(m => m.HttpMethod).SetIgnoreIfNull(true);
                    cMap.MapProperty(m => m.Priority).SetIgnoreIfNull(true);
                    cMap.MapProperty(m => m.AcceptMessageTimeRange).SetIgnoreIfNull(true);
                    cMap.MapProperty(m => m.IsRetry).SetIgnoreIfNull(true);
                });
            }
        }

        public override EntityMappingConfigure MapToDbCollection()
        {
            return new EntityMappingConfigure
            {
                MappType = typeof(MQMainConfiguration),
                ToCollection = "MQ_Default_Cfg",
                ToDatabase = "MQ_Configuration_201505",
            };
        }
    }
}
