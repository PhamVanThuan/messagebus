using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YmtSystem.Domain.MongodbRepository;
using YmtSystem.Repository.Mongodb;
using YmtSystem.Repository.Mongodb.Context;
using MongoDB.Driver;
using YmatouMQMessageMongodb.Domain.Module;
using YmatouMQMessageMongodb.Domain.IRepository;
using YmatouMQMessageMongodb.Repository.Context;
using log4net;
using MongoDB.Bson;
using Ymatou.CommonService;
using YmatouMQ.Common.Extensions;
using YmatouMQ.Common.Extensions._Task;
using YmtSystem.Repository.Mongodb.Extend;

namespace YmatouMQMessageMongodb.Repository
{
    public class MQMessageRepository : MongodbRepository<MQMessage>, IMessageRepository
    {       
        public MQMessageRepository(MongodbContext context)
            : base(context)
        {
        }

        public MQMessageRepository() : this(new MQMessageContext())
        {
        }
        //批量异步插入
        public async Task BatchAddAsync(IEnumerable<MQMessage> documents, string dbName, string collectionName,
            WriteConcern writeConcern = null)
        {           
            try
            {
                await BulkWriteAsync(documents.ToInsertOneModel(), dbName, collectionName)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                ApplicationLog.Error("[BatchAddAsync] 批量写入mongodb 异常", ex);               
            }       
        }
        //单个消息异步插入并指定超时时间
        public async Task AddAsync(MQMessage msg, string dbName, string collectionName, TimeSpan timeOut)
        {
            var cts = new CancellationTokenSource(timeOut);
            try
            {
                await AddAsync(msg, dbName, collectionName, cts.Token)
                    .ConfigureAwait(false);
            }           
            catch (Exception ex)
            {
                ApplicationLog.Error("MQMessageRepository addAsync Exception {0}", ex);
            }
        }
        //获取所有集合名称
        public IEnumerable<string> FindAllCollections(string dbName)
        {
            return ContextNewCore.Database(dbName).ListCollections()
                .ToList().Select(c=>c.GetElement("name").Value.AsString);
        }
        //获取消息集合
        public List<MQMessage> FindMessageList(FilterDefinition<MQMessage> query, string dbName
            , string table, ProjectionDefinition<MQMessage,MQMessage> fields)
        {
            return
                this.ContextNewCore.GetCollection<MQMessage>(dbName, table)
                    .Find(query).Project(fields).ToList();
        }
        //异步更新消息
        public async Task<UpdateResult> UpdateMessageAsync(
            FilterDefinition<MQMessage> queryFilter,
            UpdateDefinition<MQMessage> updateDefinition,
            string dbName,
            string collectionName, int millisecondsDelay = 300, bool multiple = false)
        {          
            if (multiple)
            {
                return await ContextNewCore.GetCollection<MQMessage>(dbName, collectionName)
                     .UpdateManyAsync(queryFilter, updateDefinition,
                         cancellationToken: new CancellationTokenSource(millisecondsDelay).Token)
                         .WithHandleException(ex => ApplicationLog.Error("[UpdateManyAsync] exception ",ex))
                         .ConfigureAwait(false);     
            }
            else
            {
              return  await ContextNewCore.GetCollection<MQMessage>(dbName, collectionName)
                    .UpdateOneAsync(queryFilter, updateDefinition,
                        cancellationToken: new CancellationTokenSource(millisecondsDelay).Token)
                        .WithHandleException(ex => ApplicationLog.Error("[UpdateOneAsync] exception ", ex))
                        .ConfigureAwait(false);     
            }
           
        }
        //同步更新
        public UpdateResult UpdateMessage(FilterDefinition<MQMessage> queryFilter,
            UpdateDefinition<MQMessage> updateDefinition, string dbName,
            string collectionName, bool multiple = false)
        {
            if (multiple)
                return ContextNewCore.GetCollection<MQMessage>(dbName, collectionName)
                    .UpdateMany(queryFilter, updateDefinition);
            return ContextNewCore.GetCollection<MQMessage>(dbName, collectionName)
                .UpdateOne(queryFilter, updateDefinition);
        }
    }
}
