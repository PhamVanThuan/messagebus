using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using RabbitMQ.Client.MessagePatterns;
using YmatouMQNet4.Configuration;
using YmatouMQ.Connection;
using YmatouMQ.Common.Utils;
using YmatouMQ.Common.Extensions;
using YmatouMQ.Common.Extensions._Task;
//using YmatouMQNet4._Persistent;
//using YmatouMQNet4.Dto;
using YmatouMQ.Common.Extensions.Serialization;
using System.Collections.Concurrent;
using System.Configuration;
using Ymatou.PerfMonitorClient;
using _MethodMonitor = Ymatou.PerfMonitorClient.MethodMonitor;
using YmatouMQSubscribe;
using YmatouMQ.Common;
using YmatouMQ.Log;
using YmatouMQ.Common.MessageHandleContract;
using YmatouMQNet4.Utils;

namespace YmatouMQ.Subscribe
{
    /// <summary>
    /// 订阅消息
    /// </summary>
    [Serializable]
    public class _Subscribe : MarshalByRefObject, ISubscribe
    {
        private readonly ILog log = LogFactory.GetLogger(LogEngineType.RealtimelWriteFile, "YmatouMQ.Core._Subscribe");
        private readonly ConcurrentDictionary<string, QueueingBasicConsumer> subcache = new ConcurrentDictionary<string, QueueingBasicConsumer>();
        private readonly ThreadLocal<QueueingBasicConsumer> localcConsumer = new ThreadLocal<QueueingBasicConsumer>();
        private readonly List<Task> th = new List<Task>();
        private readonly List<QueueingBasicConsumer> listConsumer = new List<QueueingBasicConsumer>();
        private readonly IMessageHandler<byte[]> handle;
        private readonly MessageConfiguration cfg;
        private readonly MQMainConfiguration mqMainCfg;
        private readonly MQConnectionPoolManager pool;
        private readonly string appId;
        private readonly string code;
        //private readonly SemaphoreSlim ss;
        private const int queueTimeout = 1000;
        private readonly CancellationTokenSource cts;
        public _Subscribe(string appId, string code)
        {
            this.handle = CreateEventHandler();
            this.pool = new MQConnectionPoolManager();
            this.appId = appId;
            this.code = code;
            this.mqMainCfg = MQMainConfigurationManager.Builder.GetConfiguration(appId);
            this.cfg = MQMainConfigurationManager.Builder.GetConfiguration(appId, code);
            this.cts = new CancellationTokenSource();
            //this.ss = new SemaphoreSlim((int)cfg.ConsumeCfg.MaxThreadCount.Value, (int)cfg.ConsumeCfg.MaxThreadCount.Value);           
            //ThreadPool.SetMaxThreads((int)cfg.ConsumeCfg.MaxThreadCount.Value, (int)cfg.ConsumeCfg.MaxThreadCount.Value * 6);
        }
        /// <summary>
        /// 启动订阅功能
        /// </summary>
        public void Start()
        {
            if (!cfg.Enable)
            {
                log.Warning("appId {0},code {1} 未开启消息订阅", mqMainCfg.AppId, cfg.Code);
                return;
            }
            //
            MQMainConfigurationManager.Builder.Start();
            MQMainConfigurationManager.Builder.RegisterConnectionConfigurationUpdate(UpdateConnectionPool);
            //
            pool.InitPool(mqMainCfg.AppId, code, mqMainCfg.ConnCfg.ConnectionString, null, true);
            //
            if (cfg.ConsumeCfg.UseMultipleThread.Value)
            {
                _WorkAsync();
            }
            else
            {
                _WorkSync();
            }
            log.Debug("appid {0},code {1} 启动完成", appId, code);
        }

