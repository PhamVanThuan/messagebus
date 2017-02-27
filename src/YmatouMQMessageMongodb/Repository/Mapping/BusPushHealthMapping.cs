using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization;
using YmatouMQMessageMongodb.Domain.Module;
using YmtSystem.Repository.Mongodb.Mapping;

namespace YmatouMQMessageMongodb.Repository.Mapping
{
   public  class BusPushHealthMapping : ModelMappingBase<BusPushHealth>
    {
       public BusPushHealthMapping()
       {
           if (!BsonClassMap.IsClassMapRegistered(typeof(BusPushHealth)))
           {
               BsonClassMap.RegisterClassMap<BusPushHealth>(map =>
               {
                   map.MapIdProperty(m => m.HealthId);
                   map.MapProperty(m => m.LastUpdateTime);
                   map.MapProperty(m => m.Status).SetIgnoreIfNull(true);
                   map.MapProperty(m => m.Ip).SetIgnoreIfNull(true);
               });
           }
       }

       public override EntityMappingConfigure MapToDbCollection()
        {
            return new EntityMappingConfigure
            {
                MappType = typeof(BusPushHealth),
                ToCollection = "BusPushHealth",
                ToDatabase = "MQ_Alarm"
            };
        }
    }
}
