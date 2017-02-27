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
using System.Diagnostics;
using System.Net;
using YmatouMQ.ConfigurationSync;
using YmatouMQMessageMongodb.AppService;
using YmatouMQMessageMongodb.Domain.Module;

namespace YmatouMQ.Subscribe
{
    /// <summary>
    /// 订阅消息
    /// </summary>    
    public class _Subscribe :  ISubscribe, IConnRecoveryNotify,IConnShutdownNotify
    {
        private readonly ILog log = LogFactory.GetLogger(LogEngineType.RealtimelWriteFile, "YmatouMQ.Core._Subscribe");
        private readonly ConcurrentDictionary<string, QueueingBasicConsumer> subcache = new ConcurrentDictionary<string, QueueingBasicConsumer>();
        private readonly AutoResetEvent @event = new AutoResetEvent(false);
        private readonly List<Task> th = new List<Task>();        
        private readonly IMessageHandler<byte[]> handle;
        private readonly MQConnectionPoolManager pool;
        private readonly string appId;
        private readonly string code;       
        private readonly CancellationTokenSource cts;
        private readonly object thObj = new object();
        private const int WaitQueueMillisecondsTimeOut = 3000;
        private const int waitAvailableThreadMillisecondsTimeOut = 1000;
        private int current_Concurrent = 0; //记录当前并发  
        private SemaphoreSlim semaphore;
        private MessageConfiguration cfg;
        private MQMainConfiguration mqMainCfg;
        private QueueingBasicConsumer consumer;
        private EventingBasicConsumer consumerEvent;
        private DateTime lastUpdateHealthTime;
        private Thread healtThread;
        private static bool IsRuningHealtThread;
        private static volatile bool healtIsOk = true;
       
        public _Subscribe(string appId, string code)
        {
            this.appId = appId;
            this.code = code;
            ServicePointManager.DefaultConnectionLimit = Int16.MaxValue;
            this.InitLogger();
            this.handle = CreateEventHandler();
            this.pool = new MQConnectionPoolManager();
            this.cts = new CancellationTokenSource();
        }
        /// <summary>
        /// 启动订阅功能
        /// </summary>
        public void Start()
        {
            //thread
            int workth, ioth;
            ThreadPool.GetMaxThreads(out workth, out ioth);
            this.log.Debug("begin start subscribe max work thread {0},io thread {1},appid {2} code {3}".Fomart(workth, ioth, appId, code));
            MQMainConfigurationManager.Builder.RegisterConnectionConfigurationUpdate(UpdateConnectionPool);
            //get cfg
            this.mqMainCfg = MQMainConfigurationManager.Builder.GetConfiguration(appId);
            this.cfg = MQMainConfigurationManager.Builder.GetConfiguration(appId, code);
            //set semaphore
            this.semaphore = new SemaphoreSlim(cfg.ConsumeCfg.MaxThreadCount.ToInt32(128), cfg.ConsumeCfg.MaxThreadCount.ToInt32(128));
            this.current_Concurrent = (int)cfg.ConsumeCfg.MaxThreadCount.Value;
            //init pool
            this.pool.InitPool(mqMainCfg.AppId,code, mqMainCfg.ConnCfg.ConnectionString, this,true,this);
            //configuration service
            this.StartConfiguration();
            //InitHandle
            this.handle.InitHandle(this.appId, this.code);
            //SubscribeWork
            //this.SubscribeWork();
            this.SubscribeWorkEvent();
            this.StartHealtTask();
            //thread           
            int availableThreads, ioThread;
            ThreadPool.GetAvailableThreads(out availableThreads, out ioThread);
            this.log.Debug("subscribe start ok ,appid {0} code {1},max concurrent {2},available Threads:{3}", this.appId, this.code, current_Concurrent, availableThreads);
        }

        /// <summary>
        /// 停止订阅功能
        /// </summary>
        public void Stop()
        {
            log.Debug("begin stop appid {0} code {1}", this.appId, this.code);
            var watch = Stopwatch.StartNew();
            this.ThreadStop();
            var taskstop = TaskStop();
            if (this.consumer != null && this.consumer.Model != null)
                this.consumer.Model.TryCloseChannel();
            if(this.consumerEvent!=null && this.consumerEvent.Model !=null)
                this.consumerEvent.Model.TryCloseChannel();
            this.pool.Clear();
            this.StopConfiguration();
            this.handle.CloseHandle(appId, code);
            this.StopHealtTask();
            watch.Stop();        
            this.log.Debug("subscribe stop success appid {0} code {1},stop run {2} ms", this.appId, this.code, watch.ElapsedMilliseconds);
        }

