//#define Test
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Net;
//using System.Text;
//using System.Threading;
//using System.IO;
//using System.Diagnostics;
//using System.Threading.Tasks;
//using YmatouMQ.Common.Extensions;
//using YmatouMQ.Common.Extensions._Task;
//using YmatouMQ.Common.Extensions.Serialization;
//using YmatouMQ.Common.Utils;
//using YmatouMQ.Common;
//using YmatouMQNet4.Utils;
//using YmatouMQ.Log;
//using System.Configuration;

//namespace YmatouMQNet4.Configuration
//{
//    /// <summary>
//    /// MQ配置管理
//    /// </summary>
//    public class MQMainConfigurationManager
//    {
//        private static readonly string def_cfg_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "{0}.Default.Config".Fomart(AppDomain.CurrentDomain.FriendlyName.Replace(":", "")));
//        private static readonly string dum_cfg_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "{0}.MQ.dump.Config".Fomart(AppDomain.CurrentDomain.FriendlyName.Replace(":", "")));
//        private static readonly Lazy<MQMainConfigurationManager> lazy = new Lazy<MQMainConfigurationManager>(() => new MQMainConfigurationManager());
//        private readonly ILog log = LogFactory.GetLogger(LogEngineType.RealtimelWriteFile, "YmatouMQ.Configuration.MQMainConfigurationManager");
//        private readonly ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
//        private Dictionary<string, MQMainConfiguration> _cacheCfg = new Dictionary<string, MQMainConfiguration>();
//        private MQMainConfiguration _mq_cache_default_Cfg = MQMainConfiguration.DefaultMQCfg;
//        private bool isRun = false;
//        //private Timer timer;
//        private Thread syncThread;
//        private readonly object lock_defCfg = new object();
//        private readonly object lock_AppCfg = new object();
//        private Action<IEnumerable<_UpdateConnectionInfo>> updateAction;

//        /// <summary>
//        /// 构建MQConfigurationManager
//        /// </summary>
//        public static MQMainConfigurationManager Builder { get { return lazy.Value; } }

//        private MQMainConfigurationManager()
//        {
//            InitAppSync();
//            InitCfgDefault_SyncWork();
//            log.Debug("配置初始化完成");
//        }
//        /// <summary>
//        /// 注册链接配置更新回调
//        /// </summary>
//        /// <param name="updateAction"></param>
//        public void RegisterConnectionConfigurationUpdate(Action<IEnumerable<_UpdateConnectionInfo>> updateAction)
//        {
//            this.updateAction = updateAction;
//            log.Debug("注册更新回调操作");
//        }

