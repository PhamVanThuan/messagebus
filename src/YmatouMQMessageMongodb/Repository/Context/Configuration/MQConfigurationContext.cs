using System;
using System.Configuration;
using YmatouMQ.MessageMongodb.Repository.Mapping;
using YmtSystem.Repository.Mongodb.Context;

namespace YmatouMQMessageMongodb.Repository.Context
{
    public class MQConfigurationContext : MongodbContext
    {
        public MQConfigurationContext()
            : base(ConfigurationManager.AppSettings["MQConfigurationMongoUrl"])
        {

        }
        protected override void OnEntityMap(EntityClassMap map, string contextName)
        {
            map.AddMap(new MQConfigurationMapping().MapToDbCollection(), contextName);
            // map.AddMap(new MQAppdomainConfigurationMapping().MapToDbCollection(), contextName);
        }
    }

    public class MQAppdomainConfigurationContext : MongodbContext
    {
        public MQAppdomainConfigurationContext()
            : base(ConfigurationManager.AppSettings["MQConfigurationMongoUrl"])
        {

        }
        protected override void OnEntityMap(EntityClassMap map, string contextName)
        {
            // map.AddMap(new MQConfigurationMapping().MapToDbCollection(), contextName);
            map.AddMap(new MQAppdomainConfigurationMapping().MapToDbCollection(), contextName);
        }
    }
}