        private void SubscribeWorkEvent()
        {
            var channel = pool.CreateChannel(appId, code);
            if (channel == null)
            {
                log.Error("appId:{0},Code:{1} channel 创建失败,无法启动消费者服务",appId,code);
                return;
            }
            SubscribeDeclare(channel);
            InitConsumerEvent(channel);
            StartConsumerEvent(channel);
        }

        private void ReSubscribeWorkEvent()
        {
            SubscribeWorkEvent();
        }
        #region
        [Obsolete]
        private void SubscribeWork()
        {
          
            var subsceibeTask = Task.Factory.StartNew(async () =>
            {
                #region
                //var consumer = EnsureSubscribeDeclare(await pool.CreateChannelAsync(appId, code).ConfigureAwait(false));
                //listConsumer.Add(consumer);   
                #endregion
                var consumer = await SubscribeDeclareAndStartConsumerAsync(await pool.CreateChannelAsync(appId, code).ConfigureAwait(false));
                if (consumer == null)
                {
                    log.Error("appid:{0},code:{1} consumer is null,can't start consumer.", appId, code);
                    return;
                }
                while (!cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        if (!consumer.Model.IsClosed)
                        {
                            var eventArgs = consumer.Queue.Dequeue();
                            if (eventArgs != null)
                            {                               
                                await Task.Factory.StartNew(() => HandleMessageASync(handle, eventArgs, consumer.Model)
                                                                                        , cts.Token
                                                                                        , TaskCreationOptions.DenyChildAttach
                                                                                        , TaskScheduler.Default);                               
                            }
                        }
                        else
                        {
                            WaitRecovery(consumer, null, "订阅消息channel已关闭", false);             
                        }
                    }
                    catch (EndOfStreamException ex)
                    {
                        WaitRecovery(consumer, ex.ToString(), "订阅消息异常(EndOfStreamException)");
                    }
                    catch (AlreadyClosedException ex)
                    {
                        log.Error("订阅消息异常(AlreadyClosedException) cancell {0} ,{1}", cts.Token.IsCancellationRequested, ex.ToString());
                    }
                    catch (AggregateException ex)
                    {
                        ex.Handle(log, "订阅消息异常(AggregateException)");
                    }
                    catch (Exception ex)
                    {
                        log.Error("订阅消息异常(Exception) cancell {0} ,{1}", cts.Token.IsCancellationRequested, ex.ToString());
                    }
                }
                log.Debug("appid {0},subcribe stop {1}", appId, cts.Token.IsCancellationRequested);
            }, cts.Token, TaskCreationOptions.None, new ThreadPerTaskScheduler());
            th.Add(subsceibeTask);
            log.Debug("appid {0},code {1} 创建消费者完成", appId, code);
        }
        [Obsolete]
        private void WaitRecovery(QueueingBasicConsumer consumer,string execptionStrack, string description,bool wait=true)
        {
            log.Error("{0},appId:{1},code:{2},channel IsClosed:{3},ShutdownInitiator:{4},Initiator:{5},ReplyText:{6},starack:{7}"
                    , description
                    , appId
                    , code
                    , consumer.Model.IsClosed
                    , consumer.Model.CloseReason.Initiator
                    , consumer.Model.CloseReason.ReplyCode
                    , consumer.Model.CloseReason.ReplyText
                    , execptionStrack);
            if (consumer.Model.CloseReason.Initiator != ShutdownInitiator.Application)
            {                
//                log.Info("appid:{0},code:{1} mq channel closed reason {2},wait Recovery", appId
//                    , code,
//                    consumer.Model.CloseReason.Initiator);
                @event.WaitOne();
            }
        }
        [Obsolete]
        private void CheckIsNeedUpdateSemaphorelSize()
        {
            var _cfg = MQMainConfigurationManager.Builder.GetConfiguration(appId, code);
            var newConcurrent = (int)_cfg.ConsumeCfg.MaxThreadCount.Value;
            if (current_Concurrent < newConcurrent)
            {
                lock (thObj)
                {
                    if (current_Concurrent < newConcurrent)
                    {
                        semaphore = new SemaphoreSlim(newConcurrent, newConcurrent);
                        log.Debug("检测到增加了并发数,current Semaphore: {0},new Semaphore:{1}", current_Concurrent
                            , newConcurrent);
                        current_Concurrent = newConcurrent;
                    }
                }
            }
        }
      
        #endregion
        private void StartConfiguration()
        {
            //如果启用多个appdomain策略
            if ("EnableMultipleDomain".GetAppSettings("0") == "1")
                MQMainConfigurationManager.Builder.Start();
        }
        private void StopConfiguration()
        {
            //如果启用多个appdomain策略
            if ("EnableMultipleDomain".GetAppSettings("0") == "1")
                MQMainConfigurationManager.Builder.Stop();
        }
      
