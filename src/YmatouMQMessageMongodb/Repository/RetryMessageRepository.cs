using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver.Builders;
using MongoDB.Driver;
using YmatouMQMessageMongodb.Domain.Domain;
using YmatouMQMessageMongodb.Domain.Module;
using YmatouMQMessageMongodb.Repository.Context;
using YmtSystem.Repository.Mongodb;
using YmatouMQ.Common.Extensions._Task;
using YmatouMQ.Common.Extensions;
using YmatouMQMessageMongodb.Domain.Specifications;

namespace YmatouMQMessageMongodb.Repository
{
    public class RetryMessageRepository : MongodbRepository<RetryMessage>, IRetryMessageRepository
    {
        public RetryMessageRepository()
            : base(new RetryMessageContext())
        {

        }
        public RetryMessage FindAndModify(IMongoQuery query, IMongoUpdate update, int limit, string dbName, string collectionName)
        {
            return this.Context.GetCollection(dbName, collectionName).FindAndModify(new FindAndModifyArgs
             {
                 Query = query,
                 Update = update,
                 VersionReturned = FindAndModifyDocumentVersion.Modified,
             }).GetModifiedDocumentAs<RetryMessage>();
        }

        public IEnumerable<string> FindAllCollectionName(string dbName)
        {
            return this.Context.Database(dbName).GetCollectionNames();
        }

        public Task AddAsync(RetryMessage msg, string dbName, string collectionName)
        {
            Action action = () => this.Context.Database(dbName).GetCollection<RetryMessage>(collectionName)
                .Insert(msg, new MongoInsertOptions { WriteConcern = new WriteConcern(1) });
            return action.ExecuteSynchronously();
        }
        public Task Increment_RetryCount(string _id, string dbName, string collectionName, int incValue = 1)
        {
            Action action = () => this.Context.Database(dbName).GetCollection<RetryMessage>(collectionName)
                .Update(RetryMessageSpecifications.Match_Id(_id), RetryMessageSpecifications.Increment_RetryCount(incValue), UpdateFlags.Upsert);
            return action.ExecuteSynchronously();
        }
        public Task BatchAddAsync(IEnumerable<RetryMessage> documents, string dbName, string collectionName, WriteConcern writeConcern = null)
        {
            var tcs = new TaskCompletionSource<object>();
            try
            {
                writeConcern = writeConcern ?? new WriteConcern(1);
                base.BatchAdd(documents, writeConcern, dbName, collectionName);
                tcs.SetResult(null);
            }
            catch (Exception ex)
            {
                Ymatou.CommonService.ApplicationLog.Error("批量写入mongodb 异常", ex);
                tcs.SetException(ex);
            }
            return tcs.Task;
        }
    }
}