//        /// <summary>
//        /// 获取默认的全局配置
//        /// </summary>
//        /// <returns></returns>
//        public Tuple<MQMainConfiguration, CfgTypeEnum> GetDefaultConfiguration()
//        {
//            //测试使用本地全局配置
//#if !Test
//            var path = @"E:\works\ymatou\project2\Ymatou\Main\YmatouMQ2.0\Main\doc\ymatoumqdefault.config";
//            var cfg = LoadLocalConfiguration(path);
//            log.Debug("获取MQ全局配置");
//            if (string.IsNullOrEmpty(cfg))
//            {
//                log.Warning("未获取到MQ全局配置，使用内存默认全局配置");
//                return MQMainConfiguration.DefaultMQCfg;
//            }
//            else
//            {
//                var returnVal = cfg.JSONDeserializeFromString<MQMainConfiguration>();
//                if (returnVal == null)
//                {
//                    log.Warning("序列化全局配置返回空，使用内存默认全局配置");
//                    return MQMainConfiguration.DefaultMQCfg;
//                }
//                else
//                {
//                    log.Debug("正确获取MQ到全局配置");
//                    return returnVal;
//                }
//            }
//#else
//            //获取远程配置全局配置
//            CfgTypeEnum type = CfgTypeEnum.Server;
//            MQMainConfiguration cfg = null;
//            var cfgStr = _Utils.GetRequestMQConfigurationServer(ConfigurationUri.default_Cfg,log);
//            if (cfgStr.IsEmpty())
//            {
//                log.Debug("无法从配置服务获取MQ应用配置，使用本地{0}配置", def_cfg_path);
//                cfgStr = _Utils.LoadLocalConfiguration(def_cfg_path);
//                type = CfgTypeEnum.LocalDisk;
//            }
//            if (cfgStr.IsEmpty())
//            {
//                log.Warning("无法从配置服务，本地获取MQ应用配置，使用内存默认配置");
//                cfg = MQMainConfiguration.DefaultMQCfg;
//                type = CfgTypeEnum.LocalMemory;
//            }
//            else
//            {
//                cfg = cfgStr.JSONDeserializeFromString<IEnumerable<MQMainConfiguration>>().FirstOrDefault();
//            }
//            return Tuple.Create(cfg, type);
//#endif
//        }
//        /// <summary>
//        /// 获取指定的配置
//        /// </summary>
//        /// <param name="appId"></param>
//        /// <param name="notFindThrowOut"></param>
//        /// <returns></returns>
//        public MQMainConfiguration GetConfiguration(string appId, bool notFindThrowOut = true, bool checkCfgProperty = true)
//        {
//            YmtSystemAssert.AssertArgumentNotEmpty(appId, "app id 为空无法获取配置");
//            rwLock.EnterReadLock();
//            try
//            {
//                MQMainConfiguration cfgInfo;
//                _cacheCfg.TryGetValue(appId, out cfgInfo);
//                if (cfgInfo != null)
//                {
//                    //检查配置属性
//                    if (checkCfgProperty)
//                        CheckConfiguration(cfgInfo, _mq_cache_default_Cfg);
//                    return cfgInfo;
//                }
//                if (notFindThrowOut)
//                    throw new KeyNotFoundException(string.Format("{0}没有对应的配置", appId));
//                else
//                    return _mq_cache_default_Cfg;
//            }
//            finally
//            {
//                rwLock.ExitReadLock();
//            }
//        }
//        /// <summary>
//        /// 获取指定应用下的指定消息配置
//        /// </summary>
//        /// <param name="appId"></param>
//        /// <param name="code"></param>
//        /// <returns></returns>
//        public MessageConfiguration GetConfiguration(string appId, string code, bool checkCfgPropertys = true)
//        {
//            var msgCfg = GetConfiguration(appId, checkCfgProperty: checkCfgPropertys).MessageCfgList.SingleOrDefault(e => e.Code == code);
//            YmtSystemAssert.AssertArgumentNotNull(msgCfg, string.Format("应用{0}不存在对应的业务消息{1}配置", appId, code));
//            return msgCfg;
//        }
//        public Task<MessageConfiguration> GetConfigurationAsync(string appId, string code)
//        {
//            var tcs = new TaskCompletionSource<MessageConfiguration>();
//            try
//            {
//                var result = GetConfiguration(appId, code);
//                tcs.SetResult(result);
//            }
//            catch (Exception ex)
//            {
//                tcs.SetException(ex);
//            }
//            return tcs.Task;
//        }
//        /// <summary>
//        /// 获取所有配置。如果异常则使用默认的全局配置
//        /// </summary>
//        /// <param name="useDefaultCfg"></param>
//        /// <returns></returns>
//        public Dictionary<string, MQMainConfiguration> GetConfiguration()
//        {
//            rwLock.EnterReadLock();
//            try
//            {
//                if (!SpinWait.SpinUntil(() => _cacheCfg != null, 3000))
//                {
//                    log.Error("MQConfiguration 配置为空");
//                }
//                return _cacheCfg;
//            }
//            catch (Exception ex)
//            {
//                ex.Handle(log, "获取配置异常");
//                return new Dictionary<string, MQMainConfiguration> { { "default", GetDefaultConfiguration().Item1 } };
//            }
//            finally
//            {
//                rwLock.ExitReadLock();
//            }
//        }
//#if !Test
//        /// <summary>
//        /// 自动维护配置测试
//        /// </summary>
//        public void TestConfigurationMaintain()
//        {
//            Start();
//        }
//#endif
//        /// <summary>
//        /// 备份MQ默认应用配置
//        /// </summary>    
//        public void DumpMQDefaultConfigurationFile(MQMainConfiguration cfg)
//        {
//            var json = cfg.JSONSerializationToString();
//            lock (lock_defCfg)
//            {
//                if (File.Exists(def_cfg_path)) File.Delete(def_cfg_path);
//                FileAsync.WriteAllText(def_cfg_path, json).WithHandleException("{0}", "写入MQSystem配置错误");
//            }
//        }
//        /// <summary>
//        /// 停止
//        /// </summary>
//        public void Stop()
//        {
//            //if (timer != null)
//            //    timer.Dispose();
//            isRun = false;
//            log.Debug("MQ 停止自动维护配置成功");
//        }
//        /// <summary>
//        /// 启动配置维护
//        /// </summary>
//        public void Start()
//        {
//            if (isRun) return;
//            isRun = true;
//            #region
//            //timer = new Timer(o =>
//            //{
//            //    if (!isRun)
//            //        return;
//            //    using (var monitor = new MethodMonitor(log, 1000, "完成一次配置维护"))
//            //    {
//            //        //同步配置&更新配置                
//            //        TrySyncDefaultCfg();
//            //        TrySyncAppCfg();
//            //    }
//            //    timer.Change(ConfigurationManager.AppSettings["MainCfgTimestamp"].ToInt32(30000), Timeout.Infinite);
//            //}, null, Timeout.Infinite, Timeout.Infinite);
//            //timer.Change(ConfigurationManager.AppSettings["MainCfgTimestamp"].ToInt32(30000), Timeout.Infinite);
//            #endregion
//            syncThread = new Thread(_o =>
//            {
//                while (isRun)
//                {
//                    Thread.Sleep(ConfigurationManager.AppSettings["MainCfgTimestamp"].ToInt32(30000));
//                    using (var monitor = new MethodMonitor(log, 1000, "app cfg sync "))
//                    {
//                        //同步配置&更新配置                
//                        TrySyncDefaultCfg();
//                        TrySyncAppCfg();
//                        log.Info("sync app cfg end,status ok.run {0} ms", monitor.GetRunTime.TotalMilliseconds);
//                    }
//                }
//            }) { IsBackground = true };
//            syncThread.Start();
//            log.Debug("MQ 启动自动维护配置成功, {0} 毫秒同步一次配置", ConfigurationManager.AppSettings["MainCfgTimestamp"].ToInt32(30000));
//        }

