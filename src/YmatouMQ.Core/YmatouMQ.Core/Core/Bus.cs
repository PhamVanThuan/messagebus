using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using YmatouMQNet4.Configuration;
using YmatouMQNet4.Connection;
using YmatouMQNet4.Core.Publish;
using YmatouMQNet4.Utils;
using YmatouMQNet4.Extensions;
using YmatouMQNet4.Extensions.Serialization;
using YmatouMQNet4.Extensions._Task;
using RabbitMQ.Client;
using YmatouMQNet4._Persistent;
using YmatouMQNet4.Logs;

namespace YmatouMQNet4.Core
{
    internal class Bus : IBus
    {
        private static readonly Lazy<Bus> lazy = new Lazy<Bus>(() => new Bus(), true);
        private readonly ILog log = LogFactory.GetLogger(LogEngineType.RealtimelWriteFile, "YmatouMQ.Core.Bus");
        private readonly MQConnectionPoolManager pool = new MQConnectionPoolManager();       
        private readonly MQDeclareCache exchange = new MQDeclareCache();
        private readonly PublishMessageBase pubSync = PublishMessageFactory.Context(PublishMessageType.Sync);
        private readonly PublishMessageBase pubAsync = PublishMessageFactory.Context(PublishMessageType.Async);
        private readonly PublishBufferActionAsync pubBuffer;
        private static BusApplicationStatus status = BusApplicationStatus.NotStart;

        /// <summary>
        /// 构建BUS
        /// </summary>
        public static Bus Builder { get { return lazy.Value; } }
        private Bus()
        {
            pubBuffer = new PublishBufferActionAsync(pubAsync);
        }

