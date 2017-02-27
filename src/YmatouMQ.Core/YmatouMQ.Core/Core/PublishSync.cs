using System;
using System.Collections.Generic;
using System.Linq;
using RabbitMQ.Client;
using YmatouMQNet4.Configuration;
using YmatouMQNet4.Extensions;
using YmatouMQNet4.Utils;
using YmatouMQNet4.Extensions._Task;
using System.Threading.Tasks;
using YmatouMQNet4.Extensions.Serialization;
using YmatouMQNet4._Persistent;
using System.Diagnostics;
using YmatouMQMessageMongodb.Domain.Module;
using Ymatou.PerfMonitorClient;
using _MethodMonitor = Ymatou.PerfMonitorClient.MethodMonitor;
using YmatouMQMessageMongodb.AppService;

namespace YmatouMQNet4.Core.Publish
{
    /// <summary>
    /// 发布消息-同步模式
    /// </summary>
    internal class _PublishMessageSync : PublishMessageBase
    {
        private readonly ILog _log = LogFactory.GetLogger(LogEngineType.RealtimelWriteFile, "YmatouMQ.Core.Publish._PublishMessage");
        private readonly RetryMessageCompensateAppService repo_Retry = new RetryMessageCompensateAppService();
        /// <summary>
        /// 发布消息
        /// </summary>            
        public override void PublishMessage(PublishMessageContextSync message)
        {
            //声明发布消息属性               
            _log.Debug("1接到客户端消息(_PublishMessageSync) appid: {0} ,code:{1},body: {2}", message.context.appid, message.context.code, message.context.body);
            var cfg = MQMainConfigurationManager.Builder.GetConfiguration(message.context.appid, message.context.code);
            if (cfg.MessagePropertiesCfg.PersistentMessagesLocal.Value)
            {
                //TODO:实现本地持久化                
            }
            if (cfg.MessagePropertiesCfg.PersistentMessagesMongo.Value)
            {
                var result = _PersistentMessageToMongodb.PostMessageAsync(new MQMessage(message.context.appid
                                                                                        , message.context.code
                                                                                        , message.context.ip
                                                                                        , message.context.messageid
                                                                                        , message.context.body._JSONSerializationToString()
                                                                                        , null));
            }
            var pubProper = PublishDeclare(message.channel, cfg.ExchangeCfg, cfg.MessagePropertiesCfg, message.context.messageid);
            if (cfg.PublishCfg.UseTransactionCommit.Value)
            {
                //使用事务发布消息
                PublishMessageTransaction(
                    message.channel
                    , cfg.PublishCfg.RetryCount.Value
                    , cfg.PublishCfg.RetryMillisecond.Value
                    , () => message.channel.BasicPublish(cfg.ExchangeCfg.ExchangeName, cfg.PublishCfg.RouteKey, pubProper, message.context.body._JSONSerializationToByte())
                    , () => AddMessageToExceptionQueue(new ExceptionMessageContext(message.context.appid, message.context.code, message.context.messageid, message.context.body)
                    ));
            }
            else
            {
                //发布消息
                PublishMessage(message.context.body._JSONSerializationToByte(), message.channel, cfg.ExchangeCfg.ExchangeName
                            , cfg.PublishCfg.RouteKey
                            , cfg.PublishCfg.RetryCount.Value
                            , cfg.PublishCfg.RetryMillisecond.Value
                            , pubProper
                            , () => AddMessageToExceptionQueue(new ExceptionMessageContext(message.context.appid, message.context.code, message.context.messageid, message.context.body))
                            , async () => await repo_Retry.AddAsync(new RetryMessage(message.context.appid, message.context.code, message.context.messageid, message.context.body, DateTime.Now, null)));
            }

        }
        //事务模式发布消息
        private void PublishMessageTransaction(IModel channel, uint retryCount, uint retryMillisecond, Action action, Action error)
        {
            ActionRetryHelp.Retry(() =>
            {
                channel.TxSelect();
                action();
                channel.TxCommit();
                _log.Debug("基于事务发布消息完成");
            }
            , retryCount
            , TimeSpan.FromMilliseconds(Convert.ToInt32(retryMillisecond))
            , () => channel.TxRollback()
            , ex => _log.Error("基于事务模式发布消息异常 {0}", ex)
            , error);
        }
        //普通模式发送消息
        private void PublishMessage(byte[] msgBody, IModel channel, string exchangeName, string routeKey, uint retryCount, uint retryMillisecond, IBasicProperties pubProper, Action error, Action exceptionAction = null)
        {
            ActionRetryHelp.Retry(
                    () => channel.BasicPublish(exchangeName, routeKey, pubProper, msgBody)
                    , retryCount
                    , TimeSpan.FromMilliseconds(Convert.ToInt32(retryMillisecond))
                    , exceptionAction
                    , ex => _log.Error("发送消息,异常 {0}", ex)
                    , error);
        }

        public override Task PublishMessageAsync(PublishMessageContextAsync message)
        {
            throw new NotImplementedException();
        }
    }
}