//        private void TrySyncDefaultCfg()
//        {
//            try
//            {
//                CfgDefaultUpdateWork(CfgDefault_SyncWork());
//            }
//            catch (AggregateException ex)
//            {
//                log.Error("同步默认配置异常AggregateException", ex);
//            }
//            catch (Exception ex)
//            {
//                log.Error("同步默认配置异常Exception", ex);
//            }
//        }

//        private void TrySyncAppCfg()
//        {
//            try
//            {
//                CfgAppUpdateWork(CfgAppSyncWork());
//            }
//            catch (AggregateException ex)
//            {
//                log.Error("同步具体配置异常AggregateException", ex);
//            }
//            catch (Exception ex)
//            {
//                log.Error("同步具体配置异常Exception", ex);
//            }
//        }

//        private void CfgAppUpdateWork(Dictionary<string, MQMainConfiguration> _serverCfg)
//        {
//            using (var monitor = new MethodMonitor(log, 200, "更新具体配置"))
//            {
//                if (_cacheCfg == null)
//                {
//                    _cacheCfg = _serverCfg;
//                    return;
//                }
//                if (!CfgVersionCompare(_serverCfg, _cacheCfg))
//                {
//                    return;
//                }
//                if (this.updateAction != null)
//                {
//                    ConnectionCallback(_serverCfg);
//                }
//                rwLock.EnterUpgradeableReadLock();
//                try
//                {
//                    rwLock.EnterWriteLock();
//                    try
//                    {
//                        //更新配置                                  
//                        _cacheCfg = _serverCfg;
//                        log.Debug("获取到新配置,覆盖内存中配置,更新完成");
//                    }
//                    finally
//                    {
//                        rwLock.ExitWriteLock();
//                    }
//                }
//                finally
//                {
//                    rwLock.ExitUpgradeableReadLock();
//                }
//            }
//        }

