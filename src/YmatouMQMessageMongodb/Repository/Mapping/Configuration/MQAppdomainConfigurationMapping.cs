using System;
using MongoDB.Bson.Serialization;
using YmatouMQNet4.Configuration;
using YmtSystem.Repository.Mongodb.Mapping;

namespace YmatouMQ.MessageMongodb.Repository.Mapping
{
    public class MQAppdomainConfigurationMapping : ModelMappingBase<AppdomainConfiguration>
    {
        public MQAppdomainConfigurationMapping()
        {
            BsonClassMap.RegisterClassMap<AppdomainConfiguration>(map =>
            {
                map.MapIdProperty(m => m.DomainName);
                map.MapProperty(m => m.Version);
                map.MapProperty(m => m.Status);
                map.MapProperty(m => m.Host).SetIgnoreIfNull(true);
                map.MapProperty(m => m.Items);
            });
            BsonClassMap.RegisterClassMap<DomainItem>(map =>
            {
                map.MapProperty(m => m.AppId);
                map.MapProperty(m => m.Code);
                map.MapProperty(m => m._Status);
                map.MapProperty(m => m.ConnectionPoolSize);
            });
        }
        public override EntityMappingConfigure MapToDbCollection()
        {
            return new EntityMappingConfigure
            {
                MappType = typeof(AppdomainConfiguration),
                ToCollection = "MQ_Appdomain_Cfg",
                ToDatabase = "MQ_Configuration_201505"
            };
        }
    }
}
