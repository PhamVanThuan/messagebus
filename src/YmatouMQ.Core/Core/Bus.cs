using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using YmatouMQNet4.Configuration;
using YmatouMQ.Connection;
using YmatouMQNet4.Core.Publish;
using YmatouMQNet4.Utils;
using YmatouMQ.Common.Extensions;
using YmatouMQ.Common.Extensions.Serialization;
using YmatouMQ.Common.Extensions._Task;
using RabbitMQ.Client;
using YmatouMQNet4.Logs;
using YmatouMQ.Common;
using YmatouMQ.Log;
using YmatouMQ.Common.Utils;
using YmatouMQ.ConfigurationSync;
using YmatouMQMessageMongodb.AppService;
using YmatouMQMessageMongodb.Domain.Module;

namespace YmatouMQNet4.Core
{
    internal class Bus : IRabbitMQBus, IConnRecoveryNotify
    {
        private static readonly Lazy<IRabbitMQBus> lazy = new Lazy<IRabbitMQBus>(() => new Bus());
        private static BusApplicationStatus status = BusApplicationStatus.NotStart;
        private readonly ILog log = LogFactory.GetLogger(LogEngineType.RealtimelWriteFile, "YmatouMQ.Core.Bus");
        private readonly MQConnectionPoolManager pool = new MQConnectionPoolManager();
        private readonly MQDeclareCache exchange = new MQDeclareCache();
        private readonly PublishMessageBase pubSync = PublishMessageFactory.Context(PublishMessageType.Sync);
        private readonly PublishMessageBase pubAsync = PublishMessageFactory.Context(PublishMessageType.Async);
        private readonly PublishMessageBase pubBuffer = PublishMessageFactory.Context(PublishMessageType.BufferAsync);
        private static readonly string BusAppNoStart_Description = "BusAppNoStart";
        private static readonly string AppIdConnPoolNoExists_Description = "AppIdConnPoolNoExists";
        private static readonly string PublishToDb_Description = "DirectPublishToDb";
        /// <summary>
        /// Builder rabbitmq BUS
        /// </summary>
        public static IRabbitMQBus Builder
        {
            get { return lazy.Value; }
        }

        private Bus()
        {

        }

        public IEnumerable<string> GetConnectionPoolKeys
        {
            get { return pool.ConnectionPoolKeys; }
        }

        public IEnumerable<string> GetChannelsStatus {
            get { return pool.AllChannelStatus; }
        }

        public void Publish(PublishMessageContext messageContext)
        {    
            using (var monitor = new MethodMonitor(null))
            {
                log.Debug("[PublishSync] receive from client message,appid:{0},code:{1},mid:{2},cip:{3}",
                    messageContext.appid, messageContext.code, messageContext.messageid, messageContext.ip);
                var msgCfg = MQMainConfigurationManager.Builder.GetConfiguration(messageContext.appid, messageContext.code);
                if (!msgCfg.Enable)
                {
                    log.Debug("[PublishSync] appId:{0}，code:{1} disable,,execute:{2:N0} ms", messageContext.appid, messageContext.code, monitor.GetRunTime2);
                    return;
                }
                if (!CheckBusApplicationIsRuning())
                {
                    MessageStore.AddMessagePublishLog(messageContext);
                    MessageStore.AddRetryMessage(messageContext, BusAppNoStart_Description);
                    log.Debug(
                        "$[PublishSync] appid:{0},code:{1},message mid:{2},busStatus:{3} add publish log,retry message ok,execute:{4:N0} ms",
                        messageContext.appid,
                        messageContext.code, messageContext.messageid, status,monitor.GetRunTime2);
                    return;
                }
                if (!EnsureCreateConnectionPool(messageContext.appid))
                {
                    MessageStore.AddMessagePublishLog(messageContext);
                    MessageStore.AddRetryMessage(messageContext, AppIdConnPoolNoExists_Description);
                    log.Debug(
                        "#[PublishSync] appid:{0},code:{1},message mid:{2} EnsureCreateConnectionPool is false,message add publish log,retry message ok,execute:{3:N0} ms"
                        , messageContext.appid, messageContext.code, messageContext.messageid,monitor.GetRunTime2);
                    return;
                }
           
               var _channel = pool.CreateChannel(messageContext.appid);             
                    exchange.EnsureExchangeDeclare(_channel, messageContext.appid, messageContext.code);
                    pubSync.PublishMessage(new PublishMessageContextSync {context = messageContext, channel = _channel});
                log.Debug(
                    "[PublishSync]send done,msgId:{0},execute:{1:N0} ms",
                     messageContext.messageid, monitor.GetRunTime2);
            }
        }

