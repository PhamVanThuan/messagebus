using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization;
using YmatouMQNet4.Configuration;
using YmtSystem.Repository.Mongodb.Mapping;

namespace YmatouMQAdmin.Repository.Mapping
{
    public class MQSystemConfigurationMapping : ModelMappingBase<MQSystemConfiguration>
    {
        public MQSystemConfigurationMapping()
        {
            BsonClassMap.RegisterClassMap<MQSystemConfiguration>(map =>
            {
                map.MapIdProperty(o => o.AppId);
                map.MapProperty(o => o.LogFilePath).SetElementName("lPath").SetIgnoreIfNull(true);
                map.MapProperty(o => o.LogSize).SetElementName("lSize").SetIgnoreIfNull(true);
                map.MapProperty(o => o.FulshLogTimestamp).SetElementName("flT").SetIgnoreIfNull(true);
                map.MapProperty(o => o.ConnShutdownMessageLocalEnqueue).SetElementName("mlQ").SetIgnoreIfNull(true);
                map.MapProperty(o => o.EnableTrackPubRunTime).SetElementName("prT").SetIgnoreIfNull(true);
                map.MapProperty(o => o.EnableTrackSubRunTime).SetElementName("srT").SetIgnoreIfNull(true);
                map.MapProperty(o => o.FulshMQConfigurationTimestamp).SetElementName("fcT").SetIgnoreIfNull(true);
                map.MapProperty(o => o.MaxThreadPublishAsync).SetElementName("maxth").SetIgnoreIfNull(true);
                map.MapProperty(o => o.PubMessageMemeoryQueueLimit).SetElementName("limit").SetIgnoreIfNull(true);
                map.MapProperty(o => o.InfoLogEnable).SetElementName("ilog");
                map.MapProperty(o => o.DebugLogEnable).SetElementName("dlog");
                map.MapProperty(o => o.ErrorLogEnable).SetElementName("elog");
            });
        }

        public override EntityMappingConfigure MapToDbCollection()
        {
            return new EntityMappingConfigure
            {
                MappType = typeof(MQSystemConfiguration),
                ToCollection = "MQ_Sys_Cfg",
                ToDatabase = "MQ_Configuration_201505"
            };
        }
    }
}
