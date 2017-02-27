using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Bson;
using YmatouMQ.Common.Extensions;
using YmatouMQMessageMongodb.Domain.Module;

namespace YmatouMQMessageMongodb.Domain.Specifications
{
    /// <summary>
    /// 消息总线消息规则
    /// </summary>
    public class MQMessageSpecifications
    {
        public static IMongoQuery ExcludeMessage(DateTime starTime,DateTime endTime,IEnumerable<string>  messageId)
        {
            if (messageId.IsEmptyEnumerable())
                return Query.And(Query.GT("ctime", starTime), Query.LTE("ctime", endTime));
            return Query.And(Query.GT("ctime", starTime), Query.LTE("ctime", endTime),
                Query.NotIn("mid", new BsonArray(messageId)));
        }

        public static IMongoQuery MatchMessage(string appId, string messageId)
        {
            if (string.IsNullOrEmpty(messageId)) return Query.Null;
            var arr = messageId.Split(new char[] { ',' });
            return Query.And(Query<MQMessage>.In(e => e.MsgId, arr));
        }

        public static IMongoQuery MatchMessageDate(DateTime beginTime, DateTime endTime, string clientIp = null)
        {
            if (string.IsNullOrEmpty(clientIp))
                return Query.And(Query<MQMessage>.GTE(e => e.CreateTime, beginTime), Query<MQMessage>.LT(e => e.CreateTime, endTime));
            return Query.And(Query<MQMessage>.GTE(e => e.CreateTime, beginTime), Query<MQMessage>.LT(e => e.CreateTime, endTime), Query<MQMessage>.EQ(e => e.Ip, clientIp));
        }
        public static IMongoQuery MathchMessageId(string messageid, string clientIp = null)
        {
            if (string.IsNullOrEmpty(clientIp))
                return Query<MQMessage>.EQ(m => m.MsgId, messageid);
            return Query.And(Query<MQMessage>.EQ(m => m.MsgId, messageid), Query<MQMessage>.EQ(e => e.Ip, clientIp));
        }
        public static IMongoQuery MatchMessageStatusDate(DateTime beginTime, DateTime endTime)
        {
            return Query.And(Query<MQMessageStatus>.GTE(e => e.CreateTime, beginTime), Query<MQMessageStatus>.LT(e => e.CreateTime, endTime));
        }
        public static IMongoQuery MatchMessageStatusID(string messageid)
        {
            return Query<MQMessageStatus>.EQ(m => m.MessageId, messageid);
        }

       
        public static IMongoQuery MatchMessageStatus(IEnumerable<string> messageId)
        {
            if (!messageId.Any())
            {
                return Query.Null;
            }

            return Query.In("_mid", new BsonArray(messageId));
        }
        public static string MessageStatusDbName()
        {
            return string.Format("MQ_Message_Status_{0}", DateTime.Now.ToString("yyyyMM"));
        }

        public static IMongoQuery MatchInMessageStatusId(IEnumerable<string> messageId)
        {
            return Query.In("_mid", new BsonArray(messageId));
        }

        public static IMongoQuery MatchMessageUniqueId(string uniqueId)
        {
            return Query.EQ("_id", uniqueId);
        }

        public static FilterDefinition<MQMessage> _MatchMessageUniqueId(string id)
        {
            return Builders<MQMessage>.Filter.Eq("_id", id);
        }

        public static IMongoUpdate UpdateMessageStatus(int status)
        {
            return Update.Combine(Update.Set("pushstatus", status),
                Update.CurrentDate("pushtime"));
        }
  
        public static UpdateDefinition<MQMessage> _UpdateMessageStatus(int status)
        {
            return Builders<MQMessage>.Update.Combine(Builders<MQMessage>.Update.Set("pushstatus", status),
                Builders<MQMessage>.Update.CurrentDate("pushtime"));           
        }
        public static IMongoQuery MatchMessagePushStatus(int status,DateTime beginTime,DateTime endTime)
        {
            return Query.And(Query.EQ("pushstatus", status),
                Query.And(Query.GT("ctime", beginTime), Query.LTE("ctime", endTime)));
        }

        public static IMongoQuery MatchMessageIds(IEnumerable<string> mids)
        {
            return Query.In("_id", new BsonArray(mids));
        }
        public static FilterDefinition<MQMessage> _MatchMessageIds(IEnumerable<string> mids)
        {
            return Builders<MQMessage>.Filter.In("_id", new BsonArray(mids));
        }
        public static IMongoQuery MatchMessageIds(IEnumerable<string> mids,int pushStatus)
        {
            return Query.And(Query.In("_id", new BsonArray(mids)), Query.EQ("pushstatus", pushStatus));
        }
        public static FilterDefinition<MQMessage> _MatchMessageIds(IEnumerable<string> mids, int pushStatus)
        {
           return Builders<MQMessage>.Filter.And(Builders<MQMessage>.Filter.In("_id", new BsonArray(mids)),
                Builders<MQMessage>.Filter.Eq("pushstatus", pushStatus));           
        }
    }
}