        /// <summary>
        /// 停止订阅功能
        /// </summary>
        public void Stop()
        {
            ThreadStop();
            listConsumer.EachAction(e => e.Model.TryColseChannel());
            TaskStop();
            pool.Clear();
            log.Debug("{0} 订阅消息停止成功", mqMainCfg.AppId);
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }
        private void SubscribeDeclare(IModel channel)
        {
            channel.ExchangeDeclare(cfg.ExchangeCfg.ExchangeName, cfg.ExchangeCfg._ExchangeType.ToString().ToLower(), cfg.ExchangeCfg.Durable.Value, cfg.ExchangeCfg.IsExchangeAutoDelete.Value, cfg.ExchangeCfg.Arguments);
            //声明队列
            channel.QueueDeclare(cfg.QueueCfg.QueueName, cfg.QueueCfg.IsDurable.Value, cfg.QueueCfg.IsQueueExclusive.Value, cfg.QueueCfg.IsAutoDelete.Value, cfg.QueueCfg.Args);
            //绑定到交换机           
            channel.QueueBind(cfg.QueueCfg.QueueName, cfg.ExchangeCfg.ExchangeName, cfg.ConsumeCfg.RoutingKey, cfg.QueueCfg.HeadArgs);
            //声明消费者           
            //声明流量阀值 test                
            // channel.BasicQos(0, 50, false);
            cfg.ConsumeCfg.PrefetchCount.NotNullAction(v => channel.BasicQos(0, v, false));
        }

        private Task SubscribeDeclareAsync(IModel channel)
        {
            Action fn = () =>
            {
                channel.ExchangeDeclare(cfg.ExchangeCfg.ExchangeName, cfg.ExchangeCfg._ExchangeType.ToString().ToLower(), cfg.ExchangeCfg.Durable.Value, cfg.ExchangeCfg.IsExchangeAutoDelete.Value, cfg.ExchangeCfg.Arguments);
                //声明队列
                channel.QueueDeclare(cfg.QueueCfg.QueueName, cfg.QueueCfg.IsDurable.Value, cfg.QueueCfg.IsQueueExclusive.Value, cfg.QueueCfg.IsAutoDelete.Value, cfg.QueueCfg.Args);
                //绑定到交换机
                channel.QueueBind(cfg.QueueCfg.QueueName, cfg.ExchangeCfg.ExchangeName, string.IsNullOrEmpty(cfg.ConsumeCfg.RoutingKey) ? "*.*" : cfg.ConsumeCfg.RoutingKey, cfg.QueueCfg.HeadArgs);
                //声明消费者              
                //声明流量阀值(客户端只接受小于等于cfg.ConsumeCfg.PrefetchCount.Value 的消息数量）
                cfg.ConsumeCfg.PrefetchCount.NotNullAction(v => channel.BasicQos(0, v, false));
            };
            return fn.ExecuteSynchronously();
        }
        private bool isexception = false;
        private void _WorkSync()
        {
            log.Debug("appid {0} code {1} max thread count {2}", appId, code, cfg.ConsumeCfg.MaxThreadCount.Value);
            for (var i = 0; i < cfg.ConsumeCfg.MaxThreadCount.Value; i++)
            {
                th.Add(Task.Factory.StartNew(() =>
                {

                    var channel = pool.CreateChannel(appId, code);
                    SubscribeDeclare(channel);
                    var consumer = new QueueingBasicConsumer(channel);
                    channel.BasicConsume(cfg.QueueCfg.QueueName, cfg.ConsumeCfg.IsAutoAcknowledge.Value, consumer);
                    listConsumer.Add(consumer);
                    while (!cts.Token.IsCancellationRequested)
                    {
                        if (!cfg.Enable)
                        {
                            log.Warning("appId {0},code {1} 已关闭消息订阅", appId, code);
                            break;
                        }
                        try
                        {
                            var eventArgs = consumer.Queue.Dequeue();
                            if (eventArgs != null)
                            {
                                using (_MethodMonitor.New("Subscribe_HandleSync"))
                                {
                                    HandleSync(handle, eventArgs, consumer.Model);
                                }
                            }
                            if (isexception)
                            {
                                log.Debug("是否为空 {0}", eventArgs == null);
                            }
                        }
                        catch (EndOfStreamException ex)
                        {
                            log.Error("订阅消息异常_WorkSync (EndOfStreamException)  cancell {0} ,{1}", cts.Token.IsCancellationRequested, ex.ToString());
                            break;
                        }
                        catch (AlreadyClosedException ex)
                        {
                            log.Error("订阅消息异常_WorkSync (AlreadyClosedException)  cancell {0} ,{1}", cts.Token.IsCancellationRequested, ex.ToString());
                        }
                        catch (Exception ex)
                        {
                            isexception = true;
                            log.Error("订阅消息异常_WorkSync (Exception) cancell {0} ,{1}", cts.Token.IsCancellationRequested, ex.ToString());
                        }
                    }
                }, cts.Token, TaskCreationOptions.LongRunning, new ThreadPerTaskScheduler()));
            }

        }
        private void _WorkAsync()
        {
            //最大消费者数量（线程数)
            for (var t = 0; t < cfg.ConsumeCfg.MaxThreadCount.Value; t++)
            {
                th.Add(Task.Factory.StartNew(async () =>
                {
                    var consumer = EnsureSubscribeDeclare(await pool.CreateChannelAsync(appId, code));
                    listConsumer.Add(consumer);
                    while (!cts.Token.IsCancellationRequested)
                    {
                        if (!cfg.Enable)
                        {
                            log.Warning("appId {0},code {1} 已关闭消息订阅", appId, code);
                            break;
                        }
                        //订阅消息声明&声明消费者实例
                        try
                        {
                            BasicDeliverEventArgs eventArgs;
                            consumer.Queue.Dequeue(3000, out eventArgs);
                            if (eventArgs != null)
                            {
                                using (_MethodMonitor.New("Subscribe_HandleASync"))
                                {
                                    await HandleASync(handle, eventArgs, consumer.Model).ContinueWith(e => e.Exception.Handle(log, "异步处理消息异常appid {0},code {1}".Fomart(appId, code)), TaskContinuationOptions.OnlyOnFaulted).ConfigureAwait(false);
                                }
                            }
                        }
                        catch (EndOfStreamException ex)
                        {
                            var channel = pool.CreateChannelAsync(appId, code);
                            log.Error("订阅消息异常(EndOfStreamException) channel status {0}, {1}", channel.Result.IsClosed, ex.ToString());
                            break;
                        }
                        catch (AlreadyClosedException ex)
                        {
                            log.Error("订阅消息异常(AlreadyClosedException) cancell {0} ,{1}", cts.Token.IsCancellationRequested, ex.ToString());
                        }
                        catch (Exception ex)
                        {
                            log.Error("订阅消息异常(Exception) cancell {0} ,{1}", cts.Token.IsCancellationRequested, ex.ToString());
                        }
                    }
                }, cts.Token, TaskCreationOptions.LongRunning, new ThreadPerTaskScheduler()));
            }
            log.Debug("appid {0},code {1} 创建消费者完成", appId, code);
        }

