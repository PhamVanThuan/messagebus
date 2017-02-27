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