        public void Publish(IEnumerable<PublishMessageContext> messageContext)
        {
            messageContext.EachAction(m => Publish(m));
        }

        public async Task PublishAsync(PublishMessageContext messageContext)
        {
            log.Debug(
                "[PublishAsync] receive from client message,appid:{0},code:{1},mid:{2},cip:{3},body:{4}",
                messageContext.appid, messageContext.code, messageContext.messageid, messageContext.ip,
                messageContext.body);
            var msgCfg = MQMainConfigurationManager.Builder.GetConfiguration(messageContext.appid, messageContext.code);
            if (!msgCfg.Enable)
            {
                log.Debug("[PublishAsync] appId:{0}，code:{1} disable", messageContext.appid, messageContext.code);
                return;
            }
            if (!CheckBusApplicationIsRuning())
            {
                MessageStore.AddMessagePublishLog(messageContext);
                MessageStore.AddRetryMessage(messageContext, BusAppNoStart_Description);
                log.Debug(
                    "[PublishAsync] appid:{0},code:{1},message mid:{2},busStatus:{3} add publish log,retry message ok",
                    messageContext.appid,
                    messageContext.code, messageContext.messageid, status);
                return;
            }
            if (!EnsureCreateConnectionPool(messageContext.appid))
            {
                MessageStore.AddMessagePublishLog(messageContext);
                MessageStore.AddRetryMessage(messageContext, AppIdConnPoolNoExists_Description);
                log.Debug("[PublishAsync] appid:{0},code:{1},message mid:{2} add publish log,retry message ok",
                    messageContext.appid,
                    messageContext.code, messageContext.messageid);
                return;
            }
            using (var monitor = new MethodMonitor(null))
            {
                await pubAsync.PublishMessageAsync(new PublishMessageContextAsync
                {
                    context = messageContext,
                    declare_op = exchange,
                    channel = () => pool.CreateChannelAsync(messageContext.appid)
                }).ConfigureAwait(false);
                log.Debug("[PublishAsync] message send to rabbitmq ok,appId:{0},code:{1},msgId:{2},execute:{3:N0} ms",
                    messageContext.appid,
                    messageContext.code, messageContext.messageid, monitor.GetRunTime2);
            }
        }

        public async Task PublishBufferAsync(PublishMessageContext messageContext)
        {
            log.Debug(
                "[PublishBufferAsync] receive from client message [PublishAsync],appid:{0},code:{1},mid:{2},cip:{3},body:{4}",
                messageContext.appid, messageContext.code, messageContext.messageid, messageContext.ip,
                messageContext.body);
            var msgCfg = MQMainConfigurationManager.Builder.GetConfiguration(messageContext.appid, messageContext.code);
            if (!msgCfg.Enable)
            {
                log.Debug("[PublishBufferAsync] appId:{0},code:{1} disable", messageContext.appid, messageContext.code);
                return;
            }
            if (!CheckBusApplicationIsRuning())
            {
                MessageStore.AddMessagePublishLog(messageContext);
                MessageStore.AddRetryMessage(messageContext, BusAppNoStart_Description);
                log.Debug("[PublishBufferAsync] appid:{0},code:{1},message mid:{2} add publish log,retry message ok",
                    messageContext.appid,
                    messageContext.code, messageContext.messageid);
                return;
            }
            if (!EnsureCreateConnectionPool(messageContext.appid))
            {
                MessageStore.AddMessagePublishLog(messageContext);
                MessageStore.AddRetryMessage(messageContext, AppIdConnPoolNoExists_Description);
                log.Debug(
                    "[PublishBufferAsync] EnsureCreateConnectionPool is false, appid:{0},code:{1},message mid:{2} add publish log,retry message ok",
                    messageContext.appid,
                    messageContext.code, messageContext.messageid);
                return;
            }
            using (var monitor = new MethodMonitor(null))
            {
                await pubBuffer.PublishMessageAsync(new PublishMessageContextAsync
                {
                    context = messageContext,
                    declare_op = exchange,
                    channel = () => pool.CreateChannelAsync(messageContext.appid),
                    publishproxy = pubAsync
                }).ConfigureAwait(false);
                log.Debug(
                    "[PublishBufferAsync] message send to rabbitmq ok,appId:{0},code:{1},msgId:{2},execute:{3:N0} ms",
                    messageContext.appid,
                    messageContext.code, messageContext.messageid, monitor.GetRunTime2);
            }
        }

