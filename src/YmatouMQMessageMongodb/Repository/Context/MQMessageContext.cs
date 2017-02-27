using System;
using System.Configuration;
using YmatouMQMessageMongodb.Repository.Mapping;
using YmtSystem.Repository.Mongodb.Context;

namespace YmatouMQMessageMongodb.Repository.Context
{
    public class MQMessageContext : MongodbContext
    {
        public MQMessageContext()
            : base(ConfigurationManager.AppSettings["mongotest"])
        {

        }
        protected override void OnEntityMap(EntityClassMap map, string contextName)
        {
            map.AddMap(new MQMessageMapping().MapToDbCollection(), contextName);
        }
    }
}
