using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YmtSystem.Domain.MongodbRepository;
using MongoDB.Driver;
using YmatouMQMessageMongodb.Domain.Module;

namespace YmatouMQMessageMongodb.Domain.Domain
{
    public interface IRetryMessageRepository : IMongodbRepository<RetryMessage>
    {
        RetryMessage FindAndModify(IMongoQuery query, IMongoUpdate update, int limit, string dbName, string collectionName);
        IEnumerable<string> FindAllCollectionName(string dbName);
        Task AddAsync(RetryMessage msg, string dbName, string collectionName);
        Task Increment_RetryCount(string _id, string dbName, string collectionName, int incValue = 1);
        Task BatchAddAsync(IEnumerable<RetryMessage> documents, string dbName, string collectionName, WriteConcern writeConcern = null);
    }
}
