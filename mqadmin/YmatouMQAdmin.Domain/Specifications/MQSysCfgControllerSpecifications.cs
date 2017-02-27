using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using YmatouMQNet4.Configuration;
using MongoDB.Driver.Builders;
using MongoDB.Driver;

namespace YmatouMQAdmin.Domain.Specifications
{
    public class MQCfgControllerSpecifications
    {
        public static Expression<Func<MQSystemConfiguration, bool>> MatchSysCfgAppId(string appId)
        {
            if (string.IsNullOrEmpty(appId)) return MatchAppCfgAppId();
            var _appId = appId.ToLower();
            return e => e.AppId == _appId;
        }
        public static Expression<Func<MQSystemConfiguration, bool>> MatchAppCfgAppId()
        {
            return e => true;
        }
        public static Expression<Func<MQMainConfiguration, bool>> MmatchAppCfg(string appId)
        {
            if (string.IsNullOrEmpty(appId)) return e => true;
            if (appId == "default")
                return MmatchDefaultCfg("default");
            var _appId = appId.ToLower();
            return e => e.AppId == _appId;
        }
        public static Expression<Func<MQMainConfiguration, bool>> MmatchAppCfg(string appId, string code)
        {
            if (string.IsNullOrEmpty(appId) && string.IsNullOrEmpty(code)) return e => true;
            else if (!string.IsNullOrEmpty(appId) && !string.IsNullOrEmpty(code)) return e => e.AppId == appId && e.MessageCfgList.SingleOrDefault(c => c.Code == code) != null;
            else if (!string.IsNullOrEmpty(appId)) return e => e.AppId == appId.ToLower();
            else return e => true;
        }
        public static IMongoQuery MmatchAppCfg3(string appId, string code)
        {
            if (string.IsNullOrEmpty(appId) && string.IsNullOrEmpty(code)) return Query.Null;
            else if (!string.IsNullOrEmpty(appId) && !string.IsNullOrEmpty(code)) return Query.And(Query.EQ("_id", appId), Query.ElemMatch("MessageCfgList", Query.EQ("Code", code)));//e => e.AppId == appId && e.MessageCfgList.SingleOrDefault(c => c.Code == code) != null;
            else if (!string.IsNullOrEmpty(appId)) return Query.EQ("_id", appId);
            else return Query.Null;
        }
        public static Expression<Func<MQMainConfiguration, bool>> MmatchDefaultCfg(string appId = "default")
        {
            var _appId = appId ?? "default";
            _appId = _appId.ToLower();
            return e => e.AppId == _appId;
        }
        public static IMongoQuery MatchConnectionId(string connType) 
        {
            if (string.IsNullOrEmpty(connType)) return Query.Null;
            return Query.EQ("_id", connType);
        }
    }
}
