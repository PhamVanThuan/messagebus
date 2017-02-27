using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using YmatouMQAdmin.Domain.Module;
using YmtSystem.Domain.MongodbRepository;

namespace YmatouMQAdmin.Domain.IRepository
{
    public interface IMessageRepository : IMongodbRepository<MQMessage>
    {
        Task AddAsync(MQMessage msg, string dbName, string collectionName, TimeSpan timeOut);
        Task<MongoCursor<MQMessage>> FindAsync(IMongoQuery query, string dbName, string collectionName, int index, int limit, TimeSpan timeOut);
    }
}