        private Task SubscribeDeclareAsync(IModel channel)
        {
            Action fn = () => SubscribeDeclare(channel);         
            //执行
            return fn.ExecuteSynchronously();
        }

        private void SubscribeDeclare(IModel channel)
        {
            //声明交换机
            channel.ExchangeDeclare(cfg.ExchangeCfg.ExchangeName, cfg.ExchangeCfg._ExchangeType.ToString().ToLower()
                , cfg.ExchangeCfg.Durable.Value
                , cfg.ExchangeCfg.IsExchangeAutoDelete.Value
                , cfg.ExchangeCfg.Arguments);
            //声明队列
            channel.QueueDeclare(cfg.QueueCfg.QueueName, cfg.QueueCfg.IsDurable.Value, cfg.QueueCfg.IsQueueExclusive.Value
                , cfg.QueueCfg.IsAutoDelete.Value
                , cfg.QueueCfg.Args);
            //绑定到交换机
            channel.QueueBind(cfg.QueueCfg.QueueName, cfg.ExchangeCfg.ExchangeName
                , string.IsNullOrEmpty(cfg.ConsumeCfg.RoutingKey) ? "#.#" : cfg.ConsumeCfg.RoutingKey, cfg.QueueCfg.HeadArgs);
            //声明消费者              
            //声明流量阀值(客户端只接受小于等于cfg.ConsumeCfg.PrefetchCount.Value 的消息数量）
            cfg.ConsumeCfg.PrefetchCount.NotNullAction(v => channel.BasicQos(0, v, false));
        }

        private async Task<QueueingBasicConsumer> SubscribeDeclareAndStartConsumerAsync(IModel channel)
        {
            try
            {
                if (channel == null)
                {
                    log.Error("appID:{0},Code:{1} channel 创建失败,无法启动消费者服务", appId, code);
                    return null;
                }
                //声明队列属性
                await SubscribeDeclareAsync(channel);
                //启动队列
                var consumer = await StartConsumerAsync(channel).ConfigureAwait(false);
                log.Debug("appid:{0},code:{1},consumer start ok.".Fomart(appId, code));
                return consumer;
            }
            catch (AggregateException ex)
            {
                log.Error("EnsureSubscribeDeclare error.0 appid {0},code {1},ex {2} ", this.appId, this.code, ex.ToString());
                return null;
            }
            catch (Exception ex)
            {
                this.pool.Clear();
                this.handle.CloseHandle(appId, code);
                this.cts.Cancel();
                log.Error("EnsureSubscribeDeclare error.1 appid {0},code {1},ex {2} ", this.appId, this.code, ex.ToString());
                return null;
            }
        }
        private Task<QueueingBasicConsumer> StartConsumerAsync(IModel channel)
        {
            Func<QueueingBasicConsumer> action = () =>
            {
                var consumer = new QueueingBasicConsumer(channel);
                var tag = "amq.ctag-{0}-{1}".Fomart(_Utils.GetLocalHostIp(), DateTime.Now.ToString("yyyyMMddHHmmssfff"));
                channel.BasicConsume(cfg.QueueCfg.QueueName, cfg.ConsumeCfg.IsAutoAcknowledge.Value, tag, consumer);
                return consumer;
            };
            return action.ExecuteSynchronously();
        }

        private void StartConsumerEvent(IModel channel)
        {           
            var tag = "amq.ctag-{0}-{1}".Fomart(_Utils.GetLocalHostIp(), DateTime.Now.ToString("yyyyMMddHHmmssfff"));
            channel.BasicConsume(cfg.QueueCfg.QueueName, cfg.ConsumeCfg.IsAutoAcknowledge.Value, tag, consumerEvent);
        }

        private void InitConsumerEvent(IModel channel)
        {
            consumerEvent = new EventingBasicConsumer(channel);
            consumerEvent.Registered += consumerEvent_Registered;
            consumerEvent.Unregistered += consumerEvent_Unregistered;
            consumerEvent.Received += consumerEvent_Received;
            consumerEvent.Shutdown += consumerEvent_Shutdown;
        }

        void consumerEvent_Shutdown(object sender, ShutdownEventArgs e)
        {          
            log.Info("[consumerEvent_Shutdown] healtIsOk:{0},appid:{1},code:{2},{3}", healtIsOk, appId, code, e.ToString());
            healtIsOk = false;
        }

