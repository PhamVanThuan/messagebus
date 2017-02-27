using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using YmtSystem.Domain.MongodbRepository;
using YmatouMQMessageMongodb.Domain.Module;

namespace YmatouMQMessageMongodb.Domain.IRepository
{
    public interface IMessageStatusRepository : IMongodbRepository<MQMessageStatus>
    {
        Task TryAddAsync(MQMessageStatus msg, string dbName, string collectionName, TimeSpan timeOut);             
        Task TryBatchAddAsync(IEnumerable<MQMessageStatus> msg, string dbName, string collectionName, TimeSpan timeOut);
    }
}
