using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Bson;
using YmatouMQMessageMongodb.Domain.Module;

namespace YmatouMQMessageMongodb.Domain.Specifications
{
    public class RetryMessageSpecifications
    {
        public static string GetCompensateMessageDbName()
        {
            return "MQ_Message_Compensate";
        }

        public static string CollectionName(string appid, string code)
        {
            return String.Format("Mq_{0}_{1}", appid, code);
        }

        public static IMongoUpdate Update_Status(RetryStatus status)
        {
            return Update.Combine(Update<RetryMessage>.Set(u => u.Status, status)
                , Update<RetryMessage>.Set(u => u.RetryTime, DateTime.Now)
                , Increment_RetryCount());
        }

        public static IMongoUpdate Update_Status(RetryStatus status, List<CallbackInfo> callbackKey, int retryCount = 1)
        {
            return Update.Combine(Update<RetryMessage>.Set(u => u.Status, status)
                , Update<RetryMessage>.Set(u => u.RetryTime, DateTime.Now)
                , Update.Set("retrycount", retryCount)
                , Update_CallbackStatus(callbackKey));
        }

        public static IMongoUpdate Update_AppKeyAndStatus(string key, RetryStatus status)
        {
            return Update.Combine(Update<RetryMessage>.Set(m => m.AppKey, key),
                Update<RetryMessage>.Set(m => m.Status, status));
        }    

        public static IMongoQuery MatchRetryMessageDate(DateTime beginTime, DateTime endTime)
        {
            return Query.And(Query<RetryMessage>.GTE(e => e.CreateTime, beginTime),
                Query<RetryMessage>.LT(e => e.CreateTime, endTime));
        }

        public static IMongoUpdate Increment_RetryCount(int incValue = 1)
        {
            return Update.Inc("retrycount", incValue);
        }

        /// <summary>
        /// 获取未超时，且没有补单的消息
        /// </summary>
        /// <param name="timeSecond"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static IMongoQuery Match_AwaitRetryMessage(TimeSpan scan, string key = "*")
        {
            var endTime = DateTime.Now.Subtract(scan);
            var query = Query.And(Query<RetryMessage>.GTE(q => q.RetryExpiredTime, DateTime.Now)
                , Query<RetryMessage>.GTE(q => q.CreateTime, endTime)
                , Query<RetryMessage>.LTE(q => q.CreateTime, DateTime.Now)
                , Query<RetryMessage>.NE(e => e.Status, RetryStatus.RetryOk));
            return query;
        }

        /// <summary>
        /// 匹配需要补发的消息
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static IMongoQuery Match_RetryMessage(string key)
        {
            var query = Query.And(Query<RetryMessage>.EQ(e => e.AppKey, key)
                // , Query<RetryMessage>.GT(e => e.RetryExpiredTime, DateTime.Now)
                , Query<RetryMessage>.NE(e => e.Status, RetryStatus.RetryOk));
            return query;
        }

        public static IMongoUpdate Update_RetryMessageStatus()
        {
            return Update.Combine(Update<RetryMessage>.Set(r => r.AppKey, "*")
                , Update<RetryMessage>.Set(r => r.CreateTime, DateTime.Now)
                , Update<RetryMessage>.Set(r => r.RetryExpiredTime, DateTime.Now.AddMinutes(5))
                , Update<RetryMessage>.Set(r => r.Status, RetryStatus.NotRetry)
                , Update<RetryMessage>.Set(r => r.IsReSetRetryStatus, "yes")
                );
        }

        public static IMongoQuery Match_Id(IEnumerable<string> ids)
        {
            //return Query.And(Query.Or(Query.EQ("appkey", "*"), Query.EQ("status", RetryStatus.RetryFail)), Query.In("_id", new BsonArray(ids)));
            return Query.In("_id", new BsonArray(ids));
        }

        public static IMongoQuery Match_Id(string id)
        {
            return Query.EQ("_id", id);
        }

        public static IMongoQuery Match_RetryTimeOut(DateTime timeOut)
        {
            return Query.And(Query<RetryMessage>.GTE(c => c.CreateTime, timeOut), Query<RetryMessage>.LTE(c => c.CreateTime, DateTime.Now)
                ,Query<RetryMessage>.LTE(c => c.RetryExpiredTime, DateTime.Now)
                , Query<RetryMessage>.EQ(c => c.RetryCount, 0));
        }

        public static IMongoUpdate Update_CallbackStatus(List<CallbackInfo> callbackKey)
        {
            return Update<RetryMessage>.Set(e => e.CallbackKey, callbackKey);
        }

        public static IMongoUpdate Update_AwaitRetryMessageStatus(string taskId, RetryStatus rStatus)
        {
            return Update.Combine(Update<RetryMessage>.Set(e => e.AppId, taskId),
                Update<RetryMessage>.Set(e => e.Status, rStatus));
        }

        public static UpdateBuilder UpdateMessageStatus(int pushStatus,string msgSource)
        {
           return Update.Combine(Update.Set("status", pushStatus), Update.Set("source", msgSource));
        }
    }
}
