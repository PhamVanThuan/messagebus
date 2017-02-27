using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using YmatouMQAdmin.Domain.Common;
using YmatouMQAdmin.Domain.Module;
using YmtSystem.Domain.MongodbRepository;

namespace YmatouMQAdmin.Domain.IRepository
{
    public interface IMessageStatusRepository : IMongodbRepository<MQMessageStatus>
    {
        Task AddAsync(MQMessageStatus msg, string collectionName, TimeSpan timeOut);
        Task<IEnumerable<MQMessageStatus>> FindMessageStatusAsync(IMongoQuery query, TimeSpan timeOut, string collectionName = null);

        Task<IEnumerable<MQMessageStatus>> FindMessageStatusAsync(IMongoQuery query, string dbName, string collectionName = null);
    }
}
