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
    public class AlarmContext : MongodbContext
    {
        public AlarmContext()
            : base(ConfigurationManager.AppSettings["AlarmMongoUrl"])
        {

        }
        protected override void OnEntityMap(EntityClassMap map, string contextName)
        {
            map.AddMap(new AlarmMapping().MapToDbCollection(), contextName);
        }
    }
}
