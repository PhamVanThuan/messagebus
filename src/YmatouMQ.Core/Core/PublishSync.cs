using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using RabbitMQ.Client;
using YmatouMQNet4.Configuration;
using YmatouMQ.Common.Extensions;
using YmatouMQNet4.Utils;
using YmatouMQ.Common.Extensions._Task;
using YmatouMQ.Common.Extensions.Serialization;
using YmatouMQMessageMongodb.Domain.Module;
using Ymatou.PerfMonitorClient;
using _MethodMonitor = Ymatou.PerfMonitorClient.MethodMonitor;
using YmatouMQ.Common;
using YmatouMQ.Log;
using YmatouMQ.Common.Utils;
using YmatouMQMessageMongodb.AppService;
using System.Configuration;
using YmatouMQ.ConfigurationSync;
using LocalMethodMonitor=YmatouMQ.Common.Utils.MethodMonitor;

namespace YmatouMQNet4.Core.Publish
{
    /// <summary>
    /// 发布消息-同步模式
    /// </summary>
    internal class _PublishMessageSync : PublishMessageBase
    {
        private readonly ILog _log = LogFactory.GetLogger(LogEngineType.RealtimelWriteFile,
            "YmatouMQ.Core.Publish._PublishMessage");       

        /// <summary>
        /// 发布消息
        /// </summary>            
        public override void PublishMessage(PublishMessageContextSync message)
        {
            var cfg = MQMainConfigurationManager.Builder.GetConfiguration(message.context.appid, message.context.code);
            if (cfg.MessagePropertiesCfg.PersistentMessagesLocal.Value)
            {
                //TODO:实现本地持久化                
            }
            if (cfg.MessagePropertiesCfg.PersistentMessagesMongo.Value)
            {
                MessageStore.AddMessagePublishLog(message.context);
                _log.Info("[PublishMessage] message write to memory queue success,appid: {0},code:{1},mid:{2}", message.context.appid,
                    message.context.code, message.context.messageid);
            }
            var healthCfg =
                MQMainConfigurationManager.Builder.GetConfiguration(message.context.appid).ConnCfg;
            if (message.channel == null || message.channel.IsClosed || (healthCfg.HealthCheck && Health.CheckHealthIsTimeOut(healthCfg.HealthSecond)))
            {
                _log.Debug("[PublishMessage] rabbit mq channel is null,message write to mongodb wait retry");
                MessageStore.AddRetryMessage(message.context, "MQConnClose");
                return;
            }
            var pubProper = PublishDeclare(message.channel, cfg.MessagePropertiesCfg,
                message.context.messageid, message.context.uuid);
            if (cfg.PublishCfg.UseTransactionCommit.Value)
            {
                //使用事务发布消息
                using (var localMM = new LocalMethodMonitor(_log, 50, "[PublishMessage]UseTransactionCommit,appid:{0},code:{1}"
                    .Fomart(message.context.appid, message.context.code, message.context.messageid)))
                {
                    PublishMessageTransaction(
                        message.channel
                        , cfg.PublishCfg.RetryCount.Value
                        , cfg.PublishCfg.RetryMillisecond.Value
                        ,
                        () => message.channel.BasicPublish(cfg.ExchangeCfg.ExchangeName, cfg.PublishCfg.RouteKey, pubProper,
                            message.context.body._JSONSerializationToByte())
                        , () => MessageStore.AddRetryMessage(message.context, "PubException"));
                }
            }
            else
            {
                //发布消息
                using (var localMM = new LocalMethodMonitor(_log, 50, "[PublishMessage]Direct,appid:{0},code:{1}"
                    .Fomart(message.context.appid,message.context.code,message.context.messageid)))
                {
                    PublishMessage(message.context.body._JSONSerializationToByte()
                        , message.channel
                        , cfg.ExchangeCfg.ExchangeName
                        , cfg.PublishCfg.RouteKey
                        , cfg.PublishCfg.RetryCount.Value
                        , cfg.PublishCfg.RetryMillisecond.Value
                        , pubProper
                        , null
                        , () => MessageStore.AddRetryMessage(message.context, "PubException"));
                }
            }
        }

        //事务模式发布消息
        private void PublishMessageTransaction(IModel channel, uint retryCount, uint retryMillisecond, Action action,
            Action error)
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
                , error
                );
        }

        //普通模式发送消息
        private void PublishMessage(byte[] msgBody, IModel channel, string exchangeName, string routeKey,
            uint retryCount, uint retryMillisecond, IBasicProperties pubProper, Action error,
            Action exceptionAction = null)
        {
            ActionRetryHelp.Retry(
                () => channel.BasicPublish(exchangeName, routeKey, pubProper, msgBody)
                , retryCount
                , TimeSpan.FromMilliseconds(Convert.ToInt32(retryMillisecond))
                , error
                , ex => _log.Error("[PublishMessage] publish message error", ex)
                , exceptionAction);
        }

        public override Task PublishMessageAsync(PublishMessageContextAsync message)
        {
            throw new NotImplementedException();
        }
    }
}
