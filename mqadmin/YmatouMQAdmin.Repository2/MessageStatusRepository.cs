using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using MongoDB.Driver;
using Ymatou.CommonService;
using YmatouMQAdmin.Domain.Common;
using YmatouMQAdmin.Domain.IRepository;
using YmatouMQAdmin.Domain.Module;
using YmatouMQAdmin.Repository.Context;
using YmtSystem.Repository.Mongodb;
using YmtSystem.Repository.Mongodb.Context;
using YmatouMQAdmin.Domain.Specifications;

namespace YmatouMQAdmin.Repository
{
    public class MessageStatusRepository : MongodbRepository<MQMessageStatus>, IMessageStatusRepository
    {
        public MessageStatusRepository(MongodbContext context)
            : base(context)
        {
        }
        public MessageStatusRepository() : this(new MQMessageContext()) { }

        public async Task AddAsync(MQMessageStatus msg, string collectionName, TimeSpan timeOut)
        {
            var cts = new CancellationTokenSource(timeOut);
            var token = cts.Token;
            try
            {
                await Task.Factory.StartNew(() =>
                {
                    var dbName = MQMessageSpecifications.MessageStatusDbName(); ;
                    Add(msg, dbName, collectionName);
                }, token);
            }
            catch (AggregateException ex)
            {
                foreach (var e in ex.InnerExceptions)
                    ApplicationLog.Error("MessageStatusRepository addAsync AggregateException {0}", e);
            }
            catch (Exception ex)
            {
                ApplicationLog.Error("MessageStatusRepository addAsync Exception {0}", ex);
            }
        }

        public async Task<IEnumerable<MQMessageStatus>> FindMessageStatusAsync(IMongoQuery query, TimeSpan timeOut, string collectionName = null)
        {
            var cts = new CancellationTokenSource(timeOut);
            var token = cts.Token;
            try
            {
                return await Task.Factory.StartNew(() =>
                {
                    var dbName = MQMessageSpecifications.MessageStatusDbName();
                    //如果 collectionName 为空，则查询所有状态，否则查询指定的状态
                    if (!string.IsNullOrEmpty(collectionName))
                    {
                        return Find(query, dbName, collectionName);
                    }
                    else
                    {
                        var collNames = this.Context.Database(dbName).GetCollectionNames().Where(e => !e.StartsWith("mq_"));
                        var bag = new ConcurrentBag<IEnumerable<MQMessageStatus>>();
                        Parallel.ForEach(collNames, c =>
                        {
                            var result = Find(query, dbName, c).AsEnumerable();
                            if (result.Any())
                                bag.Add(result);
                        });
                        return bag.SelectMany(e => e);
                    }
                });
            }
            catch (AggregateException ex)
            {
                foreach (var e in ex.InnerExceptions)
                    ApplicationLog.Error("MessageStatusRepository FindMessageStatusAsync AggregateException {0}", e);
                return Enumerable.Empty<MQMessageStatus>();
            }
            catch (Exception ex)
            {
                ApplicationLog.Error("MessageStatusRepository FindMessageStatusAsync Exception {0}", ex);
                return Enumerable.Empty<MQMessageStatus>();
            }
        }


        public async Task<IEnumerable<MQMessageStatus>> FindMessageStatusAsync(IMongoQuery query, string dbName, string collectionName = null)
        {
            try
            {
                return await Task.Factory.StartNew(() =>
                {
                    //如果 collectionName 为空，则查询所有状态，否则查询指定的状态
                    if (!string.IsNullOrEmpty(collectionName))
                    {
                        return Find(query, dbName, collectionName);
                    }
                    else
                    {
                        var collNames = this.Context.Database(dbName).GetCollectionNames().Where(e => !e.StartsWith("mq_"));
                        var bag = new ConcurrentBag<IEnumerable<MQMessageStatus>>();
                        Parallel.ForEach(collNames, c =>
                        {
                            var result = Find(query, dbName, c).AsEnumerable();
                            if (result.Any())
                                bag.Add(result);
                        });
                        return bag.SelectMany(e => e);
                    }
                });
            }
            catch (AggregateException ex)
            {
                foreach (var e in ex.InnerExceptions)
                    ApplicationLog.Error("MessageStatusRepository FindMessageStatusAsync AggregateException {0}", e);
                return Enumerable.Empty<MQMessageStatus>();
            }
            catch (Exception ex)
            {
                ApplicationLog.Error("MessageStatusRepository FindMessageStatusAsync Exception {0}", ex);
                return Enumerable.Empty<MQMessageStatus>();
            }
        }
    }
}
