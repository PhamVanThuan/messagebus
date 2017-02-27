using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Bson;
using YmatouMQAdmin.Domain.Module;

namespace YmatouMQAdmin.Domain.Specifications
{
    public class MQMessageSpecifications
    {
        public static IMongoQuery MatchMessage(string appId, string messageId)
        {
            if (string.IsNullOrEmpty(messageId)) return Query.Null;
            var arr = messageId.Split(new char[] { ',' });
            return Query.And(Query<MQMessage>.In(e => e.MsgId, arr));
        }

        public static IMongoQuery MatchMessageStatus(string messageId, string status)
        {
            return Query<MQMessageStatus>.EQ(e => e.MessageId, messageId);
        }

        public static IMongoQuery MatchMessageDate(DateTime beginTime, DateTime endTime)
        {
            return Query.And(Query<MQMessage>.GTE(e => e.CreateTime, beginTime), Query<MQMessage>.LT(e => e.CreateTime, endTime));
        }

        public static IMongoQuery MatchMessageStatusDate(DateTime beginTime, DateTime endTime)
        {
            return Query.And(Query<MQMessageStatus>.GTE(e => e.CreateTime, beginTime), Query<MQMessageStatus>.LT(e => e.CreateTime, endTime));
        }


        public static IMongoQuery MatchMessageStatus(IEnumerable<string> messageId)
        {
            if (!messageId.Any())
            {
                return Query.Null;
            }

            return Query.In("_mid", new BsonArray(messageId));
        }

        public static string MessageDb(string appid)
        {
            return string.Format("MQ_Message_{0}_{1}", appid, DateTime.Now.ToString("yyyyMM"));
        }
        public static string MessageCollectionName(string code)
        {
            return string.Format("Message_{0}", code);
        }

        public static string MessageStatusDbName()
        {
            return string.Format("MQ_Message_Status_{0}", DateTime.Now.ToString("yyyyMM"));
        }
    }
}