        void consumerEvent_Received(object sender, BasicDeliverEventArgs e)
        {
            healtIsOk = true;
            Task.Run(() => HandleMessageASync(handle, e, (sender as EventingBasicConsumer).Model));
        }    

        void consumerEvent_Unregistered(object sender, ConsumerEventArgs e)
        {
            log.Info("[consumerEvent_Unregistered]appid:{0},code:{1},{2}", appId, code, e.ConsumerTag); 
        }

        void consumerEvent_Registered(object sender, ConsumerEventArgs e)
        {
            log.Info("[consumerEvent_Registered]appid:{0},code:{1},{2}", appId, code, e.ConsumerTag);  
        }

        private async Task HandleMessageASync(IMessageHandler<byte[]> handle, BasicDeliverEventArgs eventArgs, IModel channdel)
        {
            await semaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                var availableThread = MQThreadPool.GetAvailableWorkThreadsAndIoThread();
                log.Debug("[begin handle] appid:{0}, code:{1} begin async handler, availableWorkThread:{2},iOThread:{3},semaphore current count:{4}"
                    , appId
                    , code
                    , availableThread.Item1
                    , availableThread.Item2
                    , semaphore.CurrentCount);
                //执行处理
                await handle.Handle(new MessageHandleContext<byte[]>(eventArgs.Body
                                                                       , eventArgs.Redelivered
                                                                       , appId
                                                                       , code
                                                                       , eventArgs.BasicProperties.GetMQMessageId()
                                                                       , _MessageSource.MessageSource_RabbitMQ
                                                                       , uuid: eventArgs.BasicProperties.GetMQMessageId("uuid"))
                                                                       , async str=>await HandleSuccessActionFunc(eventArgs, channdel, availableThread).ConfigureAwait(false)
                                                                       , null)
                                                                       .ConfigureAwait(false);
            }
            finally
            {
                semaphore.Release();
            }
        }

        private async Task HandleSuccessActionFunc(BasicDeliverEventArgs eventArgs, IModel channdel, Tuple<int, int> availableThread)
        {
            if (cfg.ConsumeCfg.IsAutoAcknowledge.Value) 
                  return;
            var result= await
            Ack(eventArgs, channdel)
            .ConfigureAwait(false);
            log.Debug("[end handle] message mid:{0},tag:{1} ACK ok?{2},availableWorkThread {3},iOThread {4}",
                        eventArgs.BasicProperties.GetMQMessageId()
                        , eventArgs.DeliveryTag
                        , result
                        , availableThread.Item1
                        , availableThread.Item2);                           
        }

        private async Task<bool> Ack(BasicDeliverEventArgs eventArgs, IModel channdel)
        {
            //如果Channel 已关闭，则放弃ACK
            if (channdel.IsClosed) 
                return false;
            Func<bool> ackAction = () =>
            {
                try
                {
                    channdel.BasicAck(eventArgs.DeliveryTag, false);
                    return true;
                }
                catch (AlreadyClosedException ex)
                {
                    log.Error("ACK fail AlreadyClosedException",ex);
                    return false;
                }
                catch (Exception ex)
                {
                    log.Error("ACK fail Exception", ex);
                    return false;
                }
            };
            return await ackAction
                .ExecuteSynchronously()
                .WithHandleException(log, null, "appid:{0},code:{1}, ack fail,mid:{2}"
                                    , mqMainCfg.AppId
                                    , cfg.Code
                                    , eventArgs.BasicProperties.GetMQMessageId())
                .ConfigureAwait(false);
           
        }
        private void InitLogger()
        {
            //如果启用多个appdomain策略           
            if ("EnableMultipleDomain".GetAppSettings("0") == "1")
            {
                log4net.GlobalContext.Properties["LogFileName"] = AppDomain.CurrentDomain.FriendlyName.Replace(":", "");
                log4net.GlobalContext.Properties["LogDirectory"] = AppDomain.CurrentDomain.FriendlyName.Replace(":", "");
                log4net.Config.XmlConfigurator.Configure(new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "log4net.config")));
            }
            else
            {
                //log4net.GlobalContext.Properties["LogFileName"] = "ad.{0}.{1}".Fomart(appId, code);
                //log4net.GlobalContext.Properties["LogDirectory"] = "ad.{0}.{1}".Fomart(appId, code);
                //log4net.Config.XmlConfigurator.Configure(new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "log4net.config")));
            }
        }
        private void ThreadStop()
        {
            try
            {
                cts.Cancel();
                log.Debug("ThreadStop stop success.");
            }
            catch (AggregateException ex)
            {
                log.Error("ThreadStop stop AggregateException appid {0},code {1},ex {2}", appId, code, ex);
            }
            catch (Exception ex)
            {
                log.Error("ThreadStop stop execption appid {0},code {1},ex {2}", appId, code, ex);
            }
        }
        private async Task TaskStop()
        {
            try
            {
                await Task.WhenAll(th.ToArray()).ConfigureAwait(false);
                log.Debug("TaskStop stop success.");
            }
            catch (AggregateException ex)
            {
                log.Error("TaskStop stop AggregateException appid {0},code {1},ex {2}", appId, code, ex);
            }
            catch (Exception ex)
            {
                log.Error("TaskStop stop execption appid {0},code {1},ex {2}", appId, code, ex);
            }
        }
        private void UpdateConnectionPool(IEnumerable<_UpdateConnectionInfo> callBack)
        {
            YmtSystemAssert.AssertArgumentNotNull(callBack, "未获取到MQ配置");
            callBack.EachAction(item => pool.ReBuilderPool(item, this, true, this.appId, this.code));
        }
        private static IMessageHandler<byte[]> CreateEventHandler()
        {
            var assemblyName = ConfigurationManager.AppSettings["handleAssemblyName"].Split(new char[] { ',' });
            var handler = Activator.CreateInstance(assemblyName[1], assemblyName[0]).Unwrap() as IMessageHandler<byte[]>;
            YmtSystemAssert.AssertArgumentNotNull(handler, "IMessageHandler 不能为空");
            return handler;
        }

        private void StartHealtTask()
        {
            if (!mqMainCfg.ConnCfg.HealthCheck)
            {
                log.Debug("[StartHealtTask] is disable health");
                return;
            }            
            if (!IsRuningHealtThread)
            {
                lock (thObj)
                {
                    if (!IsRuningHealtThread)
                    {
                        IsRuningHealtThread = true;                                         
                        healtThread = new Thread(() =>
                        {
                            while (MQMainConfigurationManager.Builder.GetConfiguration(appId, false, false).ConnCfg.HealthCheck)
                            {      
                                //线程启动即刻执行更新链接状态                                
                                try
                                {
                                    var delay = MQMainConfigurationManager.Builder.GetConfiguration(appId, false, false).ConnCfg.HealthSecond;
                                    var status = healtIsOk
                                        ? BusPushHealth.ConnStatus_Ok
                                        : BusPushHealth.ConnStatus_shutdown;
                                    BusPushHealthAppService.UpdateBusPushHealth("AppId".GetAppSettings(""), status);
                                    log.Info("[HealthUpdate] done,appid:{0},consumer conn status:{1},next update :{2}", appId, status,
                                        DateTime.Now.AddSeconds(delay));
                                    Thread.Sleep(delay * 1000);
                                }
                                catch(Exception ex)
                                {
                                    log.Error("[StartHealtTask] run healt task exception ", ex);
                                }
                            }
                        }) { IsBackground = true };
                        healtThread.Start();
                        log.Debug("HealthUpdate StartHealtTask start success. appid:{0}",appId);
                    }
                }
            }            
        }

        private void StopHealtTask()
        {
            if (!mqMainCfg.ConnCfg.HealthCheck)
            {
                log.Debug("[StartHealtTask] is disable health");
                return;
            }
            if (IsRuningHealtThread)
            {
                lock (thObj)
                {
                    if (IsRuningHealtThread)
                    {
                        IsRuningHealtThread = false;
                        try
                        {
                            var task = BusPushHealthAppService.UpdateBusPushHealth("AppId".GetAppSettings(""),
                                BusPushHealth.ConnStatus_Ok);
                            log.Debug("StopHealtTask UpdateBusPushHealth ConnStatus_Ok,appId:{0}",appId);
                        }
                        catch (Exception ex)
                        {
                            log.Error("StopHealtTask Exception {0}", ex);
                        }     
                    }
                }
            }                  
        }

        public Task Notify(string appId, IModel channel)
        {
            return Task.Factory.StartNew(async () =>
            {
                if (!healtIsOk)
                {
                    healtIsOk = true;
                    await
                        BusPushHealthAppService.UpdateBusPushHealth("AppId".GetAppSettings(""),
                            BusPushHealth.ConnStatus_Ok);
                }
                log.Debug("接到链接恢复通知，重新构建consumer完成,appid {0},code {1}".Fomart(this.appId, this.code));
            });
        }

        public Task Notify()
        {
            log.Debug("ConnShutdownNotify Notify UpdateBusPushHealth ConnStatus_shutdown.");
            healtIsOk = false;
            return BusPushHealthAppService.UpdateBusPushHealth("AppId".GetAppSettings(""), BusPushHealth.ConnStatus_shutdown);
        }
    }
}