        public async Task PublishAsync(IEnumerable<PublishMessageContext> messageContex)
        {           
            messageContex.EachAction(async m => await PublishAsync(m));
            return;
        }

        public TMessage PullMessage<TMessage>(string appId, string code)
        {
            CheckBusApplicationIsRun();
            var msgCfg = MQMainConfigurationManager.Builder.GetConfiguration(appId, code);
            EnsureCreateConnectionPool(appId);
            var channel = pool.CreateChannel(appId);
            exchange.EnsureQueueDeclare(channel, appId, code);
            var result = channel.BasicGet(msgCfg.QueueCfg.QueueName, msgCfg.ConsumeCfg.IsAutoAcknowledge.Value);
            if (result == null)
            {
                log.Debug("appid {0},msgid {1} PullMessage，message is null", appId, code);
                return default(TMessage);
            }
            //channel.BasicNack(result.DeliveryTag, false, true);
            if (!msgCfg.ConsumeCfg.IsAutoAcknowledge.Value)
                channel.BasicAck(result.DeliveryTag, false);
            return result.Body.JSONDeserializeFromByteArray<TMessage>();
        }

        public void PullMessage<TMessage>(string appId, string code, IMessageHandler<TMessage> handle)
        {
            var msg = PullMessage<TMessage>(appId, code);
            if (msg != null)
            {
                var context = new MessageHandleContext<TMessage>(msg, false);
                handle.Handle(context);
            }
        }

        public uint MessageCount(string appId, string code)
        {
            var channel = pool.CreateChannel(appId);
            var queueCfg = MQMainConfigurationManager.Builder.GetConfiguration(appId, code);
            return channel.QueueDeclarePassive(queueCfg.QueueCfg.QueueName).MessageCount;
        }

        public bool RemoveCacheExchang(string appid, string code)
        {
            return exchange.RemoveExchangeCache(appid, code);
        }

        public void StartBusApplication()
        {
            //TODO:
            if (status == BusApplicationStatus.Runing || status == BusApplicationStatus.Starting) return;
            status = BusApplicationStatus.Starting;
            log.Debug("message bus appservice being start...");
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;           
            using (var mm = new MethodMonitor(log, descript: "StartBusApplication"))
            {
                MQMainConfigurationManager.Builder.RegisterConnectionConfigurationUpdate(UpdateConnectionPool);
                //初始化连接池
                initConnectionPool();
                //设置程序状态
                MessageAppService_TimerBatch.Instance.StartJob();
                //_PersistentMessageToMongodb.StartJob();
                MQMainConfigurationManager.Builder.Start();
                status = BusApplicationStatus.Runing;
                log.Debug("message bus appservice start ok, Execute {0:N0} ms", mm.GetRunTime2);
            }
        }

        public void PublishToDb(PublishMessageContext messageContext)
        {
            log.Debug("[PublishToDb] receive from client message,appid:{0},code:{1},mid:{2},cip:{3},body:{4}",
                messageContext.appid, messageContext.code, messageContext.messageid, messageContext.ip,
                messageContext.body);
            var msgCfg = MQMainConfigurationManager.Builder.GetConfiguration(messageContext.appid, messageContext.code);
            if (!msgCfg.Enable)
            {
                log.Debug("appId:{0}，code:{1} disable", messageContext.appid, messageContext.code);
                return;
            }
            MessageStore.AddMessagePublishLog(messageContext);
            MessageStore.AddRetryMessage(messageContext, PublishToDb_Description);
            log.Debug(
                "[PublishToDb] appid:{0},code:{1},message mid:{2} add publish log,retry message ok",
                messageContext.appid,
                messageContext.code, messageContext.messageid);
        }

