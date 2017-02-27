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
using YmatouMQNet4.Extensions;
using YmatouMQNet4.Extensions._Task;
using YmatouMQNet4.Extensions.Serialization;
using YmatouMQNet4._Persistent;
using YmatouMQMessageMongodb.Domain.Module;
using Ymatou.PerfMonitorClient;
using _MethodMonitor = Ymatou.PerfMonitorClient.MethodMonitor;

namespace YmatouMQNet4.Core
{
    /// <summary>
    /// 发布消息-异步模式
    /// </summary>
    internal class _PublishMessageAsync : PublishMessageBase
    {
        private readonly ILog log = LogFactory.GetLogger(LogFactory._LogType, "YmatouMQ.Core._PublishMessageAsync");

        public _PublishMessageAsync()
        {
        }

        public override async Task PublishMessageAsync(PublishMessageContextAsync message)
        {
            using (_MethodMonitor.New("PublishMessageAsync_{0}_{1}".Fomart(message.context.appid, message.context.code)))
            {
                var cfg = MQMainConfigurationManager.Builder.GetConfiguration(message.context.appid, message.context.code);
                if (cfg.MessagePropertiesCfg.PersistentMessagesLocal.Value)
                {
                    //TODO:
                }
                if (cfg.MessagePropertiesCfg.PersistentMessagesMongo.Value)
                {
                    await _PersistentMessageToMongodb.PostMessageAsync(new MQMessage(message.context.appid
                                                                                    , message.context.code
                                                                                    , message.context.ip
                                                                                    , message.context.messageid
                                                                                    , message.context.body._JSONSerializationToString()
                                                                                    , null));
                }
                await PublishDeclareAsync(await message.channel(), cfg.ExchangeCfg, cfg.MessagePropertiesCfg, message.context.messageid).ContinueWith(async r =>
                {
                    if (r.IsFaulted)
                    {
                        r.Exception.Handle(log, "异步发送消息前置任务声明发布属性异常");
                        return;
                    }
                    else
                    {
                        var _channel = await message.channel();
                        await _channel.ExecutedAsync(channel =>
                                                    channel.BasicPublish(cfg.ExchangeCfg.ExchangeName
                                                    , cfg.PublishCfg.RouteKey
                                                    , r.Result
                                                    , message.context.body._JSONSerializationToByte()))
                        .ContinueWith(pub =>
                        {
                            if (pub.IsCompleted)
                            {
                                //todo:发送成功消息发送到mongodb
                                if (cfg.MessagePropertiesCfg.PersistentMessagesMongo.Value)
                                {
                                    //_PersistentMessageToMongodb.PostMessageAsync()
                                }
                                log.Debug("应用{0}消息{1}异步发布完成", message.context.appid, message.context.code);
                            }
                            if (pub.IsFaulted)
                            {
                                log.Error("异步发送消息异常,appId,{0}，msgid,{1}，error,{2}", message.context.appid, message.context.code, pub.Exception.InnerException.ToString());
                                AddMessageToExceptionQueue(new ExceptionMessageContext(message.context.appid, message.context.code, message.context.messageid, message.context.body));
                                //todo:异常消息发送到mongodb
                            }
                        });
                    }
                }).ConfigureAwait(false);
            }
        }
        //异步执行发布消息属性声明
        private Task<IBasicProperties> PublishDeclareAsync(IModel channel, ExchangeConfiguration exchangeCfg, MessagePropertiesConfiguration msgProperties, string msgid = null)
        {
            Func<IBasicProperties> task = () => { return PublishDeclare(channel, exchangeCfg, msgProperties, msgid); };
            return task.ExecuteSynchronously();
        }

        //忽略同步发送消息
        public override void PublishMessage(PublishMessageContextSync message)
        {
            throw new NotImplementedException();
        }
    }
}
