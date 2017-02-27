using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YmatouMQMessageMongodb.Domain.Module;
using YmtSystem.Repository.Mongodb.Mapping;

namespace YmatouMQMessageMongodb.Repository.Mapping
{
    public class AlarmMapping : ModelMappingBase<Alarm>
    {
        public AlarmMapping()
        {
            BsonClassMap.RegisterClassMap<Alarm>(map =>
            {
                map.MapIdProperty(m => m.CallbackId);
                map.MapProperty(m => m.CallbackUrl).SetIgnoreIfNull(true);
                map.MapProperty(m => m.AlarmAppId).SetIgnoreIfNull(true);
                map.MapProperty(m => m.Description).SetIgnoreIfNull(true);
            });
        }
        public override EntityMappingConfigure MapToDbCollection()
        {
            return new EntityMappingConfigure
            {
                MappType = typeof(Alarm),
                ToCollection = "Alarm",
                ToDatabase = "MQ_Alarm"
            };
        }
    }
}
