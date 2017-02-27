using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YmatouMQAdmin.Domain.IRepository;
using YmatouMQAdmin.Domain.Module;
using YmatouMQAdmin.Repository.Context;
using YmtSystem.Domain.MongodbRepository;
using YmtSystem.Repository.Mongodb;
using YmtSystem.Repository.Mongodb.Context;
using Ymatou.CommonService;
using MongoDB.Driver;

namespace YmatouMQAdmin.Repository
{
    public class MQMessageRepository : MongodbRepository<MQMessage>, IMessageRepository
    {
        public MQMessageRepository(MongodbContext context)
            : base(context)
        {
        }
        public MQMessageRepository() : this(new MQMessageContext()) { }

        public async Task AddAsync(MQMessage msg, string dbName, string collectionName, TimeSpan timeOut)
        {
            var cts = new CancellationTokenSource(timeOut);
            var token = cts.Token;
            try
            {
                await Task.Factory.StartNew(() =>
                {
                    Add(msg, dbName, collectionName);
                }, token);
            }
            catch (AggregateException ex)
            {
                foreach (var e in ex.InnerExceptions)
                    ApplicationLog.Error("MQMessageRepository addAsync AggregateException {0}", e);
            }
            catch (Exception ex)
            {
                ApplicationLog.Error("MQMessageRepository addAsync Exception {0}", ex);
            }
        }

        public async Task<MongoCursor<MQMessage>> FindAsync(IMongoQuery query, string dbName, string collectionName, int index, int limit, TimeSpan timeOut)
        {
            var cts = new CancellationTokenSource(timeOut);
            var token = cts.Token;
            try
            {
                return await Task.Factory.StartNew(() =>
                 {
                     return Find(query, dbName, collectionName, index, limit);
                 });
            }
            catch (AggregateException ex)
            {
                foreach (var e in ex.InnerExceptions)
                    ApplicationLog.Error("MQMessageRepository FindAsync AggregateException", e);

                return null;
            }
            catch (Exception ex)
            {
                ApplicationLog.Error("MQMessageRepository FindAsync Exception", ex);
                return null;
            }
        }      
    }
}
