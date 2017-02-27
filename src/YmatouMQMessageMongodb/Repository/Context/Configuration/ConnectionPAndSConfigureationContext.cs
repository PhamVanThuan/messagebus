using System;
using System.Configuration;
using YmatouMQ.MessageMongodb.Repository.Mapping;
using YmtSystem.Repository.Mongodb.Context;

namespace YmatouMQMessageMongodb.Repository.Context
{
    public class ConnectionPAndSConfigureationContext : MongodbContext
    {
        public ConnectionPAndSConfigureationContext()
            : base(ConfigurationManager.AppSettings["MQConfigurationMongoUrl"])
        {
 
        }
        protected override void OnEntityMap(EntityClassMap map, string contextName)
        {
            map.AddMap(new ConnectionPAndSConfigureationMapping().MapToDbCollection(), contextName);
        }
    }
}
