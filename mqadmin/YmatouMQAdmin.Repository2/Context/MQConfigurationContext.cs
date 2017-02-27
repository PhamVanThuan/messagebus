using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YmatouMQAdmin.Repository.Mapping;
using YmtSystem.Repository.Mongodb.Context;

namespace YmatouMQAdmin.Repository.Context
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
