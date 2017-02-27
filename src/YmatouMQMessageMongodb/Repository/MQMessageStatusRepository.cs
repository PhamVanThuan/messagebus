using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using MongoDB.Driver;
using YmtSystem.Repository.Mongodb;
using YmtSystem.Repository.Mongodb.Context;
using YmatouMQMessageMongodb.Domain.Module;
using YmatouMQMessageMongodb.Repository.Context;
using YmatouMQMessageMongodb.Domain.IRepository;
using YmatouMQ.Common.Extensions;
using YmatouMQMessageMongodb.Domain.Specifications;
using YmatouMQ.Common.Extensions._Task;
using YmatouMQ.Log;
using YmtSystem.Repository.Mongodb.Extend;
using ILog = YmatouMQ.Common.ILog;

namespace YmatouMQMessageMongodb.Repository
{
    public class MessageStatusRepository : MongodbRepository<MQMessageStatus>, IMessageStatusRepository
    {
        private static readonly ILog logger =
            LogFactory.GetLogger( LogEngineType.RealtimelWriteFile,"YmatouMQMessageMongodb.Repository.MessageStatusRepository");

        public MessageStatusRepository(MongodbContext context)
            : base(context)
        {
        }

        public MessageStatusRepository() : this(new MQMessageContext())
        {
        }
        //批量异步添加消息状态数据
        public async Task TryBatchAddAsync(IEnumerable<MQMessageStatus> msg, string dbName, string collectionName,
            TimeSpan timeOut)
        {
            try
            {
                await
                    BulkWriteAsync(msg.ToInsertOneModel(), dbName, collectionName,
                        new CancellationTokenSource(timeOut).Token);
            }
            catch (AggregateException ex)
            {
                ex.Handle(logger,"MessageStatusRepository addAsync.1 AggregateException");               
            }
            catch (Exception ex)
            {
                if (!(ex.GetBaseException() is MongoDuplicateKeyException))
                    ex.Handle(logger, "MessageStatusRepository TryBatchAddAsync.1 Exception");                   
            }

        }       
        //异步添加单条消息状态数据
        public async Task TryAddAsync(MQMessageStatus msg, string dbName, string collectionName, TimeSpan timeOut)
        {                       
            try
            {
                await AddAsync(msg, dbName, collectionName, new CancellationTokenSource(timeOut).Token);
            }          
            catch (Exception ex)
            {
                if (!(ex.GetBaseException() is MongoDuplicateKeyException) 
                    && !(ex.GetBaseException() is MongoWriteException))
                    ex.Handle(logger, "MessageStatusRepository addAsync.1 Exception");                          
            }
        }          
    }
}