//        private void ConnectionCallback(Dictionary<string, MQMainConfiguration> _serverCfg)
//        {
//            //执行链接池回调
//            var tmpDic = new HashSet<_UpdateConnectionInfo>();
//            foreach (var serverConn in _serverCfg)
//            {
//                MQMainConfiguration connCfg;
//                if (_cacheCfg.TryGetValue(serverConn.Key, out connCfg))
//                {
//                    //检测链接配置属性变化
//                    if (!serverConn.Value.ConnCfg.Equals(connCfg.ConnCfg))
//                    {
//                        var tmpCacheConnInfo = ConnectionInfo.Build(connCfg.ConnCfg.ConnectionString);
//                        var tmpServerConnInfo = ConnectionInfo.Build(serverConn.Value.ConnCfg.ConnectionString);
//                        CfgUpdateType cfgUpdateType = CfgUpdateType.ConnStringModify;
//                        uint connNum = 0;
//                        if (tmpCacheConnInfo.IsConnHostModify(tmpServerConnInfo))
//                            cfgUpdateType = CfgUpdateType.ConnStringModify;
//                        else
//                        {
//                            if (tmpCacheConnInfo.isAddConn(tmpServerConnInfo, out connNum))
//                                cfgUpdateType = CfgUpdateType.ConnNumAddModify;
//                            else if (tmpCacheConnInfo.isDecrementConn(tmpServerConnInfo, out connNum))
//                                cfgUpdateType = CfgUpdateType.ConnNumDecrement;
//                        }
//                        tmpDic.Add(new _UpdateConnectionInfo { AppId = serverConn.Value.AppId, Conn = tmpServerConnInfo, ConnNum = connNum, UpType = cfgUpdateType });
//                    }
//                    else
//                    {
//                        var tmpServerConnInfo = ConnectionInfo.Build(serverConn.Value.ConnCfg.ConnectionString);
//                        tmpDic.Add(new _UpdateConnectionInfo { AppId = serverConn.Value.AppId, Conn = tmpServerConnInfo, ConnNum = tmpServerConnInfo.PoolMinSize.Value, UpType = CfgUpdateType.AddNewConn });
//                    }
//                }
//            }
//            if (tmpDic != null && tmpDic.Count >= 1)
//            {
//                log.Debug("应用{0}链接属性发生变化，需要重建链接池", string.Join(",", tmpDic.Select(e => e.Conn)));
//                this.updateAction(tmpDic);
//                //Task.Factory.StartNew(e => this.updateAction, tmpDic);
//            }
//        }
//        //配置版本比较
//        private bool CfgVersionCompare(Dictionary<string, MQMainConfiguration> _serverCfg, Dictionary<string, MQMainConfiguration> localCfg)
//        {
//            //服务端配置项大于缓存配置项，或者服务端配置版本大于缓存版本
//            return _serverCfg.Count > localCfg.Count || _serverCfg.Values.Where(s_cfg => localCfg.Values.Where(c_cfg => s_cfg.AppId == c_cfg.AppId && s_cfg.Version > c_cfg.Version).Any()).Any();
//        }
//        /// <summary>
//        /// 检查当前配置，如果没有配置项则使用默认的全局配置
//        /// </summary>
//        /// <param name="currentCfg">当前配置</param>
//        /// <param name="defaultCfg">默认的全局配置</param>
//        private void CheckConfiguration(MQMainConfiguration currentCfg, MQMainConfiguration defaultCfg)
//        {
//            YmtSystemAssert.AssertArgumentNotNull(defaultCfg, "默认全局配置不能为空");
//            currentCfg.NullObjectReplace(v => currentCfg = v, defaultCfg);

//            currentCfg.ConnCfg.NullObjectReplace(v => currentCfg.ConnCfg = v, defaultCfg.ConnCfg);
//            currentCfg.ConnCfg.ConnectionString.NullObjectReplace(v => currentCfg.ConnCfg.ConnectionString = v, defaultCfg.ConnCfg.ConnectionString);

//            var defCfg = defaultCfg.MessageCfgList.First();