        public void PublishBatchToDb(IEnumerable<PublishMessageContext> messageContext, string appId, string code, string ip)
        {
            log.Debug("[PublishBatchToDb] receive from client message,appid:{0},code:{1},ip:{2},cip:{3},body count:{4}",
                appId, code, ip,
                messageContext.Count());
            var msgCfg = MQMainConfigurationManager.Builder.GetConfiguration(appId, code);
            if (!msgCfg.Enable)
            {
                log.Debug("[PublishBatchToDb] appId:{0}，code:{1} disable", appId, code);
                return;
            }            
            MessageStore.AddMessageBatch(messageContext, appId, code, ip);
            MessageStore.AddRetryMessageBatch(messageContext, appId, code);

            log.Debug("[PublishBatchToDb] AddMessageBatch,AddRetryMessageBatch success,appid:{0},code:{1}", appId, code);
        }

        void CurrentDomain_FirstChanceException(object sender, FirstChanceExceptionEventArgs e)
        {

        }

        void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();
            log.Error("UnobservedTaskExceptionEventArgs error {0},{1}", sender, e.Exception.ToString());
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            log.Error("CurrentDomain_UnhandledException error,{0},{1}", sender, e.ExceptionObject.ToString());
        }

        public void StopBusApplication()
        {
            if (status == BusApplicationStatus.Stop || status == BusApplicationStatus.Stoping) return;
            status = BusApplicationStatus.Stoping;
            //TODO:执行系统清理
            log.Debug("message bus appservice begin stop...");
            using (var mm = new MethodMonitor(log, descript: "StopBusApplication"))
            {                
                //停止配置维护
                MQMainConfigurationManager.Builder.Stop();
                //清理链接池
                pool.Clear();
                MessageAppService_TimerBatch.Instance.StopJob();               
                status = BusApplicationStatus.Stop;
                //pubBuffer.Stop();
                log.Debug("message bus appservice stop ok...");
            }
        }

        public MQConnectionPoolManager Pool
        {
            get { return pool; }
        }

        public BusApplicationStatus BusStatus
        {
            get { return status; }
        }

        //处理通知
        public Task Notify(string appId, IModel channel)
        {
            Action action = () => { };
            return action.ExecuteSynchronously();
        }

        //确保创建连接池 （应用于，动态增加配置的情况）
        private bool EnsureCreateConnectionPool(string appid)
        {
            //如果应用没有建立连接则重新创建链接
            if (!pool.CheckAppIdExists(appid))
            {
                pool.InitPool(appid, MQMainConfigurationManager.Builder.GetConfiguration(appid).ConnCfg.ConnectionString,
                    this);
            }
            if (pool.CheckAppIdExists(appid))
            {
                return true;
            }
            log.Error("[EnsureCreateConnectionPool] appid {0} init pool fail.", appid);
            return false;
        }

        //检查BusApp appId状态
        private void CheckBusApplicationIsRun()
        {
            if (status != BusApplicationStatus.Runing)
                throw new Exception<MQException>("MQBUS not runing，please call StartBusApplication");
        }

        private bool CheckBusApplicationIsRuning()
        {
            var isRuning = status == BusApplicationStatus.Runing;
            if (!isRuning)
            {
                log.Error("![CheckBusApplicationIsRuning] bus application not running, status {0}！！", status);
            }
            return isRuning;
        }

        //更新链接
        private void UpdateConnectionPool(IEnumerable<_UpdateConnectionInfo> callBack)
        {
            YmtSystemAssert.AssertArgumentNotNull(callBack, "未获取到MQ配置");
            callBack.EachAction(item => pool.ReBuilderPool(item, this));
        }

        //初始化链接
        private void initConnectionPool()
        {
            //初始化链接(测试)
            var cfg = MQMainConfigurationManager.Builder.GetConfiguration();
            YmtSystemAssert.AssertArgumentNotNull(cfg, "未获取到MQ配置");
            cfg.EachAction(item => pool.InitPool(item.Value.AppId, item.Value.ConnCfg.ConnectionString, this));
        }
    }
}
