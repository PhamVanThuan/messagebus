using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using YmatouMQMessageMongodb.Domain.Domain;
using YmatouMQMessageMongodb.Repository;
using YmatouMQMessageMongodb.Domain.Module;
using YmatouMQMessageMongodb.Domain.Specifications;

namespace YmatouMQMessageMongodb.AppService
{
    public class RetryMessageCompensateAppService : IRetryMessageCompensateAppService
    {
        public readonly IRetryMessageRepository repo = new RetryMessageRepository();

        public bool CheckExists(string id,string appId,string code)
        {
           return repo.Exists(RetryMessageSpecifications.Match_Id(id), RetryMessageSpecifications.GetCompensateMessageDbName(),
                RetryMessageSpecifications.CollectionName(appId, code));
        }

        public void InsertCompensateMessage(RetryMessage msg)
        {
            repo.Add(msg, RetryMessageSpecifications.GetCompensateMessageDbName(),
                RetryMessageSpecifications.CollectionName(msg.AppId, msg.Code));
        }

        public void BatchAddMessage(IEnumerable<RetryMessage> list, string appId, string code)
        {
            repo.BatchAdd(list, WriteConcern.W1, RetryMessageSpecifications.GetCompensateMessageDbName(),
                RetryMessageSpecifications.CollectionName(appId, code));
        }

        public Task AddAsync(RetryMessage msg)
        {
            return repo.AddAsync(msg, RetryMessageSpecifications.GetCompensateMessageDbName(),
                RetryMessageSpecifications.CollectionName(msg.AppId, msg.Code));
        }

        public long UpdateAppKey_Test(string key)
        {
            var result = FindAwaitRetryMessageIds(0, 3, TimeSpan.FromMinutes(1),
                RetryMessageSpecifications.CollectionName("sms", "test"));
            return Update_AppKeyAndStatus(key, result, RetryMessageSpecifications.CollectionName("sms", "test"),
                RetryStatus.Retrying);
        }

        public Task UpdateCallback(string _id, List<CallbackInfo> callbackkey, string collectionName)
        {
            var tcs = new TaskCompletionSource<WriteConcernResult>();
            try
            {
                var result = repo.Update(RetryMessageSpecifications.Match_Id(_id)
                    , RetryMessageSpecifications.Update_CallbackStatus(callbackkey)
                    , RetryMessageSpecifications.GetCompensateMessageDbName()
                    , collectionName);
                tcs.SetResult(result);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
            return tcs.Task;
        }

        public Task<WriteConcernResult> UpdateStatus(string id, string collectionName, RetryStatus status)
        {
            var tcs = new TaskCompletionSource<WriteConcernResult>();
            try
            {
                var result = repo.Update(RetryMessageSpecifications.Match_Id(id)
                    , RetryMessageSpecifications.Update_Status(status)
                    , RetryMessageSpecifications.GetCompensateMessageDbName()
                    , collectionName);
                tcs.SetResult(result);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
            return tcs.Task;
        }

        public Task<WriteConcernResult> UpdateRetryTimeOutStatus(string collectionName, DateTime time)
        {
            var tcs = new TaskCompletionSource<WriteConcernResult>();
            try
            {
                var result = repo.Update(RetryMessageSpecifications.Match_RetryTimeOut(time)
                    , RetryMessageSpecifications.Update_RetryMessageStatus()
                    , new MongoUpdateOptions {Flags = UpdateFlags.Multi}
                    , RetryMessageSpecifications.GetCompensateMessageDbName()
                    , collectionName);
                tcs.SetResult(result);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
            return tcs.Task;
        }

        public Task<WriteConcernResult> UpdateStatusAndIncrementRetryCount(string id, string collectionName,
            RetryStatus status, List<CallbackInfo> callbackList, int retryCount = 1)
        {
            var tcs = new TaskCompletionSource<WriteConcernResult>();
            try
            {
                var result = repo.Update(RetryMessageSpecifications.Match_Id(id)
                    , RetryMessageSpecifications.Update_Status(status, callbackList, retryCount)
                    , RetryMessageSpecifications.GetCompensateMessageDbName()
                    , collectionName);
                tcs.SetResult(result);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
            return tcs.Task;
        }

        public IEnumerable<RetryMessage> FindRealAwaitRetryMessage(string taskKey, int index, int limit,
            string collectionName)
        {
            return repo.Find(RetryMessageSpecifications.Match_RetryMessage(taskKey)
                , RetryMessageSpecifications.GetCompensateMessageDbName()
                , collectionName
                , SortBy.Descending("ctime")
                , index
                , limit);
        }

        public long Update_AppKeyAndStatus(string key, IEnumerable<string> ids, string collectionName,
            RetryStatus status)
        {
            return repo.Update(RetryMessageSpecifications.Match_Id(ids)
                , RetryMessageSpecifications.Update_AppKeyAndStatus(key, status)
                , new MongoUpdateOptions {Flags = UpdateFlags.Multi}
                , RetryMessageSpecifications.GetCompensateMessageDbName()
                , collectionName).DocumentsAffected;
        }

        /// <summary>
        /// 查找需要补发消息的唯一标识(_id)
        /// </summary>
        /// <param name="index"></param>
        /// <param name="limit"></param>
        /// <param name="second"></param>
        /// <param name="collectionName"></param>
        /// <returns></returns>
        public IEnumerable<string> FindAwaitRetryMessageIds(int index, int limit, TimeSpan scan, string collectionName,
            string key = "*")
        {
            return repo.Find(RetryMessageSpecifications.Match_AwaitRetryMessage(scan)
                , RetryMessageSpecifications.GetCompensateMessageDbName()
                , collectionName
                , SortBy.Descending("ctime")
                , Fields.Include("_id")
                , index
                , limit).Select(m => m._id);
        }

        public IEnumerable<RetryMessage> FindAwaitRetryMessageAndSetStatus(int index, int limit, TimeSpan scan
            , string collectionName, RetryStatus status, string key = "*")
        {
            return repo.FindAndModify<IEnumerable<RetryMessage>>(new FindAndModifyArgs
            {
                Query = RetryMessageSpecifications.Match_AwaitRetryMessage(scan),
                Update = RetryMessageSpecifications.Update_AwaitRetryMessageStatus(key, status),
            }, RetryMessageSpecifications.GetCompensateMessageDbName()
                , collectionName).Item1;
        }

        public IEnumerable<string> FindAllCollection(string dbName = null)
        {
            var dbname = dbName ?? RetryMessageSpecifications.GetCompensateMessageDbName();
            return repo.FindAllCollectionName(dbname).Where(c => c.StartsWith("Mq_"));
        }

        public Task Increment_RetryCount(string _id, string dbName, string collectionName, int incValue = 1)
        {
            return repo.Increment_RetryCount(_id, dbName ?? RetryMessageSpecifications.GetCompensateMessageDbName(),
                collectionName, incValue);
        }
    }
}
