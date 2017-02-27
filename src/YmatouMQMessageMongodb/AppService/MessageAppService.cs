using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Ymatou.CommonService;
using YmatouMQ.Common.Extensions;
using YmatouMQ.Common.MessageHandleContract;
using YmatouMQMessageMongodb.Domain.Domain;
using YmatouMQMessageMongodb.Domain.IRepository;
using YmatouMQMessageMongodb.Domain.Module;
using YmatouMQMessageMongodb.Domain.Specifications;
using YmatouMQMessageMongodb.Repository;

namespace YmatouMQMessageMongodb.AppService
{
    public class MessageAppService
    {
        private static readonly IMessageRepository messageRepository = new MQMessageRepository();
        private static readonly IRetryMessageRepository retryRepo = new RetryMessageRepository();
        private static readonly IMessageStatusRepository statusRepo = new MessageStatusRepository();



        public static bool ExistsPushMessageStatus( string id, string appid, string code)
        {
            var db = MQMessageStatus.GetDbName();
            var tb = MQMessageStatus.GetCollectionName(appid);
            return statusRepo.Exists(MQMessageSpecifications.MatchMessageUniqueId(id), db, tb);

        }

        //检查是否存在重试的消息
        public static bool CheckExistsRetryMessage(string id, string appId, string code)
        {
            return retryRepo.Exists(RetryMessageSpecifications.Match_Id(id),
                RetryMessageSpecifications.GetCompensateMessageDbName(),
                RetryMessageSpecifications.CollectionName(appId, code));
        }
        //异步添加消息状态
        public static async Task TryAddMQMessageStatusInfoAsync(MQMessageStatus msg, string appid, string code)
        {
            var db = MQMessageStatus.GetDbName();
            var tb = MQMessageStatus.GetCollectionName(appid);
            await statusRepo.TryAddAsync(msg, db, tb, TimeSpan.FromSeconds(3)).ConfigureAwait(false);
        }
        public static void SaveMessageStatus(MQMessageStatus msg, string appid, string code)
        {
            var db = MQMessageStatus.GetDbName();
            var tb = MQMessageStatus.GetCollectionName(appid);
            statusRepo.Save(msg, db, tb);
        }
        //批量更新消息状态
        public static async Task TryUpdateMultipleMessageStatusTask(IEnumerable<string> ids, int status, string appid,
            string code)
        {
            try
            {
                var db = MessageDbCollections.GenerateDbName(appid);
                var tb = MessageDbCollections.GenerateCollectionsName(code);
                var upResult = await messageRepository.UpdateMessageAsync(MQMessageSpecifications._MatchMessageIds(ids),
                    MQMessageSpecifications._UpdateMessageStatus(status), db, tb, multiple: true).ConfigureAwait(false);
                ApplicationLog.Debug(
                    "[TryUpdateMultipleMessageStatusTask] appid:{0},code:{1},ids count:{2},status:{3},db:{4},table:{5}, update result:{6}.".Fomart(appid,
                        code,
                        ids.Count(), status, db, tb, upResult.ModifiedCount));
            }
            catch (OperationCanceledException ex)
            {
                ApplicationLog.Error("appid:{0},code:{1},ids count:{2},status update result OperationCanceledException {3}"
                    .Fomart(
                        appid, code, ids.Count(), ex.ToString()));
            }
            catch (Exception ex)
            {
                ApplicationLog.Error("appid:{0},code:{1},ids count:{2},status update result Exception {3}".Fomart(
                    appid, code, ids.Count(), ex.ToString()));
            }
        }
        //查询消息推送状态
        public static int FindMessagePushStatus(string id,string appId,string code)
        {
            var db = MessageDbCollections.GenerateDbName(appId);
            var tb = MessageDbCollections.GenerateCollectionsName(code);
            var message = messageRepository.FindMessageList(MQMessageSpecifications._MatchMessageUniqueId(id), db, tb,
                Builders<MQMessage>.Projection.Include("pushstatus")).FirstOrDefault();
            return  message==null?Int32.MinValue: 
            message.PushStatus;
        }
        //查询消息推送状态
        public static bool IsCehckMessageRetry(string id, string appId, string code)
        {
            var status = FindMessagePushStatus(id, appId, code);
            return status == MQMessage.Init ? false : true;
        }
        //批量更新消息状态
        public static void TryUpdateMultipleMessageStatus(IEnumerable<string> ids, int status, string appid, string code)
        {
            try
            {
                var db = MessageDbCollections.GenerateDbName(appid);
                var tb = MessageDbCollections.GenerateCollectionsName(code);
                var upResult = messageRepository.UpdateMessage(MQMessageSpecifications._MatchMessageIds(ids),
                    MQMessageSpecifications._UpdateMessageStatus(status), db, tb, multiple: true);
                ApplicationLog.Debug(
                    "appid:{0},code:{1},ids count:{2},status:{3},db:{4},table:{5}, update result:{6}.".Fomart(appid,
                        code,
                        ids.Count(), status, db, tb, upResult.ModifiedCount));
            }
            catch (OperationCanceledException ex)
            {
                ApplicationLog.Error("appid:{0},code:{1},ids count:{2},status update result OperationCanceledException {3}"
                    .Fomart(
                        appid, code, ids.Count(), ex.ToString()));
            }
            catch (Exception ex)
            {
                ApplicationLog.Error("appid:{0},code:{1},ids count:{2},status update result Exception {3}".Fomart(
                    appid, code, ids.Count(), ex.ToString()));
            }
        }
        //查找消息唯一ID集合
        public static IEnumerable<string> FindMessageId(IEnumerable<string> ids
            , string appid, string code, int pushStatus)
        {
            var db = MessageDbCollections.GenerateDbName(appid);
            var tb = MessageDbCollections.GenerateCollectionsName(code);

            return
                messageRepository.FindMessageList(MQMessageSpecifications._MatchMessageIds(ids), db, tb,
                    Builders<MQMessage>.Projection.Include("_id")).Select(m => m._id);
        }
        //更新消息推送状态
        public static IEnumerable<MessagePushStatus2> TryUpdateMessagePushStatus(
            IEnumerable<MessagePushStatus2> ids)
        {
            try
            {
                var list = new ConcurrentBag<MessagePushStatus2>();
                ids.GroupBy(m => m.AppId).EachAction(g =>
                {
                    var appid = g.Key;
                    g.GroupBy(c => c.Code).EachAction(code =>
                    {
                        var upIds = code.Select(m => m.UuId);

                        var upResult = MessageAppService.TryFindAndUpdateMessagePushStatus(upIds,
                            MQMessage.AlreadyPush,
                            appid, code.Key);
                        if (upResult.Any())
                        {
                            upResult.EachAction(
                                c => list.Add(new MessagePushStatus2 {AppId = appid, Code = code.Key, UuId = c}));
                        }
                    });
                });
                return list;

            }
            catch (Exception ex)
            {
                ApplicationLog.Error("appid:{0},code:{1},ids count:{2},status update result Exception {3}".Fomart(
                    null, null, ids.Count(), ex.ToString()));
            }
            return Enumerable.Empty<MessagePushStatus2>();
        }
        //批量添加需要重试的消息
        public static void AddRetryMessageBatch(IEnumerable<RetryMessage> message, string appId, string code)
        {
            if (!message.Any()) return;

            var db = RetryMessageSpecifications.GetCompensateMessageDbName();
            var tb = RetryMessageSpecifications.CollectionName(appId, code);
            if (message.Count() <= 500)
                retryRepo.BatchAdd(message, WriteConcern.W1, db,
                    tb);
            else
            {
                var pagecount = message.Count()/500 + (message.Count()%500 > 0 ? 1 : 0);
                for (int i = 0; i < pagecount; i++)
                {
                    var msg = message.Skip(i).Take(i*500);
                    retryRepo.BatchAdd(msg, WriteConcern.W1, db, tb);
                    ApplicationLog.Debug("AddRetryMessageBatch appid:{0},code:{1},pageIndex:{2},pageCount:{3} batch add success.".Fomart(appId, code,
                    i, pagecount));
                }
            }
        }
        //更新消息推送状态
        private static IEnumerable<string> TryFindAndUpdateMessagePushStatus(IEnumerable<string> ids, int status,
            string appid, string code)
        {
            var db = MessageDbCollections.GenerateDbName(appid);
            var tb = MessageDbCollections.GenerateCollectionsName(code);
            try
            {                
                var upResult =
                    messageRepository.UpdateMessage(MQMessageSpecifications._MatchMessageIds(ids, MQMessage.Init),
                        MQMessageSpecifications._UpdateMessageStatus(status), db, tb, multiple: true);
                ApplicationLog.Debug(
                    "[TryFindAndUpdateMessagePushStatus] appid:{0},code:{1},ids count:{2},status:{3},db:{4},table:{5}, update result:{6}.".Fomart(appid,
                        code,
                        ids.Count(), status, db, tb, upResult.ModifiedCount));
                if (upResult.MatchedCount <= 0)
                {
                    ApplicationLog.Debug("#[TryFindAndUpdateMessagePushStatus] no match ,db:{0},table:{1}".Fomart(db,tb));
                    return ids;
                }
                var messageIds = FindMessageId(ids, appid, code, MQMessage.AlreadyPush);
                return ids.Except(messageIds);
            }
            catch (Exception ex)
            {
                ApplicationLog.Error("[TryFindAndUpdateMessagePushStatus] appid:{0},code:{1},ids count:{2},status update result Exception {3}".Fomart(
                    appid, code, ids.Count(), ex.ToString()));
                return ids;
            }           
        }
    }
}
