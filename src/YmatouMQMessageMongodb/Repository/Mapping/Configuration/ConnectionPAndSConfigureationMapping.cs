using System;
using MongoDB.Bson.Serialization;
using YmatouMQNet4.Configuration;
using YmtSystem.Repository.Mongodb.Mapping;

namespace YmatouMQ.MessageMongodb.Repository.Mapping
{
    public class ConnectionPAndSConfigureationMapping : ModelMappingBase<ConnectionPAndSConfigureation>
    {
        public ConnectionPAndSConfigureationMapping()
        {
            BsonClassMap.RegisterClassMap<ConnectionPAndSConfigureation>(map =>
            {
                map.MapIdProperty(m => m.ConnId);
                map.MapMember(m => m.ConnType).SetIgnoreIfNull(true);
                map.MapMember(m => m.ConnectionString).SetIgnoreIfNull(true);
            });
        }
        public override EntityMappingConfigure MapToDbCollection()
        {
            return new EntityMappingConfigure
            {
                MappType = typeof(ConnectionPAndSConfigureation),
                ToCollection = "MQ_Connection_Cfg",
                ToDatabase = "MQ_Configuration_201505",
            };
        }
    }
}
