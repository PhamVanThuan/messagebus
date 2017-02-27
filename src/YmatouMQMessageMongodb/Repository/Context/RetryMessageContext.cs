using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YmatouMQMessageMongodb.Repository.Mapping;
using YmtSystem.Repository.Mongodb.Context;

namespace YmatouMQMessageMongodb.Repository.Context
{
    public class RetryMessageContext: MongodbContext
    {
        public RetryMessageContext()
            : base(ConfigurationManager.AppSettings["RetryMessageMongoUrl"])
        {

        }
        protected override void OnEntityMap(EntityClassMap map, string contextName)
        {
            map.AddMap(new RetryMessageMapping().MapToDbCollection(), contextName);
        }
    }
}
