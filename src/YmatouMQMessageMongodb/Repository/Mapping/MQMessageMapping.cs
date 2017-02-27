using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using YmtSystem.Repository.Mongodb.Mapping;
using YmatouMQMessageMongodb.Domain.Module;

namespace YmatouMQMessageMongodb.Repository.Mapping
{
    public class MQMessageMapping : ModelMappingBase<MQMessage>
    {
        public MQMessageMapping()
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof (MQMessage)))
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
                    map.MapProperty(m => m.BusReceivedServerIp).SetElementName("busIp").SetIgnoreIfNull(true);
                    map.MapProperty(m => m.UuId).SetElementName("uuid").SetIgnoreIfNull(true);
                    map.MapProperty(m => m.PushStatus).SetElementName("pushstatus").SetIgnoreIfNull(true);
                    map.MapProperty(m => m.PushTime).SetElementName("pushtime").SetIgnoreIfNull(true);
                });
            }
            if (!BsonClassMap.IsClassMapRegistered(typeof (MQMessageStatus)))
            {
                BsonClassMap.RegisterClassMap<MQMessageStatus>(map =>
                {
                    map.MapIdProperty(m => m._sid);
                    map.MapProperty(m => m.MessageId).SetElementName("_mid").SetIgnoreIfNull(true);
                    map.MapProperty(m => m.Status).SetElementName("status").SetIgnoreIfNull(true);
                    map.MapProperty(m => m.HandleSource).SetElementName("source").SetIgnoreIfNull(true);
                    map.MapProperty(m => m.CreateTime).SetElementName("ctime");
                    map.MapProperty(m => m.CallbackId).SetElementName("_cid").SetIgnoreIfNull(true);
                    map.MapProperty(m => m.ReceivedMessageIp).SetElementName("r_ip").SetIgnoreIfNull(true);
                    map.MapProperty(m => m.MessageUuid).SetElementName("uuid").SetIgnoreIfNull(true);
                });
            }
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