        private QueueingBasicConsumer EnsureSubscribeDeclare(IModel channel)
        {
            var key = "{0}_{1}_{2}".Fomart(appId, code, Thread.CurrentThread.ManagedThreadId);
            return subcache.GetOrAdd(key, k =>
            {
                SubscribeDeclareAsync(channel).WithHandleException(" EnsureSubscribeDeclare {0} error", key);
                var consumer = new QueueingBasicConsumer(channel);
                channel.BasicConsume(cfg.QueueCfg.QueueName, cfg.ConsumeCfg.IsAutoAcknowledge.Value, consumer);
                return consumer;
            });
        }
        private void HandleSync(IMessageHandler<byte[]> handle, BasicDeliverEventArgs eventArgs, IModel channel)
        {
            log.Debug("客户端订阅消息开始同处理 appId {0},code {1},msgBody {2} byte", mqMainCfg.AppId, cfg.Code, eventArgs.Body.Length);
            var result = ActionRetryHelp.Retry(
                         () => handle.Handle(new MessageHandleContext<byte[]>(eventArgs.Body
                             , eventArgs.Redelivered
                             , cfg.ConsumeCfg.CallbackUrl
                             , cfg.ConsumeCfg.CallbackMethodType
                             , cfg.ConsumeCfg.CallbackTimeOut.Value
                             , appId
                             , code
                             , null))
                         , cfg.ConsumeCfg.RetryCount.Value
                         , TimeSpan.FromMilliseconds(cfg.ConsumeCfg.RetryMillisecond.Value)
                         , () => { }
                         , err => log.Debug("客户端消息处理出现为处理的异常,appId,{0},code,{1}, erroMsg,{2}", mqMainCfg.AppId, cfg.Code, err.ToString())
                         , ResponseData<ResponseNull>.CreateFailTask(ResponseNull._Null, errorCode: 500));
            if (result.Result.Code == 200)
            {
                var handleSuccessResult = ContinueWith_HandleClientResponseSuccess(channel, eventArgs)
                    .ContinueWith(e => e.Exception.Handle(log, "ContinueWith_HandleClientResponseSuccess error appid {0} {1}", appId, code)
                    , TaskContinuationOptions.OnlyOnFaulted);
            }
            else
            {
                var handleFailResult = ContinueWith_HandleClientResponseFail(channel, eventArgs, result.Result.Code)
                    .ContinueWith(e => e.Exception.Handle(log, "ContinueWith_HandleClientResponseFail error appid {0} {1}", appId, code)
                    , TaskContinuationOptions.OnlyOnFaulted);
            }
        }
        private async Task HandleASync(IMessageHandler<byte[]> handle, BasicDeliverEventArgs eventArgs, IModel channdel)
        {
            log.Debug("客户端订阅消息开始异步处理 appId {0},code {1},msgBody {2} byte", mqMainCfg.AppId, cfg.Code, eventArgs.Body.Length);
            var result = ResponseData<ResponseNull>.CreateFail(ResponseNull._Null, 500);
            var handleTask = await ActionRetryHelp.Retry(
                                                    () => handle.Handle(new MessageHandleContext<byte[]>(eventArgs.Body
                                                        , eventArgs.Redelivered
                                                        , cfg.ConsumeCfg.CallbackUrl
                                                        , cfg.ConsumeCfg.CallbackMethodType
                                                        , cfg.ConsumeCfg.CallbackTimeOut.Value
                                                        , appId
                                                        , code
                                                        , null))
                                                    , cfg.ConsumeCfg.RetryCount.Value
                                                    , TimeSpan.FromMilliseconds(cfg.ConsumeCfg.RetryMillisecond.Value)
                                                    , () => { }
                                                    , err => log.Debug("客户端消息处理出现为处理的异常,appId,{0},code,{1}, erroMsg,{2}", mqMainCfg.AppId, cfg.Code, err.ToString())
                                                    , null).ConfigureAwait(false);
            if (result.Code == 200)
            {
                await ContinueWith_HandleClientResponseSuccess(channdel, eventArgs)
                    .ContinueWith(e => e.Exception.Handle(log, "(HandleASync)ContinueWith_HandleClientResponseSuccess error appid {0} {1}", appId, code)
                    , TaskContinuationOptions.OnlyOnFaulted).ConfigureAwait(false);
            }
            else
            {
                await ContinueWith_HandleClientResponseFail(channdel, eventArgs, result.Code)
                    .ContinueWith(e => e.Exception.Handle(log, "(HandleASync) ContinueWith_HandleClientResponseFail error appid {0} {1}", appId, code)
                    , TaskContinuationOptions.OnlyOnFaulted).ConfigureAwait(false);
            }

        }
        private async Task ContinueWith_HandleClientResponseFail(IModel channel, BasicDeliverEventArgs eventArgs, int resultCode)
        {
            try
            {
                //客户端消息处理失败，根据配置发送ACK
                if (cfg.ConsumeCfg.HandleFailAcknowledge.Value)
                {
                    Action ackAction = () => channel.BasicAck(eventArgs.DeliveryTag, false);
                    await ackAction.ExecuteSynchronously();
                }
                if (resultCode == 501 || cfg.ConsumeCfg.HandleFailPersistentStore.Value)
                {
                    //消息持久化到mongodb
                    object _mid;
                    if (eventArgs.BasicProperties.Headers != null && eventArgs.BasicProperties.Headers.TryGetValue("msgid", out _mid))
                    {
                        var __mid = ((byte[])_mid).GetString();
                        //消息处理失败是否发送消息到mongodb
                        if (!cfg.ConsumeCfg.HandleFailMessageToMongo.Value)
                            await _PersistentMessageToLocal.MongoStore(eventArgs.Body.JSONDeserializeFromByteArray<string>(), this.appId, this.cfg.Code, __mid, Status.HandleException);
                        //发送处理失败通知
                        await _PersistentMessageToLocal.SendNotice(mqMainCfg.AppId, cfg.Code, __mid, Status.HandleException);
                    }
                    else
                    {
                        log.Debug("应用{0},code{1},开启消息处理失败持久化,但无法获取消息ID", mqMainCfg.AppId, cfg.Code);
                    }
                }
            }
            catch (AggregateException ex)
            {
                ex.Handle(log, "ContinueWith_HandleClientResponseFail (AggregateException) error,appid {0},code {1}".Fomart(appId, code));
            }
            catch (Exception ex)
            {
                ex.Handle(log, "ContinueWith_HandleClientResponseFail (Exception) error,appid {0},code {1}".Fomart(appId, code));
            }
        }
        private async Task ContinueWith_HandleClientResponseSuccess(IModel sub, BasicDeliverEventArgs eventArgs)
        {
            //如果没有设置自动ACK，则在客户端消息处理结束后ACK
            try
            {
                if (!cfg.ConsumeCfg.IsAutoAcknowledge.Value)
                {
                    Action ackAction = () => sub.BasicAck(eventArgs.DeliveryTag, false);
                    await ackAction.ExecuteSynchronously().ConfigureAwait(false);
                }
                if (cfg.ConsumeCfg.HandleSuccessSendNotice.Value)
                {
                    object _mid;
                    if (eventArgs.BasicProperties.Headers != null && eventArgs.BasicProperties.Headers.TryGetValue("msgid", out _mid))
                    {
                        var __mid = ((byte[])_mid).GetString();
                        await _PersistentMessageToLocal.SendNotice(mqMainCfg.AppId, cfg.Code, __mid, Status.HandleSuccess).ConfigureAwait(false);
                    }
                    else
                    {
                        log.Warning("未获取消息ID，无法发送消息处理状态通知 appid {0},code {1}", appId, code);
                    }
                }
            }
            catch (AggregateException ex)
            {
                ex.Handle(log, "ContinueWith_HandleClientResponseSuccess (AggregateException) error,appid {0},code {1}".Fomart(appId, code));
            }
            catch (Exception ex)
            {
                ex.Handle(log, "ContinueWith_HandleClientResponseSuccess (Exception) error,appid {0},code {1}".Fomart(appId, code));
            }
        }
        private void ThreadStop()
        {
            try
            {
                cts.Cancel();
            }
            catch
            {

            }
        }
        private void TaskStop()
        {
            try
            {
                var cts = new CancellationTokenSource(3000);
                var token = cts.Token;
                Task.WaitAll(th.ToArray(), token);
            }
            catch
            {

            }
        }
        private void UpdateConnectionPool(IEnumerable<_UpdateConnectionInfo> callBack)
        {
            YmtSystemAssert.AssertArgumentNotNull(callBack, "未获取到MQ配置");
            callBack.EachAction(item => pool.ReBuilderPool(item, null));
        }
        private static IMessageHandler<byte[]> CreateEventHandler()
        {
            var assemblyName = ConfigurationManager.AppSettings["handleAssemblyName"].Split(new char[] { ',' });
            var handler = Activator.CreateInstance(assemblyName[1], assemblyName[0]).Unwrap() as IMessageHandler<byte[]>;
            YmtSystemAssert.AssertArgumentNotNull(handler, "IMessageHandler 不能为空");
            return handler;
        }

        void channel_ModelShutdown(object sender, ShutdownEventArgs e)
        {
            if (e.Initiator != ShutdownInitiator.Application)
            {
                log.Error("客户端订阅 appId:{0} 获取消息, msgId:{1}，消息通道异常关闭:{1} ,原因:{3}", mqMainCfg.AppId
               , cfg.Code, e.Initiator, e.ReplyText);
            }
            else
            {
                log.Debug("客户端订阅 appId:{0} 获取消息, msgId:{1}，消息通道正常关闭:{1} ,原因:{3}", mqMainCfg.AppId
                    , cfg.Code, e.Initiator, e.ReplyText);
            }
        }
    }
}
