using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using YmtSystem.Domain.MongodbRepository;
using YmatouMQMessageMongodb.Domain.Module;

namespace YmatouMQMessageMongodb.Domain.IRepository
{
    public interface IMessageRepository : IMongodbRepository<MQMessage>
    {
        Task BatchAddAsync(IEnumerable<MQMessage> documents, string dbName, string collectionName,
            WriteConcern writeConcern = null);

        Task AddAsync(MQMessage msg, string dbName, string collectionName, TimeSpan timeOut);

        IEnumerable<string> FindAllCollections(string dbName);

        Task<UpdateResult> UpdateMessageAsync(
            FilterDefinition<MQMessage> queryFilter,
            UpdateDefinition<MQMessage> updateDefinition,
            string dbName,
            string collectionName, int millisecondsDelay = 300, bool multiple = false);

        UpdateResult UpdateMessage(FilterDefinition<MQMessage> queryFilter,
            UpdateDefinition<MQMessage> updateDefinition, string dbName,
            string collectionName, bool multiple = false);

        List<MQMessage> FindMessageList(FilterDefinition<MQMessage> query, string dbName
            , string table, ProjectionDefinition<MQMessage, MQMessage> fields);
    }
}
