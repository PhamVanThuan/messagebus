using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YmatouMQ.Common;
using YmatouMQ.Common.Extensions;
using YmatouMQ.Common.Extensions.Serialization;
using YmatouMQ.Common.Extensions._Task;
using YmatouMQ.ConfigurationSync;
using YmatouMQ.Log;
using YmatouMQMessageMongodb.AppService;
using YmatouMQMessageMongodb.Domain.Module;
using YmatouMQ.Common.MessageHandleContract;

namespace YmatouMQNet4.Core
{
    class MessageStore
    {
        private static readonly ILog _log = LogFactory.GetLogger(LogEngineType.RealtimelWriteFile,
            "YmatouMQ.Core.MessageStoreToDb");
        private static readonly IRetryMessageCompensateAppService RetryMessageAppService=new RetryMessageCompensateAppService();
        //添加需要重试的消息
        public static void AddRetryMessage(PublishMessageContext context,string description=null)
        {
            AddRetryMessageBatch(new List<PublishMessageContext>() { context }, context.appid, context.code, description);
        }
        //批量添加消息日志
        public static void AddMessageBatch(IEnumerable<PublishMessageContext> context, string appid, string code,
            string ip, int pushStats = 0)
        {           
            context.EachAction(c => AddMessagePublishLog(c, pushStats));
        }
        //批量添加需要重试的消息
        public static void AddRetryMessageBatch(IEnumerable<PublishMessageContext> context, string appid, string code, string description = null)
        {
            var cfg = MQMainConfigurationManager.Builder.GetConfiguration(appid, code);
            var callbackCfg = cfg.CallbackCfgList.Where(c => c.Enable == true);
            if (!callbackCfg.Any())
            {
                _log.Debug("![AddRetryMessageBatch] appid:{0},code:{1} disable all callback url.", appid, code);
                return;
            }
            var messageList = context.CopyTo(
                m =>
                    new RetryMessage(appid
                        , code
                        , m.messageid
                        , m.body._JSONSerializationToString()
                        , DateTime.Now.AddMinutes(cfg.ConsumeCfg.RetryTimeOut.Value)
                        , uuid: m.uuid
                        , callBackKey: callbackCfg.Select(c => c.CallbackKey).ToList()                        
                        , messageSource:_MessageSource.MessageSource_Publish
                        , desc: description));
            MessageAppService.AddRetryMessageBatch(messageList, appid, code);
            _log.Info("[AddRetryMessageBatch] done,appid:{0},code:{1},desc:{2},message count:{3}",appid,code,description,context.Count());
        }
        //异步添加需要重试的消息
        public static async Task AddRetryMessageAsync(PublishMessageContext context, string description = null)
        {
            var cfg = MQMainConfigurationManager.Builder.GetConfiguration(context.appid, context.code);
            var callbackCfg = cfg.CallbackCfgList.Where(c => c.Enable == true);
            if (!callbackCfg.Any())
            {
                _log.Debug("![AddRetryMessageAsync] appid:{0},code:{1} disable all callback url.", context.appid, context.code);
                return;
            }

            var message = new RetryMessage(context.appid
                        , context.code
                        , context.messageid
                        , context.body._JSONSerializationToString()
                        , DateTime.Now.AddMinutes(cfg.ConsumeCfg.RetryTimeOut.Value)
                        , uuid: context.uuid
                        , callBackKey: callbackCfg.Select(c => c.CallbackKey).ToList()
                        , messageSource: _MessageSource.MessageSource_Publish
                        , desc: description);
            await RetryMessageAppService.AddAsync(message).ConfigureAwait(false);
            _log.Info("[AddRetryMessageAsync] done,appid:{0},code:{1},desc:{2}", context.appid, context.code, description);
        }

        //批量添加消息日志到内存队列
        public static async Task AddMessagePublishLog(PublishMessageContext context,int pushStats=0)
        {    
           await MessageAppService_TimerBatch.Instance.PostMessageAsync(new MQMessage(context.appid
                , context.code
                , context.ip
                , context.messageid
                , context.body._JSONSerializationToString()
                , null
                , context.uuid
                , pushStats)).ConfigureAwait(false);
        }
    }
}
