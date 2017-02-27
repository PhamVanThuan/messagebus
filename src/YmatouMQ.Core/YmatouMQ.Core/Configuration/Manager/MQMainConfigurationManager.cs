#define Test
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using YmatouMQNet4.Extensions;
using YmatouMQNet4.Extensions._Task;
using YmatouMQNet4.Extensions.Serialization;
using YmatouMQNet4.Utils;
using YmatouMQNet4.Connection;

namespace YmatouMQNet4.Configuration
{
    /// <summary>
    /// MQ配置管理
    /// </summary>
    public class MQMainConfigurationManager
    {
        private static readonly string def_cfg_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "YamtouMQDefault.Config");
        private static readonly string dum_cfg_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "YamtouMQConfig.dump.Config");
        private static readonly Lazy<MQMainConfigurationManager> lazy = new Lazy<MQMainConfigurationManager>(() => new MQMainConfigurationManager(), true);
        private readonly ILog log = LogFactory.GetLogger(LogEngineType.RealtimelWriteFile, "YmatouMQ.Configuration.MQMainConfigurationManager");
        private readonly ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        private Dictionary<string, MQMainConfiguration> _cacheCfg = new Dictionary<string, MQMainConfiguration>();
        private MQMainConfiguration _mq_cache_default_Cfg = MQMainConfiguration.DefaultMQCfg;
        private bool isRun = false;
        private Timer timer;
        private readonly object lock_defCfg = new object();
        private readonly object lock_AppCfg = new object();
        private Action<IEnumerable<_UpdateConnectionInfo>> updateAction;

        /// <summary>
        /// 构建MQConfigurationManager
        /// </summary>
        public static MQMainConfigurationManager Builder { get { return lazy.Value; } }

        private MQMainConfigurationManager()
        {
            _cacheCfg = CfgAppSyncWork(true);
            _mq_cache_default_Cfg = CfgDefault_SyncWork(true);
            log.Debug("配置初始化完成");
        }
        /// <summary>
        /// 注册链接配置更新回调
        /// </summary>
        /// <param name="updateAction"></param>
        public void RegisterConnectionConfigurationUpdate(Action<IEnumerable<_UpdateConnectionInfo>> updateAction)
        {
            this.updateAction = updateAction;
            log.Debug("注册更新回调操作");
        }
        /// <summary>
        /// 获取默认的全局配置
        /// </summary>
        /// <returns></returns>
        public Tuple<MQMainConfiguration, CfgTypeEnum> GetDefaultConfiguration()
        {
            //测试使用本地全局配置
#if !Test
            var path = @"E:\works\ymatou\project2\Ymatou\Main\YmatouMQ2.0\Main\doc\ymatoumqdefault.config";
            var cfg = LoadLocalConfiguration(path);
            log.Debug("获取MQ全局配置");
            if (string.IsNullOrEmpty(cfg))
            {
                log.Warning("未获取到MQ全局配置，使用内存默认全局配置");
                return MQMainConfiguration.DefaultMQCfg;
            }
            else
            {
                var returnVal = cfg.JSONDeserializeFromString<MQMainConfiguration>();
                if (returnVal == null)
                {
                    log.Warning("序列化全局配置返回空，使用内存默认全局配置");
                    return MQMainConfiguration.DefaultMQCfg;
                }
                else
                {
                    log.Debug("正确获取MQ到全局配置");
                    return returnVal;
                }
            }
#else
            //获取远程配置全局配置
            CfgTypeEnum type = CfgTypeEnum.Server;
            MQMainConfiguration cfg = null;
            var cfgStr = RequestMQConfigurationServer(ConfigurationUri.default_Cfg);
            if (cfgStr.IsNull())
            {
                log.Debug("无法从配置服务获取MQ应用配置，使用本地{0}配置", def_cfg_path);
                cfgStr = LoadLocalConfiguration(def_cfg_path);
                type = CfgTypeEnum.LocalDisk;
            }
            if (cfgStr.IsNull())
            {
                log.Warning("无法从配置服务，本地获取MQ应用配置，使用内存默认配置");
                cfg = MQMainConfiguration.DefaultMQCfg;
                type = CfgTypeEnum.LocalMemory;
            }
            else
            {
                cfg = cfgStr.JSONDeserializeFromString<IEnumerable<MQMainConfiguration>>().FirstOrDefault();
            }
            return Tuple.Create(cfg, type);
#endif
        }
        /// <summary>
        /// 获取指定的配置
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="notFindUseDefaultCfg"></param>
        /// <returns></returns>
        public MQMainConfiguration GetConfiguration(string appId, bool notFindUseDefaultCfg = true)
        {
            YmtSystemAssert.AssertArgumentNotEmpty(appId, "app id 为空无法获取配置");
            try
            {
                rwLock.EnterReadLock();
                MQMainConfiguration cfgInfo;
                _cacheCfg.TryGetValue(appId, out cfgInfo);
                if (cfgInfo != null)
                {
                    //检查配置属性
                    CheckConfiguration(cfgInfo, _mq_cache_default_Cfg);
                    return cfgInfo;
                }
                if (notFindUseDefaultCfg)
                    throw new KeyNotFoundException(string.Format("{0}没有对应的配置", appId));
                else
                    return _mq_cache_default_Cfg;
            }
            finally
            {
                rwLock.ExitReadLock();
            }
        }
        /// <summary>
        /// 获取指定应用下的指定消息配置
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public MessageConfiguration GetConfiguration(string appId, string code)
        {
            var msgCfg = GetConfiguration(appId).MessageCfgList.SingleOrDefault(e => e.Code == code);
            if (msgCfg == null) log.Error("appid {0},code {1} 配置不存在,无法发送消息".Fomart(appId, code));
            YmtSystemAssert.AssertArgumentNotNull(msgCfg, string.Format("应用{0}不存在对应的业务消息{1}配置", appId, code));
            return msgCfg;
        }
        public Task<MessageConfiguration> GetConfigurationAsync(string appId, string code)
        {
            var tcs = new TaskCompletionSource<MessageConfiguration>();
            try
            {
                var result = GetConfiguration(appId, code);
                tcs.SetResult(result);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
            return tcs.Task;
        }
        /// <summary>
        /// 获取所有配置。如果异常则使用默认的全局配置
        /// </summary>
        /// <param name="useDefaultCfg"></param>
        /// <returns></returns>
        public Dictionary<string, MQMainConfiguration> GetConfiguration()
        {
            try
            {
                rwLock.EnterReadLock();
                if (!SpinWait.SpinUntil(() => _cacheCfg != null, 3000))
                {
                    log.Error("MQConfiguration 配置为空");
                }
                return _cacheCfg;
            }
            catch (Exception ex)
            {
                ex.Handle(log, "获取配置异常");
                return new Dictionary<string, MQMainConfiguration> { { "default", GetDefaultConfiguration().Item1 } };
            }
            finally
            {
                rwLock.ExitReadLock();
            }
        }
#if Test
        /// <summary>
        /// 自动维护配置测试
        /// </summary>
        public void TestConfigurationMaintain()
        {
            Start();
        }
#endif
        /// <summary>
        /// 备份MQ默认应用配置
        /// </summary>    
        public void DumpMQDefaultConfigurationFile(MQMainConfiguration cfg)
        {
            var json = cfg.JSONSerializationToString();
            lock (lock_defCfg)
            {
                if (File.Exists(def_cfg_path)) File.Delete(def_cfg_path);
                FileAsync.WriteAllText(def_cfg_path, json).WithHandleException("{0}", "写入MQSystem配置错误");
            }
        }
        /// <summary>
        /// 停止
        /// </summary>
        public void Stop()
        {
            if (timer != null)
                timer.Dispose();
            isRun = false;
            log.Debug("MQ 停止自动维护配置成功");
        }
        /// <summary>
        /// 启动配置维护
        /// </summary>
        public void Start()
        {
            if (isRun) return;
            isRun = true;
            timer = new Timer(o =>
            {
                if (!isRun)
                    return;
                using (var monitor = new MethodMonitor(log, 1000, "完成一次配置维护"))
                {
                    //同步配置&更新配置
                    var t1 = Task.Factory.StartNew(() => CfgAppUpdateWork(CfgAppSyncWork()));
                    var t2 = Task.Factory.StartNew(() => CfgDefaultUpdateWork(CfgDefault_SyncWork()));
                    TryWaitall(t1, t2);
                }
                //var mqSystemCfg = MQSystemConfiguration.LoadMQSysConfiguration();
                //刷新配置时间：30 秒
                //var fulshTime = 30000;mqSystemCfg.FulshMQConfigurationTimestamp ?? 3000;
                timer.Change(30000, Timeout.Infinite);
            }, null, Timeout.Infinite, Timeout.Infinite);

            timer.Change(0, Timeout.Infinite);
            log.Debug("MQ 启动自动维护配置成功");
        }

        private void TryWaitall(Task t1, Task t2)
        {
            try
            {
                Task.WaitAll(t1, t2);
            }
            catch (AggregateException ex)
            {
                ex.Handle(log, "MQMainConfigurationManager 配置维护异常(AggregateException)");
            }
            catch (Exception ex)
            {
                ex.Handle(log, "MQMainConfigurationManager 配置维护异常(Exception)");
            }
        }

        private void CfgAppUpdateWork(Dictionary<string, MQMainConfiguration> _serverCfg)
        {
            if (_cacheCfg == null)
            {
                _cacheCfg = _serverCfg;
                return;
            }
            if (!CfgVersionCompare(_serverCfg, _cacheCfg))
            {
                return;
            }
            //执行链接池回调
            if (this.updateAction != null)
            {
                var tmpDic = new HashSet<_UpdateConnectionInfo>();
                foreach (var serverConn in _serverCfg)
                {
                    MQMainConfiguration connCfg;
                    //key 在缓存中找到则更新链接池，否则说明配置服务增加了新的链接
                    if (_cacheCfg.TryGetValue(serverConn.Key, out connCfg))
                    {
                        //检测链接配置属性变化
                        if (!serverConn.Value.ConnCfg.Equals(connCfg.ConnCfg))
                        {
                            var tmpCacheConnInfo = ConnectionInfo.Build(connCfg.ConnCfg.ConnectionString);
                            var tmpServerConnInfo = ConnectionInfo.Build(serverConn.Value.ConnCfg.ConnectionString);
                            CfgUpdateType cfgUpdateType = CfgUpdateType.ConnStringModify;
                            uint connNum = 0;
                            if (tmpCacheConnInfo.IsConnHostModify(tmpServerConnInfo))
                                cfgUpdateType = CfgUpdateType.ConnStringModify;
                            else
                            {
                                if (tmpCacheConnInfo.isAddConn(tmpServerConnInfo, out connNum))
                                    cfgUpdateType = CfgUpdateType.ConnNumAddModify;
                                else if (tmpCacheConnInfo.isDecrementConn(tmpServerConnInfo, out connNum))
                                    cfgUpdateType = CfgUpdateType.ConnNumDecrement;
                            }
                            tmpDic.Add(new _UpdateConnectionInfo { AppId = serverConn.Value.AppId, Conn = tmpServerConnInfo, ConnNum = connNum, UpType = cfgUpdateType });
                        }
                    }
                    else
                    {
                        var tmpServerConnInfo = ConnectionInfo.Build(serverConn.Value.ConnCfg.ConnectionString);
                        tmpDic.Add(new _UpdateConnectionInfo { AppId = serverConn.Value.AppId, Conn = tmpServerConnInfo, ConnNum = tmpServerConnInfo.PoolMinSize.Value, UpType = CfgUpdateType.AddNewConn });
                    }
                }
                if (tmpDic != null && tmpDic.Count >= 1)
                {
                    log.Debug("应用{0}链接属性发生变化，需要重建链接池", string.Join(",", tmpDic.Select(e => e.Conn)));
                    this.updateAction(tmpDic);
                    //Task.Factory.StartNew(e => this.updateAction, tmpDic);
                }
            }
            rwLock.EnterUpgradeableReadLock();
            try
            {
                rwLock.EnterWriteLock();
                try
                {
                    //更新配置                                  
                    _cacheCfg = _serverCfg;
                    log.Debug("获取到新配置,覆盖内存中配置,更新完成");
                }
                finally
                {
                    rwLock.ExitWriteLock();
                }
            }
            finally
            {
                rwLock.ExitUpgradeableReadLock();
            }
        }
        //配置版本比较
        private bool CfgVersionCompare(Dictionary<string, MQMainConfiguration> _serverCfg, Dictionary<string, MQMainConfiguration> localCfg)
        {
            //服务端配置项大于缓存配置项，或者服务端配置版本大于缓存版本
            return _serverCfg.Count > localCfg.Count || _serverCfg.Values.Where(s_cfg => localCfg.Values.Where(c_cfg => s_cfg.AppId == c_cfg.AppId && s_cfg.Version > c_cfg.Version).Any()).Any();
        }
        /// <summary>
        /// 检查当前配置，如果没有配置项则使用默认的全局配置
        /// </summary>
        /// <param name="currentCfg">当前配置</param>
        /// <param name="defaultCfg">默认的全局配置</param>
        private void CheckConfiguration(MQMainConfiguration currentCfg, MQMainConfiguration defaultCfg)
        {
            YmtSystemAssert.AssertArgumentNotNull(defaultCfg, "默认全局配置不能为空");
            currentCfg.NullObjectReplace(v => currentCfg = v, defaultCfg);

            currentCfg.ConnCfg.NullObjectReplace(v => currentCfg.ConnCfg = v, defaultCfg.ConnCfg);
            currentCfg.ConnCfg.ConnectionString.NullObjectReplace(v => currentCfg.ConnCfg.ConnectionString = v, defaultCfg.ConnCfg.ConnectionString);

            var defCfg = defaultCfg.MessageCfgList.First();

            foreach (var item in currentCfg.MessageCfgList)
            {
                //检查消费者配置属性
                //item.ConsumeCfg.NullObjectReplace(v => item.ConsumeCfg = v, defCfg.ConsumeCfg);
                //item.ConsumeCfg.Args.NullObjectReplace(v => item.ConsumeCfg.Args = v, defCfg.ConsumeCfg.Args);
                //item.ConsumeCfg.IsAutoAcknowledge.NullAction(v => item.ConsumeCfg.IsAutoAcknowledge = v, defCfg.ConsumeCfg.IsAutoAcknowledge ?? false);
                //item.ConsumeCfg.MaxThreadCount.NullAction(v => item.ConsumeCfg.MaxThreadCount = v, defCfg.ConsumeCfg.MaxThreadCount ?? 512);
                //item.ConsumeCfg.PrefetchCount.NullAction(v => item.ConsumeCfg.PrefetchCount = v, defCfg.ConsumeCfg.PrefetchCount ?? 500);
                //item.ConsumeCfg.UseMultipleThread.NullAction(v => item.ConsumeCfg.UseMultipleThread = v, defCfg.ConsumeCfg.UseMultipleThread);
                //item.ConsumeCfg.HandleFailAcknowledge.NullAction(v => item.ConsumeCfg.HandleFailAcknowledge = v, defCfg.ConsumeCfg.HandleFailAcknowledge);
                //item.ConsumeCfg.HandleFailRQueue.NullAction(v => item.ConsumeCfg.HandleFailRQueue = v, defCfg.ConsumeCfg.HandleFailRQueue);
                //item.ConsumeCfg.RoutingKey.NullObjectReplace(v => item.ConsumeCfg.RoutingKey = v, defCfg.ConsumeCfg.RoutingKey);
                //item.ConsumeCfg.CallbackMethodType.NullObjectReplace(v => item.ConsumeCfg.CallbackMethodType = v, defCfg.ConsumeCfg.CallbackMethodType);
                //item.ConsumeCfg.CallbackTimeOut.NullAction(v => item.ConsumeCfg.CallbackTimeOut = v, defCfg.ConsumeCfg.CallbackTimeOut);
                //item.ConsumeCfg.CallbackTimeOutAck.NullAction(v => item.ConsumeCfg.CallbackTimeOutAck = v, defCfg.ConsumeCfg.CallbackTimeOutAck);
                //item.ConsumeCfg.RetryCount.NullAction(v => item.ConsumeCfg.RetryCount = v, defCfg.ConsumeCfg.RetryCount);
                //item.ConsumeCfg.RetryMillisecond.NullAction(v => item.ConsumeCfg.RetryMillisecond = v, defCfg.ConsumeCfg.RetryMillisecond);
                //item.ConsumeCfg.HandleFailPersistentStore.NullAction(v => item.ConsumeCfg.HandleFailPersistentStore = v, defCfg.ConsumeCfg.HandleFailPersistentStore);
                //item.ConsumeCfg.HandleSuccessSendNotice.NullAction(v => item.ConsumeCfg.HandleSuccessSendNotice = v, defCfg.ConsumeCfg.HandleSuccessSendNotice);
                ////item.ConsumeCfg.ConsumeCount.NullAction(v => item.ConsumeCfg.ConsumeCount = v, defCfg.ConsumeCfg.ConsumeCount);
                //item.ConsumeCfg.HandleFailMessageToMongo.NotNullAction(v => item.ConsumeCfg.HandleFailMessageToMongo = v, defCfg.ConsumeCfg.HandleFailMessageToMongo ?? false);
                //检查发布配置属性
                item.PublishCfg.NullObjectReplace(v => item.PublishCfg = v, defCfg.PublishCfg);
                item.PublishCfg.PublisherConfirms.NullAction(v => item.PublishCfg.PublisherConfirms = v, defCfg.PublishCfg.PublisherConfirms ?? false);
                item.PublishCfg.RetryCount.NullAction(v => item.PublishCfg.RetryCount = v, defCfg.PublishCfg.RetryCount ?? 1);
                item.PublishCfg.RetryMillisecond.NullAction(v => item.PublishCfg.RetryMillisecond = v, defCfg.PublishCfg.RetryMillisecond ?? 500);
                item.PublishCfg.UseTransactionCommit.NullAction(v => item.PublishCfg.UseTransactionCommit = v, defCfg.PublishCfg.UseTransactionCommit ?? false);
                item.PublishCfg.RouteKey.NullObjectReplace(v => item.PublishCfg.RouteKey = v, defCfg.PublishCfg.RouteKey);
                //检查交换机属性
                item.ExchangeCfg.NullObjectReplace(v => item.ExchangeCfg = v, defCfg.ExchangeCfg);
                item.ExchangeCfg._ExchangeType.NullAction(v => item.ExchangeCfg._ExchangeType = v, defCfg.ExchangeCfg._ExchangeType ?? ExchangeType.direct);
                item.ExchangeCfg.Arguments.NullObjectReplace(v => item.ExchangeCfg.Arguments = v, defCfg.ExchangeCfg.Arguments);
                item.ExchangeCfg.Durable.NullAction(v => item.ExchangeCfg.Durable = v, defCfg.ExchangeCfg.Durable ?? true);
                item.ExchangeCfg.IsExchangeAutoDelete.NullAction(v => item.ExchangeCfg.IsExchangeAutoDelete = v, defCfg.ExchangeCfg.IsExchangeAutoDelete ?? false);
                //检查队列属性
                item.QueueCfg.NullObjectReplace(v => item.QueueCfg = v, defCfg.QueueCfg);
                item.QueueCfg.IsQueueExclusive.NullAction(v => item.QueueCfg.IsQueueExclusive = v, defCfg.QueueCfg.IsQueueExclusive ?? false);
                item.QueueCfg.IsAutoDelete.NullAction(v => item.QueueCfg.IsAutoDelete = v, defCfg.QueueCfg.IsAutoDelete ?? false);
                item.QueueCfg.IsDurable.NullAction(v => item.QueueCfg.IsDurable = v, defCfg.QueueCfg.IsDurable ?? true);
                item.QueueCfg.Args.NullObjectReplace(v => item.QueueCfg.Args = v, defCfg.QueueCfg.Args);
                item.QueueCfg.HeadArgs.NullObjectReplace(v => item.QueueCfg.HeadArgs = v, defCfg.QueueCfg.HeadArgs);
                //消息属性检查
                item.MessagePropertiesCfg.NullObjectReplace(v => item.MessagePropertiesCfg = v, defCfg.MessagePropertiesCfg);
                item.MessagePropertiesCfg.PersistentMessages.NullAction(v => item.MessagePropertiesCfg.PersistentMessages = v, defCfg.MessagePropertiesCfg.PersistentMessages??true);
                item.MessagePropertiesCfg.PersistentMessagesLocal.NullAction(v => item.MessagePropertiesCfg.PersistentMessagesLocal = v, defCfg.MessagePropertiesCfg.PersistentMessagesLocal??true);
                item.MessagePropertiesCfg.PersistentMessagesMongo.NullAction(v => item.MessagePropertiesCfg.PersistentMessagesMongo = v, defCfg.MessagePropertiesCfg.PersistentMessagesMongo??true);
                item.MessagePropertiesCfg.Priority.NullAction(v => item.MessagePropertiesCfg.Priority = v, defCfg.MessagePropertiesCfg.Priority);
                item.MessagePropertiesCfg.Expiration.NullAction(v => item.MessagePropertiesCfg.Expiration = v, defCfg.MessagePropertiesCfg.Expiration);
                item.MessagePropertiesCfg.ContextType.NullObjectReplace(v => item.MessagePropertiesCfg.ContextType = v, defCfg.MessagePropertiesCfg.ContextType);
                item.MessagePropertiesCfg.ContentEncoding.NullObjectReplace(v => item.MessagePropertiesCfg.ContentEncoding = v, defCfg.MessagePropertiesCfg.ContentEncoding);
            }

        }
        //同步MQAPP应用具体匹配
        private Dictionary<string, MQMainConfiguration> CfgAppSyncWork(bool isCtor = false)
        {
            //申明配置临时变量
            Dictionary<string, MQMainConfiguration> _serverCfg = null;
            //从MQ配置中心，获取配置
            var remotingCfg = RequestMQConfigurationServer(ConfigurationUri.app_Cfg);
            //log.Debug("从配置服务{0},获取到配置 {1}", ConfigurationUri.app_Cfg, remotingCfg.IsNull());
            //序列化配置
            _serverCfg = FillMQConfiguration(remotingCfg);
            //没有获取远程配置，则加载本地dump 的配置
            if (_serverCfg == null || _serverCfg.Count == 0)
            {
                log.Warning("未获取到MQ配置中心配置，加载本地dump的配置");
                _serverCfg = LoadLocalDumpConfiguration();
            }
            else
            {
                //如果只构造函数调用，则直接覆盖本地dump文件
                if (isCtor)
                {
                    DumpMQConfigurationFile(remotingCfg);
                }
                else
                {
                    if (_cacheCfg == null || !_cacheCfg.Values.Any())
                    {
                        _cacheCfg = _serverCfg;
                    }
                    else
                    {
                        if (CfgVersionCompare(_serverCfg, _cacheCfg))
                        {
                            DumpMQConfigurationFile(remotingCfg);
                            log.Debug("远程配置版本 {0} 大于本地dump配置版本 {1}，覆盖本地配置", 0, 1);
                        }
                    }
                }
            }
            //如果本地dum 配置没有获取到，则使用内存的默认配置
            if (_serverCfg == null)
            {
                log.Fatal("本地，远程配置都为空,请检查。使用默认全局配置");
                var defaultCfg = GetDefaultConfiguration().Item1;
                _serverCfg = new Dictionary<string, MQMainConfiguration> { { "default", defaultCfg } };
            }
            return _serverCfg;
        }
        //同步MQ应用默认配置
        private MQMainConfiguration CfgDefault_SyncWork(bool isCtor = false)
        {
            var cfgTuple = GetDefaultConfiguration();
            MQMainConfiguration tmp_Cfg = cfgTuple.Item1;
            if (isCtor)
            {
                if (cfgTuple.Item2 == CfgTypeEnum.Server)
                {
                    DumpMQDefaultConfigurationFile(tmp_Cfg);
                }
            }
            else
            {
                if (_mq_cache_default_Cfg == null)
                {
                    _mq_cache_default_Cfg = tmp_Cfg;
                }
                else
                {
                    CfgDefaultUpdateWork(tmp_Cfg);
                }
            }
            return tmp_Cfg;
        }
        //更新默认配置
        private void CfgDefaultUpdateWork(MQMainConfiguration serverCfg)
        {
            if (serverCfg.Version > _mq_cache_default_Cfg.Version)
            {
                _mq_cache_default_Cfg = serverCfg;
                DumpMQDefaultConfigurationFile(serverCfg);
            }
        }
        //dump MQ 配置文件
        private void DumpMQConfigurationFile(string cfgInfo)
        {
            if (cfgInfo == null) return;
            log.Debug("dump 远程获取的配置到本地磁盘");
            lock (lock_AppCfg)
            {
                //var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "MQConfig", "YamtouMQConfig.dump.Config");
                if (File.Exists(dum_cfg_path)) File.Delete(dum_cfg_path);
                FileAsync.WriteAllText(dum_cfg_path, cfgInfo).GetResult(true);
            }
        }
        //加载本地磁盘上dump的配置
        private Dictionary<string, MQMainConfiguration> LoadLocalDumpConfiguration()
        {
            //var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "MQConfig", "YamtouMQConfig.dump.Config");
            if (!File.Exists(dum_cfg_path)) return null;
            log.Debug("{0},{1}", "加载本地磁盘上的配置", dum_cfg_path);
            IEnumerable<MQMainConfiguration> cfgList = null;
            using (var fileStream = FileAsync.OpenRead(dum_cfg_path))
            using (var streamRead = new StreamReader(fileStream, Encoding.GetEncoding("utf-8")))
            {
                cfgList = Task.Factory.StartNew(() => streamRead.ReadToEnd())
                                     .ContinueWith(str => str.Result.JSONDeserializeFromString<IEnumerable<MQMainConfiguration>>(), TaskContinuationOptions.OnlyOnRanToCompletion)
                                     .GetResult(true, descript: "加载本地dump文件 {0}".Fomart(dum_cfg_path));
            }
            return Adapter(cfgList);
        }
        //填充内存配置
        private Dictionary<string, MQMainConfiguration> FillMQConfiguration(string value)
        {
            if (string.IsNullOrEmpty(value)) return null;
            Dictionary<string, MQMainConfiguration> dic = new Dictionary<string, MQMainConfiguration>();
            value.JSONDeserializeFromString<IEnumerable<MQMainConfiguration>>().EachAction(cfg => dic[cfg.AppId] = cfg);
            log.Info("从配置服务获取到{0}个配置", dic.Count);
            return dic;
        }
        //请求配置服务
        private string RequestMQConfigurationServer(string url)
        {

#if !Test 
            //test 模式，从本地加载配置
            var path = @"E:\works\ymatou\project2\Ymatou\Main\YmatouMQ2.0\Main\doc\YamtouMQConfig.config";
            //var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "MQConfig", "YamtouMQConfig.Config");
            return LoadLocalConfiguration(path);
          
#else
            //配置服务地址          
            var request = WebRequest.Create(url);
            request.Method = "GET";
            request.Timeout = 3000;
            return request.DownloadDataAsync(Encoding.GetEncoding("utf-8")).GetResult(true, descript: "请求配置服务 {0}".Fomart(url));
#endif
        }
        //加载本地配置
        private string LoadLocalConfiguration(string path)
        {
            if (!File.Exists(path))
            {
                log.Error("文件{0}不存在", path);
                return null;
            }
            using (var fileStream = FileAsync.OpenRead(path))
            using (var streamRead = new StreamReader(fileStream, Encoding.GetEncoding("utf-8")))
            {
                return Task.Factory.StartNew(() => streamRead.ReadToEnd())
                    .ContinueWith(str => str.Result, TaskContinuationOptions.ExecuteSynchronously).GetResult(true, descript: "加载本地文件 {0}".Fomart(path));
            }
        }
        private Dictionary<string, MQMainConfiguration> Adapter(IEnumerable<MQMainConfiguration> cfgList)
        {
            Dictionary<string, MQMainConfiguration> c = new Dictionary<string, MQMainConfiguration>();
            cfgList.EachAction(i =>
            {
                c[i.AppId] = i;
            });
            return c;
        }
    }
}