//            foreach (var item in currentCfg.MessageCfgList)
//            {
//                //检查消费者配置属性
//                item.ConsumeCfg.NullObjectReplace(v => item.ConsumeCfg = v, defCfg.ConsumeCfg);
//                item.ConsumeCfg.Args.NullObjectReplace(v => item.ConsumeCfg.Args = v, defCfg.ConsumeCfg.Args);
//                item.ConsumeCfg.IsAutoAcknowledge.NullAction(v => item.ConsumeCfg.IsAutoAcknowledge = v, defCfg.ConsumeCfg.IsAutoAcknowledge ?? false);
//                item.ConsumeCfg.MaxThreadCount.NullAction(v => item.ConsumeCfg.MaxThreadCount = v, defCfg.ConsumeCfg.MaxThreadCount ?? 512);
//                item.ConsumeCfg.PrefetchCount.NullAction(v => item.ConsumeCfg.PrefetchCount = v, defCfg.ConsumeCfg.PrefetchCount /*?? 500*/);
//                item.ConsumeCfg.UseMultipleThread.NullAction(v => item.ConsumeCfg.UseMultipleThread = v, defCfg.ConsumeCfg.UseMultipleThread ?? false);
//                item.ConsumeCfg.HandleFailAcknowledge.NullAction(v => item.ConsumeCfg.HandleFailAcknowledge = v, defCfg.ConsumeCfg.HandleFailAcknowledge ?? true);
//                item.ConsumeCfg.HandleFailRQueue.NullAction(v => item.ConsumeCfg.HandleFailRQueue = v, defCfg.ConsumeCfg.HandleFailRQueue ?? false);
//                item.ConsumeCfg.RoutingKey.NullObjectReplace(v => item.ConsumeCfg.RoutingKey = v, defCfg.ConsumeCfg.RoutingKey ?? "#.#");
//                item.ConsumeCfg.CallbackMethodType.NullObjectReplace(v => item.ConsumeCfg.CallbackMethodType = v, defCfg.ConsumeCfg.CallbackMethodType ?? "POST");
//                item.ConsumeCfg.CallbackTimeOut.NullAction(v => item.ConsumeCfg.CallbackTimeOut = v, defCfg.ConsumeCfg.CallbackTimeOut ?? 10000);
//                item.ConsumeCfg.CallbackTimeOutAck.NullAction(v => item.ConsumeCfg.CallbackTimeOutAck = v, defCfg.ConsumeCfg.CallbackTimeOutAck ?? true);
//                item.ConsumeCfg.RetryCount.NullAction(v => item.ConsumeCfg.RetryCount = v, defCfg.ConsumeCfg.RetryCount ?? 1);
//                item.ConsumeCfg.RetryMillisecond.NullAction(v => item.ConsumeCfg.RetryMillisecond = v, defCfg.ConsumeCfg.RetryMillisecond ?? 1000);
//                item.ConsumeCfg.HandleFailPersistentStore.NullAction(v => item.ConsumeCfg.HandleFailPersistentStore = v, defCfg.ConsumeCfg.HandleFailPersistentStore ?? true);
//                item.ConsumeCfg.HandleSuccessSendNotice.NullAction(v => item.ConsumeCfg.HandleSuccessSendNotice = v, defCfg.ConsumeCfg.HandleSuccessSendNotice ?? true);
//                //item.ConsumeCfg.ConsumeCount.NullAction(v => item.ConsumeCfg.ConsumeCount = v, defCfg.ConsumeCfg.ConsumeCount);
//                item.ConsumeCfg.HandleFailMessageToMongo.NotNullAction(v => item.ConsumeCfg.HandleFailMessageToMongo = v, defCfg.ConsumeCfg.HandleFailMessageToMongo ?? false);
//                item.ConsumeCfg.RetryTimeOut.NullAction(v => item.ConsumeCfg.RetryTimeOut = v, defCfg.ConsumeCfg.RetryTimeOut);
//                //检查发布配置属性
//                item.PublishCfg.NullObjectReplace(v => item.PublishCfg = v, defCfg.PublishCfg);
//                item.PublishCfg.PublisherConfirms.NullAction(v => item.PublishCfg.PublisherConfirms = v, defCfg.PublishCfg.PublisherConfirms ?? false);
//                item.PublishCfg.RetryCount.NullAction(v => item.PublishCfg.RetryCount = v, defCfg.PublishCfg.RetryCount ?? 1);
//                item.PublishCfg.RetryMillisecond.NullAction(v => item.PublishCfg.RetryMillisecond = v, defCfg.PublishCfg.RetryMillisecond ?? 500);
//                item.PublishCfg.UseTransactionCommit.NullAction(v => item.PublishCfg.UseTransactionCommit = v, defCfg.PublishCfg.UseTransactionCommit ?? false);
//                item.PublishCfg.RouteKey.NullObjectReplace(v => item.PublishCfg.RouteKey = v, defCfg.PublishCfg.RouteKey ?? "#.#");
//                //检查交换机属性
//                item.ExchangeCfg.NullObjectReplace(v => item.ExchangeCfg = v, defCfg.ExchangeCfg);
//                item.ExchangeCfg._ExchangeType.NullAction(v => item.ExchangeCfg._ExchangeType = v, defCfg.ExchangeCfg._ExchangeType ?? ExchangeType.direct);
//                item.ExchangeCfg.Arguments.NullObjectReplace(v => item.ExchangeCfg.Arguments = v, defCfg.ExchangeCfg.Arguments);
//                item.ExchangeCfg.Durable.NullAction(v => item.ExchangeCfg.Durable = v, defCfg.ExchangeCfg.Durable ?? true);
//                item.ExchangeCfg.IsExchangeAutoDelete.NullAction(v => item.ExchangeCfg.IsExchangeAutoDelete = v, defCfg.ExchangeCfg.IsExchangeAutoDelete ?? false);
//                //检查队列属性
//                item.QueueCfg.NullObjectReplace(v => item.QueueCfg = v, defCfg.QueueCfg);
//                item.QueueCfg.IsQueueExclusive.NullAction(v => item.QueueCfg.IsQueueExclusive = v, defCfg.QueueCfg.IsQueueExclusive ?? false);
//                item.QueueCfg.IsAutoDelete.NullAction(v => item.QueueCfg.IsAutoDelete = v, defCfg.QueueCfg.IsAutoDelete ?? false);
//                item.QueueCfg.IsDurable.NullAction(v => item.QueueCfg.IsDurable = v, defCfg.QueueCfg.IsDurable ?? true);
//                item.QueueCfg.Args.NullObjectReplace(v => item.QueueCfg.Args = v, defCfg.QueueCfg.Args);
//                item.QueueCfg.HeadArgs.NullObjectReplace(v => item.QueueCfg.HeadArgs = v, defCfg.QueueCfg.HeadArgs);
//                //消息属性检查
//                item.MessagePropertiesCfg.NullObjectReplace(v => item.MessagePropertiesCfg = v, defCfg.MessagePropertiesCfg);
//                item.MessagePropertiesCfg.PersistentMessages.NullAction(v => item.MessagePropertiesCfg.PersistentMessages = v, defCfg.MessagePropertiesCfg.PersistentMessages ?? false);
//                item.MessagePropertiesCfg.PersistentMessagesLocal.NullAction(v => item.MessagePropertiesCfg.PersistentMessagesLocal = v, defCfg.MessagePropertiesCfg.PersistentMessagesLocal ?? true);
//                item.MessagePropertiesCfg.PersistentMessagesMongo.NullAction(v => item.MessagePropertiesCfg.PersistentMessagesMongo = v, defCfg.MessagePropertiesCfg.PersistentMessagesMongo ?? true);
//                item.MessagePropertiesCfg.Priority.NullAction(v => item.MessagePropertiesCfg.Priority = v, defCfg.MessagePropertiesCfg.Priority);
//                item.MessagePropertiesCfg.Expiration.NullAction(v => item.MessagePropertiesCfg.Expiration = v, defCfg.MessagePropertiesCfg.Expiration);
//                item.MessagePropertiesCfg.ContextType.NullObjectReplace(v => item.MessagePropertiesCfg.ContextType = v, defCfg.MessagePropertiesCfg.ContextType ?? "application/json");
//                item.MessagePropertiesCfg.ContentEncoding.NullObjectReplace(v => item.MessagePropertiesCfg.ContentEncoding = v, defCfg.MessagePropertiesCfg.ContentEncoding ?? "utf-8");
//                //回调属性检查
//                item.CallbackCfgList.EachAction(c =>
//                {
//                    c.Enable.NullAction(v => c.Enable = v, defCfg.CallbackCfgList.FirstOrDefault() == null ? true : defCfg.CallbackCfgList.FirstOrDefault().Enable ?? true);
//                    c.HttpMethod.NullObjectReplace(v => c.HttpMethod = v, defCfg.CallbackCfgList.First().HttpMethod ?? "POST");
//                    c.Url.NullObjectReplace(v => c.Url = v, defCfg.CallbackCfgList.First().Url);
//                    c.Priority.NullAction(v => c.Priority = v, defCfg.CallbackCfgList.First().Priority);
//                    c.ContentType.NullObjectReplace(v => c.ContentType = v, defCfg.CallbackCfgList.First().ContentType ?? "application/json");
//                    c.CallbackTimeOut.NullAction(v => c.CallbackTimeOut = v, defCfg.CallbackCfgList.First().CallbackTimeOut ?? 2000);
//                    c.CallbackKey.NullObjectReplace(v => c.CallbackKey = v, defCfg.CallbackCfgList.First().CallbackKey);
//                    c.IsRetry.NullAction(v => c.IsRetry = v, defCfg.CallbackCfgList.First().IsRetry ?? 1);
//                });
//            }

