using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization;
using YmatouMQAdmin.Domain.Module;
using YmtSystem.Repository.Mongodb.Mapping;

namespace YmatouMQAdmin.Repository.Mapping
{
    public class MQMessageMapping : ModelMappingBase<MQMessage>
    {
        public MQMessageMapping()
        {
            BsonClassMap.RegisterClassMap<MQMessage>(map =>
            {
                map.MapIdProperty(m => m._id);
                map.MapProperty(m => m.AppId).SetElementName("aid").SetIgnoreIfNull(true);
                map.MapProperty(m => m.Code).SetElementName("code").SetIgnoreIfNull(true);
                map.MapProperty(m => m.MsgId).SetElementName("mid").SetIgnoreIfNull(true);
                map.MapProperty(m => m.Ip).SetElementName("ip").SetIgnoreIfNull(true);
                map.MapProperty(m => m.Body).SetElementName("body").SetIgnoreIfNull(true);             
                map.MapProperty(m => m.CreateTime).SetElementName("ctime");

            });
            BsonClassMap.RegisterClassMap<MQMessageStatus>(map =>
            {
                map.MapIdProperty(m => m._sid);
                map.MapProperty(m => m.MessageId).SetElementName("_mid").SetIgnoreIfNull(true);
                map.MapProperty(m => m.Status).SetElementName("status").SetIgnoreIfNull(true);
                map.MapProperty(m => m.CreateTime).SetElementName("ctime");
            });
        }

        public override EntityMappingConfigure MapToDbCollection()
        {
            return new EntityMappingConfigure
            {
                MappType = typeof(MQMessage),
                ToCollection = "Message",
                ToDatabase = "MQ_Message"
            };
        }
    }
}
