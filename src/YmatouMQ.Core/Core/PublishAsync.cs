using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using YmatouMQNet4.Utils;
using YmatouMQNet4.Configuration;
using YmatouMQ.Common.Extensions;
using YmatouMQ.Common.Extensions._Task;
using YmatouMQ.Common.Extensions.Serialization;
using YmatouMQMessageMongodb.Domain.Module;
using Ymatou.PerfMonitorClient;
using _MethodMonitor = Ymatou.PerfMonitorClient.MethodMonitor;
using YmatouMQ.Common;
using YmatouMQ.Log;
using YmatouMQ.Connection;
using YmatouMQMessageMongodb.AppService;
using YmatouMQ.ConfigurationSync;
using YmatouMQ.Common.MessageHandleContract;

namespace YmatouMQNet4.Core
{
    /// <summary>
    /// 发布消息-异步模式
    /// </summary>
    internal class _PublishMessageAsync : PublishMessageBase
    {
        private readonly ILog log = LogFactory.GetLogger(LogFactory._LogType, "YmatouMQ.Core._PublishMessageAsync");
      
        public override async Task PublishMessageAsync(PublishMessageContextAsync message)
        {
            var cfg = MQMainConfigurationManager.Builder.GetConfiguration(message.context.appid,
                message.context.code);
            if (cfg.MessagePropertiesCfg.PersistentMessagesLocal.Value)
            {
                //TODO:本地消息持久化
            }
            if (cfg.MessagePropertiesCfg.PersistentMessagesMongo.Value)
            {
                await MessageStore.AddMessagePublishLog(message.context).ConfigureAwait(false);
                log.Info("[PublishMessageAsync] message write to memory queue success,appid:{0},code:{1},mid:{2}",
                    message.context.appid,
                    message.context.code, message.context.messageid);
            }
            var healthCfg =
                MQMainConfigurationManager.Builder.GetConfiguration(message.context.appid).ConnCfg;
            var __channel = await message.channel().ConfigureAwait(false);
            if (__channel == null || __channel.IsClosed || (healthCfg.HealthCheck && Health.CheckHealthIsTimeOut(healthCfg.HealthSecond)))
            {
                await MessageStore.AddRetryMessageAsync(message.context,"MQConnClose").ConfigureAwait(false);
                log.Debug("[PublishMessageAsync] rabbit mq channel is null,message write to mongodb wait retry");
                return;
            }
            message.declare_op.EnsureExchangeDeclare(__channel, message.context.appid, message.context.code);
            await
                PublishDeclareAsync(__channel, cfg.ExchangeCfg, cfg.MessagePropertiesCfg, message.context.messageid,
                    message.context.uuid).ContinueWith(async r =>
                    {
                        if (r.IsFaulted)
                        {
                            r.Exception.Handle(log, "[PublishMessageAsync] PublishDeclareAsync Execution failed");
                            return;
                        }
                        else
                        {
                            await __channel.ExecutedAsync(channel =>
                                channel.BasicPublish(cfg.ExchangeCfg.ExchangeName
                                    , cfg.PublishCfg.RouteKey
                                    , r.Result
                                    , message.context.body._JSONSerializationToByte()))
                                .ContinueWith(async pub =>
                                {
                                    if (pub.IsCompleted)
                                    {
                                        log.Debug(
                                            "[PublishMessageAsync] appid:{0},code:{1},message id:{2} send to rabbitmq ok",
                                            message.context.appid,
                                            message.context.code, message.context.messageid);
                                    }
                                    if (pub.IsFaulted)
                                    {
                                        //todo:异常消息发送到mongodb 补单库                                 
                                        await MessageStore.AddRetryMessageAsync(message.context, "MQConnClose").ConfigureAwait(false);
                                        log.Error(
                                            "[PublishMessageAsync] appId:{0},code:{1}，msgid:{2},send to rabbitmq，error,{3}"
                                            , message.context.appid, message.context.code, message.context.messageid
                                            , pub.Exception.InnerException.ToString());
                                    }
                                });
                        }
                    }).ConfigureAwait(false);
        }

        //异步执行发布消息属性声明
        private Task<IBasicProperties> PublishDeclareAsync(IModel channel, ExchangeConfiguration exchangeCfg
            , MessagePropertiesConfiguration msgProperties, string msgid = null, string uuid = null)
        {
            Func<IBasicProperties> task = () => PublishDeclare(channel, msgProperties, msgid, uuid);
            return task.ExecuteSynchronously();
        }

        //忽略同步发送消息
        public override void PublishMessage(PublishMessageContextSync message)
        {
            throw new NotImplementedException();
        }       
    }
}