//        }
//        private void InitAppSync()
//        {
//            _cacheCfg = LoadLocalDumpConfiguration();
//            if (_cacheCfg == null || !_cacheCfg.Any())
//            {
//                _cacheCfg = CfgAppSyncWork(true);
//            }
//        }
//        //同步MQAPP应用具体匹配
//        private Dictionary<string, MQMainConfiguration> CfgAppSyncWork(bool isCtor = false)
//        {
//            using (var monitor = new MethodMonitor(log, 200, "同步具体配置"))
//            {
//                //申明配置临时变量
//                Dictionary<string, MQMainConfiguration> _serverCfg = null;
//                //从MQ配置中心，获取配置
//                var remotingCfg = _Utils.GetRequestMQConfigurationServer(GenerateRequestUrl(),log);
//                //序列化配置
//                _serverCfg = FillMQConfiguration(remotingCfg);
//                //没有获取远程配置，则加载本地dump 的配置
//                if (_serverCfg == null || _serverCfg.Count == 0)
//                {
//                    log.Debug("未获取到MQ配置中心配置，加载本地dump的配置");
//                    _serverCfg = LoadLocalDumpConfiguration();
//                }
//                else
//                {
//                    //如果只构造函数调用，则直接覆盖本地dump文件
//                    if (isCtor)
//                    {
//                        DumpMQConfigurationFile(remotingCfg);
//                    }
//                    else
//                    {
//                        if (_cacheCfg == null || !_cacheCfg.Values.Any())
//                        {
//                            _cacheCfg = _serverCfg;
//                        }
//                        else
//                        {
//                            if (CfgVersionCompare(_serverCfg, _cacheCfg))
//                            {
//                                DumpMQConfigurationFile(remotingCfg);
//                                log.Debug("远程配置版本 {0} 大于本地dump配置版本 {1}，覆盖本地配置", 0, 1);
//                            }
//                        }
//                    }
//                }
//                //如果本地dum 配置没有获取到，则使用内存的默认配置
//                if (_serverCfg == null || !_serverCfg.Any())
//                {
//                    log.Fatal("本地，远程配置都为空,请检查。使用默认全局配置");
//                    var defaultCfg = GetDefaultConfiguration().Item1;
//                    _serverCfg = new Dictionary<string, MQMainConfiguration> { { "default", defaultCfg } };
//                }
//                return _serverCfg;
//            }
//        }
//        //根据应用程序appdomain获取配置
//        private string GenerateRequestUrl()
//        {
//            var domainName = AppDomain.CurrentDomain.FriendlyName;
//            switch (domainName)
//            {
//                case "YmatouMQConsumeService.exe":
//                case "YmatouMQConsumeSecondaryService.exe":
//                    return ConfigurationUri.subDomainCfg;
//                case "YmatouMQPublishService.exe":
//                case "YmatouMQPublishSecondaryService.exe":
//                    return "{0}?owerhost={1}".Fomart(ConfigurationUri.pubDomainCfg, _Utils.GetCurrentHost());
//                case "YmatouMQMessageCompensateService.exe":
//                    return ConfigurationUri.compensateCfg;
//                default:
//                    var domainNames = AppDomain.CurrentDomain.FriendlyName.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
//                    var appid = AppDomain.CurrentDomain.FriendlyName.StartsWith("ad.") && domainNames.Length > 2 ? domainNames[1] : null;
//                    var code = AppDomain.CurrentDomain.FriendlyName.StartsWith("ad.") && domainNames.Length > 2 ? domainNames[2] : null;
//                    var url = "{0}?appid={1}&code={2}".Fomart(ConfigurationUri.app_Cfg, appid, code);
//                    return url;
//            }
//        }
//        private void InitCfgDefault_SyncWork()
//        {
//            var cfgStr = _Utils.LoadLocalConfiguration(def_cfg_path);
//            if (string.IsNullOrEmpty(cfgStr))
//            {
//                _mq_cache_default_Cfg = CfgDefault_SyncWork(true);
//            }
//            else
//            {
//                _mq_cache_default_Cfg = cfgStr.JSONDeserializeFromString<IEnumerable<MQMainConfiguration>>().FirstOrDefault();
//                if (_mq_cache_default_Cfg == null)
//                {
//                    _mq_cache_default_Cfg = CfgDefault_SyncWork(true);
//                }
//            }
//        }
//        //同步MQ应用默认配置
//        private MQMainConfiguration CfgDefault_SyncWork(bool isCtor = false)
//        {
//            using (var monitor = new MethodMonitor(log, 200, "default app cfg Sync "))
//            {
//                var cfgTuple = GetDefaultConfiguration();
//                MQMainConfiguration tmp_Cfg = cfgTuple.Item1;
//                if (isCtor)
//                {
//                    if (cfgTuple.Item2 == CfgTypeEnum.Server)
//                    {
//                        DumpMQDefaultConfigurationFile(tmp_Cfg);
//                    }
//                }
//                else
//                {
//                    if (_mq_cache_default_Cfg == null)
//                    {
//                        _mq_cache_default_Cfg = tmp_Cfg;
//                    }
//                    else
//                    {
//                        CfgDefaultUpdateWork(tmp_Cfg);
//                    }
//                }
//                return tmp_Cfg;
//            }
//        }
//        //更新默认配置
//        private void CfgDefaultUpdateWork(MQMainConfiguration serverCfg)
//        {
//            if (serverCfg.Version > _mq_cache_default_Cfg.Version)
//            {
//                using (var monitor = new MethodMonitor(log, 200, "Dump默认配置"))
//                {
//                    _mq_cache_default_Cfg = serverCfg;
//                    DumpMQDefaultConfigurationFile(serverCfg);
//                }
//            }
//        }
//        //dump MQ 配置文件
//        private void DumpMQConfigurationFile(string cfgInfo)
//        {
//            if (cfgInfo == null) return;
//            log.Debug("dump 远程获取的配置到本地磁盘");
//            lock (lock_AppCfg)
//            {
//                if (File.Exists(dum_cfg_path)) File.Delete(dum_cfg_path);
//                FileAsync.WriteAllText(dum_cfg_path, cfgInfo).WithHandleException(log, null, "{0}", "dump appCfg error");
//            }
//        }
//        //加载本地磁盘上dump的配置
//        private Dictionary<string, MQMainConfiguration> LoadLocalDumpConfiguration()
//        {
//            if (!File.Exists(dum_cfg_path))
//            {
//                log.Debug("{0},{1}", "配置文件不存在", dum_cfg_path);
//                return new Dictionary<string, MQMainConfiguration>();
//            }
//            log.Debug("{0},{1}", "加载本地磁盘上的配置", dum_cfg_path);
//            string cfgString = string.Empty;
//            using (var fileStream = FileAsync.OpenRead(dum_cfg_path))
//            using (var streamRead = new StreamReader(fileStream, Encoding.GetEncoding("utf-8")))
//            {
//                cfgString = streamRead.ReadToEnd();
//            }
//            return Adapter(cfgString.JSONDeserializeFromString<IEnumerable<MQMainConfiguration>>());
//        }
//        //填充内存配置
//        private Dictionary<string, MQMainConfiguration> FillMQConfiguration(string value)
//        {
//            if (string.IsNullOrEmpty(value)) return null;
//            Dictionary<string, MQMainConfiguration> dic = new Dictionary<string, MQMainConfiguration>();
//            value.JSONDeserializeFromString<IEnumerable<MQMainConfiguration>>().EachAction(cfg => dic[cfg.AppId] = cfg);
//            //log.Info("从配置服务获取到{0}个配置", dic.Count);
//            return dic;
//        }        
//        private Dictionary<string, MQMainConfiguration> Adapter(IEnumerable<MQMainConfiguration> cfgList)
//        {
//            Dictionary<string, MQMainConfiguration> c = new Dictionary<string, MQMainConfiguration>();
//            cfgList.EachAction(i => c[i.AppId] = i);
//            return c;
//        }
//    }
//}