        public void Publish(PublishMessageContext messageContext)
        {
            var msgCfg = MQMainConfigurationManager.Builder.GetConfiguration(messageContext.appid, messageContext.code);
            if (!msgCfg.Enable)
            {
                log.Debug("应用{0}，业务{1}未开启发布订阅功能", messageContext.appid, messageContext.code);
                return;
            }
            CheckBusApplicationIsRun();
            var _channel = pool.CreateChannel(messageContext.appid);
            exchange.EnsureExchangeDeclare(_channel, messageContext.appid, messageContext.code);
            pubSync.PublishMessage(new PublishMessageContextSync { context = messageContext, channel = _channel });
            log.Debug("appId {0} (sync) send message msgId {1} send message ok", messageContext.appid, messageContext.code);
        }
        public void Publish(IEnumerable<PublishMessageContext> messageContext)
        {
            messageContext.EachAction(m => Publish(m));
        }
        public Task PublishAsync(PublishMessageContext messageContex)
        {
            var msgCfg = MQMainConfigurationManager.Builder.GetConfiguration(messageContex.appid, messageContex.code);
            if (!msgCfg.Enable)
            {
                log.Debug("应用{0}，业务{1}未开启发布功能", messageContex.appid, messageContex.code);
                Func<ReturnNull> r = () => new ReturnNull();
                return r.ExecuteSynchronously();
            }
            CheckBusApplicationIsRun();

            exchange.EnsureExchangeDeclare(pool.CreateChannel(messageContex.appid), messageContex.appid, messageContex.code);
            return pubAsync.PublishMessageAsync(new PublishMessageContextAsync { context = messageContex, channel = async () => await pool.CreateChannelAsync(messageContex.appid) });

        }
        public async Task PublishBufferAsync(PublishMessageContext messageContex)
        {
            log.Debug("接收到客户端 ip {0} ,appid {1},code {2} 消息 {3}", messageContex.ip, messageContex.appid, messageContex.code, messageContex.body);
            exchange.EnsureExchangeDeclare(pool.CreateChannel(messageContex.appid), messageContex.appid, messageContex.code);
            await pubBuffer.PublishMessageAsync(new PublishMessageContextAsync { context = messageContex, channel = () => pool.CreateChannelAsync(messageContex.appid) }).ConfigureAwait(false);
        }
        public IEnumerable<Task> PublishAsync(IEnumerable<PublishMessageContext> messageContex)
        {
            var list = new List<Task>();
            messageContex.EachAction(m => list.Add(PublishAsync(m)));
            return list;
        }
        public TMessage PullMessage<TMessage>(string appId, string code)
        {
            CheckBusApplicationIsRun();
            var msgCfg = MQMainConfigurationManager.Builder.GetConfiguration(appId, code);
            var channel = pool.CreateChannel(appId);
            exchange.EnsureQueueDeclare(channel, appId, code);
            var result = channel.BasicGet(msgCfg.QueueCfg.QueueName, msgCfg.ConsumeCfg.IsAutoAcknowledge.Value);
            if (result == null)
            {
                log.Debug("配置 appid {0},msgid {1} PullMessage 操作，未获取到消息", appId, code);
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
        public void StartBusApplication()
        {
            //TODO:
            if (status == BusApplicationStatus.Runing || status == BusApplicationStatus.Starting) return;
            status = BusApplicationStatus.Starting;
            log.Debug("MQBUS应用开始启动...");
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            //AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
            using (var mm = new MethodMonitor(log, descript: "StartBusApplication"))
            {
                MQMainConfigurationManager.Builder.RegisterConnectionConfigurationUpdate(UpdateConnectionPool);
                MQMainConfigurationManager.Builder.Start();
                //初始化连接池
                initConnectionPool();
                //设置程序状态 
                _PersistentMessageToMongodb.StartJob();
                status = BusApplicationStatus.Runing;
                log.Debug("MQBUS应始启动完成...");
            }
        }

        void CurrentDomain_FirstChanceException(object sender, FirstChanceExceptionEventArgs e)
        {
            //log.Error("appdomain error {0},{1}", sender, e.Exception);
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
            log.Debug("MQBUS应用开始停止...");
            using (var mm = new MethodMonitor(log, descript: "StopBusApplication"))
            {
                //停止所有订阅
                //StopSubscribe();
                //停止配置维护
                MQMainConfigurationManager.Builder.Stop();
                //清理链接池
                pool.Clear();
                _PersistentMessageToMongodb.StopJob();
                //LocalLogHelp.Close();
                status = BusApplicationStatus.Stop;
                pubBuffer.Stop();
                log.Debug("MQBUS应用停止完成...");
            }
        }
        public MQConnectionPoolManager Pool { get { return pool; } }
        public BusApplicationStatus BusStatus { get { return status; } }
        //处理通知
        public Task Notify(string appId, IModel channel)
        {
            var t1 = Task.Factory.StartNew(() =>
            {
                var exMsg = pubSync.GetExceptionMessageContext(appId);
                if (exMsg != null)
                {
                    log.Debug("同步发送{0}获取到{1}个异常消息", appId, exMsg.Count);
                    foreach (var item in exMsg.GetConsumingEnumerable())
                        PublishAsync(new PublishMessageContext { body = item.body, appid = item.appId, code = item.code, messageid = item.msgId, ip = null });
                }
            });
            var t2 = Task.Factory.StartNew(() =>
            {
                var exMsg2 = pubAsync.GetExceptionMessageContext(appId);
                if (exMsg2 != null)
                {
                    log.Debug("异步发送{0}获取到{1}个异常消息", appId, exMsg2.Count);
                    foreach (var item in exMsg2.GetConsumingEnumerable())
                        PublishAsync(new PublishMessageContext { body = item.body, appid = item.appId, code = item.code, messageid = item.msgId, ip = null });
                }
            });
            return Task.Factory.ContinueWhenAll(new Task[] { t1, t2 }, a => { });
        }
        //检查BusApp 应用状态
        private void CheckBusApplicationIsRun()
        {
            if (status != BusApplicationStatus.Runing)
                throw new Exception<MQException>("MQBUS 应用程序未启动，请调用 StartBusApplication 方法启动BUSApplication");
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
