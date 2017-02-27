using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using YmatouMQAdmin.Domain.IRepository;
using YmatouMQAdmin.Domain.Module;
using YmatouMQAdmin.Repository;
using Ymatou.CommonService;
using YmatouMQ.Common.Dto;
using YmatouMQAdmin.WebApp2.Models;
using YmatouMQAdmin.Domain.Specifications;
using MongoDB.Driver;

namespace YmatouMQAdmin.WebApp2.Controllers
{
    public class MQMessagePersistentController : ApiController
    {
        //private static readonly IMessageRepository MsgRepo = new MQMessageRepository();
        //private static readonly IMessageStatusRepository statusRepo = new MessageStatusRepository();
        //消息持久化
        [Route("mq/admin/m/MessagePersistentDto")]
        public async Task Post([FromBody]MessagePersistentDto value)
        {
            if (value == null) return;
            //持久化消息
            await CfgRepositoryDeclare.MsgRepo.AddAsync(new MQMessage(value.AppId, value.Code, value.Ip, value.MsgUniqueId, value.Body, value.Status.ToString())
                , MQMessageSpecifications.MessageDb(value.AppId)
                , MQMessageSpecifications.MessageCollectionName(value.Code)
                , TimeSpan.FromMilliseconds(2000));
            //如果包含了状态，则测持久化消息状态
            if (value.Status != null)
            {
                var info = new MQMessageStatus(value.MsgUniqueId, value.Status.ToString(), value.AppId);
                await CfgRepositoryDeclare.statusRepo.AddAsync(info, info.AssignCollectionName(), TimeSpan.FromMilliseconds(2000));
            }
            ApplicationLog.Debug(string.Format("message {0} sourceip {1} add to mongodb", value.MsgUniqueId, value.Ip));
        }
        //获取持久化消息
        [Route("mq/admin/findmessage/")]
        public async Task<IEnumerable<MQMessage>> Get([FromUri]MQMessageQueryDto value)
        {
            if (value == null) return null;
            if (string.IsNullOrEmpty(value.AppId)) return null;
            if (string.IsNullOrEmpty(value.Code)) return null;
            //如果不包含状态查询
            if (!value.Status)
            {
                var result = await CfgRepositoryDeclare.MsgRepo.FindAsync(MQMessageSpecifications.MatchMessage(value.AppId, value.Message)
                                             , MQMessageSpecifications.MessageDb(value.AppId)
                                             , MQMessageSpecifications.MessageCollectionName(value.Code)
                                             , value.Skip <= 0 ? 0 : value.Skip
                                             , value.Limit <= 0 ? 50 : value.Limit
                                             , TimeSpan.FromMilliseconds(3000)).ConfigureAwait(false);
                return result.AsParallel().ToList();
            }
            else
            {
                //1.查询消息实体
                var message = await CfgRepositoryDeclare.MsgRepo.FindAsync(MQMessageSpecifications.MatchMessage(value.AppId, value.Message), MQMessageSpecifications.MessageDb(value.AppId),
                    MQMessageSpecifications.MessageCollectionName(value.Code), value.Skip <= 0 ? 0 : value.Skip, value.Limit <= 0 ? 50 : value.Limit, TimeSpan.FromMilliseconds(3000));
                //2.查询消息状态
                var result = await CfgRepositoryDeclare.statusRepo.FindMessageStatusAsync(MQMessageSpecifications.MatchMessageStatus(message.Select(e => e.MsgId)), TimeSpan.FromMilliseconds(5000), null);
                var tmpMessagelist = new List<MQMessage>();
                foreach (var item in message)
                {
                    item.Status = result.Where(e => e.MessageId == item.MsgId).ToArray();
                    tmpMessagelist.Add(item);
                }

                return tmpMessagelist;
            }
        }
    }
}
