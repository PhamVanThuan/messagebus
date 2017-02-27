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
    public class RetryMessageMapping : ModelMappingBase<RetryMessage>
    {
        public RetryMessageMapping()
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof (RetryMessage)))
            {
                BsonClassMap.RegisterClassMap<RetryMessage>(map =>
                {
                    map.MapIdProperty(m => m._id);
                    map.MapProperty(m => m.Status).SetElementName("status").SetIgnoreIfNull(true);
                    map.MapProperty(m => m.AppId).SetElementName("appid").SetIgnoreIfNull(true);
                    map.MapProperty(m => m.Code).SetElementName("code").SetIgnoreIfNull(true);
                    map.MapProperty(m => m.MessageId).SetElementName("mid").SetIgnoreIfNull(true);
                    map.MapProperty(m => m.Body).SetElementName("body").SetIgnoreIfNull(true);
                    map.MapProperty(m => m.CreateTime).SetElementName("ctime").SetIgnoreIfNull(true);
                    map.MapProperty(m => m.RetryTime).SetElementName("rtime").SetIgnoreIfNull(true);
                    map.MapProperty(m => m.RetryExpiredTime).SetElementName("rtimeout").SetIgnoreIfNull(true);
                    map.MapProperty(m => m.AppKey).SetElementName("appkey").SetIgnoreIfNull(true);
                    map.MapProperty(m => m.CallbackKey).SetElementName("callback").SetIgnoreIfNull(true);
                    map.MapProperty(m => m.RetryCount).SetElementName("retrycount");
                    map.MapProperty(m => m.IsReSetRetryStatus)
                        .SetElementName("isReSetRetryStatus")
                        .SetIgnoreIfNull(true);
                    map.MapProperty(m => m.Desc).SetIgnoreIfNull(true);
                    map.MapProperty(m => m.MessageSource).SetElementName("source").SetIgnoreIfNull(true);
                });
            }
            if (!BsonClassMap.IsClassMapRegistered(typeof (CallbackInfo)))
            {
                BsonClassMap.RegisterClassMap<CallbackInfo>(map =>
                {
                    map.MapProperty(m => m.CallbackKey).SetElementName("_cid").SetIgnoreIfNull(true);
                    map.MapProperty(m => m.Status).SetElementName("_status");
                    map.MapProperty(m => m.RetryCount).SetElementName("_count");
                });
            }
        }

        public override EntityMappingConfigure MapToDbCollection()
        {
            return new EntityMappingConfigure
            {
                MappType = typeof(RetryMessage),
                ToDatabase = "MQ_Message_Compensate",
                ToCollection = "message"
            };
        }
    }
}
