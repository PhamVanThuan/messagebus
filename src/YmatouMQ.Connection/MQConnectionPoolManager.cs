using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using RabbitMQ.Client.Exceptions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing.Impl;
using RabbitMQ.Client.Impl;
using YmatouMQNet4.Configuration;
using YmatouMQ.Common.Extensions;
using YmatouMQ.Common.Utils;
using YmatouMQ.Common.Extensions._Task;
using YmatouMQ.Common;
using YmatouMQ.Common.Utils;
using YmatouMQ.Log;
using YmatouMQ.ConfigurationSync;
using _MethodMonitor = Ymatou.PerfMonitorClient.MethodMonitor;

namespace YmatouMQ.Connection
{
    /// <summary>
    /// 消息总线RabbitMQ链接管理
    /// </summary>
    public sealed class MQConnectionPoolManager : DisposableObject
    {
        private const string DEFAULT_CONN_KEY = "DEFAULT_CONN_KEY";
        private static readonly ConcurrentDictionary<string, MQServerEventListener> listEvents = new ConcurrentDictionary<string, MQServerEventListener>();
        private readonly ILog log = LogFactory.GetLogger(LogFactory._LogType, "YmatouMQ.Connection.MQConnectionPoolManager");
        private readonly ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        private readonly ConcurrentDictionary<string, List<AutorecoveringConnection>> pool = new ConcurrentDictionary<string, List<AutorecoveringConnection>>();
        private readonly ConcurrentDictionary<string, ConnectionInfo> connInfoCache = new ConcurrentDictionary<string, ConnectionInfo>();
        private readonly ConcurrentDictionary<string, IModel> channelPool = new ConcurrentDictionary<string, IModel>();        
        private readonly ConcurrentDictionary<string,ChannelPool> channelPools=new ConcurrentDictionary<string, ChannelPool>();
        private int index = 0;
       
        /// <summary>
        /// ctor MQConnectionPoolManager
        /// </summary>
        public MQConnectionPoolManager()
        {

        }
        /// <summary>
        /// 获取链接池指定appID链接大小
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public uint GetConnPoolSize(string appId, string code = null)
        {
            return Convert.ToUInt32(pool["{0}_{1}".Fomart(appId, code)].Count);
        }
        /// <summary>
        /// 初始化池（用户发布消息）
        /// </summary>
        /// <param name="appId">应用Id</param>
        /// <param name="connectionString">链接字符窜</param>
        /// <param name="notify">链接异常恢复后通知</param>
        public void InitPool(string appId, string connectionString, IConnRecoveryNotify notify)
        {
            InitPool(appId, null, connectionString, notify, false);
        }
        /// <summary>
        /// 初始化池（用于订阅消息）
        /// </summary>
        /// <param name="appId">应用Id</param>
        /// <param name="code">业务类型</param>
        /// <param name="connectionStr">连接窜</param>
        /// <param name="notify">链接异常恢复后通知</param>
        public void InitPool(string appId, string code, string connectionStr, IConnRecoveryNotify notify
            , bool useSubscribe = false,IConnShutdownNotify shutdownNotify=null)
        {
            try
            {
                rwLock.EnterWriteLock();
                log.Info("[InitPool] appid {0},code {1},connString {2}.", appId, code, connectionStr);
                var connInfo = ConnectionInfo.Build(connectionStr);
                private_initMQPool(appId, code, connInfo, notify, useSubscribe, shutdownNotify);
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }
        /// <summary>
        /// 重新构建链接池
        /// </summary>    
        /// <param name="info"></param>
        /// <param name="notify"></param>
        public void ReBuilderPool(_UpdateConnectionInfo info, IConnRecoveryNotify notify
            , bool useSubscribe = false, string appid = null, string code = null
            , IConnShutdownNotify shutdownNotify = null)
        {
            var _appId = info.AppId;
            var _code = info.Code ?? code;
            try
            {
                var key = GenerateConnKey(_appId, _code);
                log.Info("[ReBuilderPool] begin rebuilder connection pool,key:{0}.", key);
                if (info.UpType == CfgUpdateType.ConnRinit || info.UpType == CfgUpdateType.ConnStringModify)
                {
                    log.Info("[ReBuilderPool] 检测到链接配置变更{0}，需要移除原来的链接", key);
                    RmoveMQPoolItemsByAppId(key,info.Conn.Host);
                    log.Info("[ReBuilderPool] 移除{0}关联链接完成", _appId, _code);
                    private_initMQPool(_appId, _code, info.Conn, notify, useSubscribe,shutdownNotify);
                }
                else if (info.UpType == CfgUpdateType.AddNewConn)
                {
                    private_initMQPool(_appId, _code, info.Conn, notify, useSubscribe, shutdownNotify);
                    log.Info("add new connection doen,appId:{0},code:{2}",appid,code);
                }
                else
                {
                    if (info.UpType == CfgUpdateType.ConnNumAddModify)
                        AddPoolConn(key, info.Conn, info.ConnNum);
                    else
                        RemoveConnection(key, info.ConnNum);
                    log.Info("[ReBuilderPool] UpType:{0},{1}个链接", info.UpType, info.ConnNum);
                }
                log.Info("[ReBuilderPool] done,appId{0},action type:{0}", _appId, info.UpType);
            }
            catch (Exception ex)
            {
                log.Error("[ReBuilderPool] 重新构建链接失败 {0},{1}", _appId, ex.ToString());
            }
        }

        public void ChannelAction(string appId, Action<IModel> modelAction)
        {
            var chStruct = channelPools[appId].GetChannelStruct(() => GetConnection(appId));
            try
            {
                modelAction(chStruct.channel);
            }
            finally
            {
                channelPools[appId].Free(chStruct);
            }
        }

        /// <summary>
        /// 创建MQ Channel
        /// </summary>
        /// <param name="appId">应用Id</param>
        /// <param name="code">业务代码（订阅消息时使用）</param>
        /// <returns></returns>
        public IModel CreateChannel(string appId, string code = null)
        {
            var key = "{0}_{1}_{2}".Fomart(appId, code, Thread.CurrentThread.ManagedThreadId);
            IModel _channel;
            try
            {
                _channel = channelPool.GetOrAdd(key, k => CreateChannelFunction(appId, code, key).Invoke());
                //TODO:检查channel 状态
                if (_channel.IsClosed)
                {
                    _channel = channelPool.AddOrUpdate(key, k => CreateChannelFunction(appId, code, key).Invoke(),
                        (k, m) => CreateChannelFunction(appId, code, k).Invoke());
                    log.Error(
                        _channel.IsOpen
                            ? "channel status is close,retry create channel success,channel key:{0},CloseReason:{1}"
                            : "channel status is close,retry create channel fail,channel key:{0},CloseReason:{1}",
                        key, _channel.CloseReason != null ? _channel.CloseReason.ToString() : null);
                }
                return _channel;
            }
            catch (ChannelAllocationException ex)
            {
                log.Error("create channel Exception,channel number Greater than limit,channel key:{0}, error msg:{1}",
                    key, ex.ToString());
                channelPool.TryRemove(key, out _channel);
                return null;
            }
            catch (Exception ex)
            {
                log.Error("create channel,channel key:{0}, Exception {1}", key, ex.ToString());
                channelPool.TryRemove(key, out _channel);
                return null;
            }
        }

        /// <summary>
        /// 创建channel
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public Task<IModel> CreateChannelAsync(string appId, string code = null)
        {
            Func<IModel> fn = () => CreateChannel(appId, code);
            return fn.ExecuteSynchronously();
        }
        /// <summary>
        /// 创建channel
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public IModel DirectChannel(string appId, string code = null)
        {
            var poolKey = "{0}_{1}".Fomart(appId, code);
            if (!pool.ContainsKey(poolKey))
                poolKey = "{0}_".Fomart(appId);
            return GetConnection(poolKey).CreateModel();
        }
        /// <summary>
        /// 获取一个短链接
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public IConnection DirectConnection(string appId, string code = null)
        {
            log.Info("创建一个短链接 {0}", appId);
            var connInfo = GetCurrentConnectionInfo(GenerateConnKey(appId, code));
            var conn = CreateConnection(connInfo);
            log.Info("短链接创建完成");
            return conn;
        }
        /// <summary>
        /// 检查appid 是否存在pool
        /// </summary>
        /// <param name="appid"></param>
        /// <returns></returns>
        public bool CheckAppIdExists(string appid, string code = null)
        {
            return pool.ContainsKey("{0}_{1}".Fomart(appid, code));
        }
        /// <summary>
        /// 清除链接池
        /// </summary>
        public void Clear()
        {
            InternalDispose();
        }       
        public IEnumerable<string> ConnectionPoolKeys { get { return pool.Keys; } }

        public IEnumerable<string> AllChannelStatus
        {
            get
            {
                return
                    channelPool.Select(
                        ch =>
                            "channelKey:{0},active:{1},closeReason:{2}".Fomart(ch.Key, ch.Value.IsOpen,
                                ch.Value.CloseReason == null ? null : ch.Value.CloseReason.ToString())).ToList();
            }
        }

        protected override void InternalDispose()
        {
            pool.Values.SelectMany(e => e).EachAction(e =>
            {
                TryCloseMQConnectionAndChannel(e);
                log.Info("{0}链接释放", e.Endpoint.HostName);
            });
            listEvents.EachAction(e => e.Value.UnRegisterMQServerEvent());
            listEvents.Clear();
            pool.Clear();
            channelPool.Clear();
            log.Info("MQBUS应用完成链接池链接释放");
        }

        private Func<IModel> CreateChannelFunction(string appId, string code, string key)
        {
            Func<IModel> fn = () =>
            {               
                using (var mm = new MethodMonitor(null, 50)) 
                using (_MethodMonitor.New("CreateChannel"))
                {
                    var poolKey = "{0}_{1}".Fomart(appId, code);
                    if (!pool.ContainsKey(poolKey))
                        poolKey = "{0}_".Fomart(appId);
                    var conn = GetConnection(poolKey);
                    IModel channel = conn.CreateModel();
                    log.Debug(
                        "@[CreateChannelFunction] create new channel,channel pool size:{0},channel key:{1},channel number:{2},channel Max:{3},execute {4:N0} ms",
                        channelPool.Count, key,
                        channel.ChannelNumber, conn.ChannelMax, mm.GetRunTime2);
                    CheckNeedAddNewConnection(poolKey, conn, channel);
                    return channel;
                }
            };
            return fn;
        }

        //链接池增加新的链接
        private void AddPoolConn(string connkey, ConnectionInfo conn, uint num = 1)
        {
            var pool = GetPool(connkey);
            if (pool.Count >= conn.PoolMaxSize)
            {
                throw new Exception<MQException>(string.Format("应用 {0} ，链接池已到达最大上限 {1}，无法创建新的链接"
                    , connkey, conn.PoolMaxSize));
            }
            for (var i = 0; i < num; i++)
            {
                if (i < pool.Count)
                {
                    pool.Add(CreateConnection(conn));
                }
            }
        }
        private void RemoveConnection(string connkey, uint num)
        {
            var pool = GetPool(connkey);
            if (pool.Count <= 0)
            {
                log.Info("应用{0},链接池链接小于等于零，无法移除链接", connkey);
                return;
            }
            if (num > pool.Count)
            {
                log.Warning("应用{0},要移除的链接大于链接池链接无法移除", connkey);
                return;
            }
            rwLock.EnterUpgradeableReadLock();
            try
            {
                rwLock.EnterWriteLock();
                try
                {
                    for (var i = 1; i < num; i++)
                    {
                        TryCloseMQConnectionAndChannel(pool[i], connKey: connkey);
                    }
                    pool.RemoveRange(1, Convert.ToInt32(num));
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
        private string GenerateConnKey(string appid, string code)
        {
            return "{0}_{1}".Fomart(appid, code);
        }
        private void private_initMQPool(string appId, string code, ConnectionInfo connInfo, IConnRecoveryNotify notify, bool useSubscribe = false, IConnShutdownNotify shutdownNotify = null)
        {
            var key = GenerateConnKey(appId, code);
            if (pool.ContainsKey(key))
            {
                return;
            }
            connInfoCache.AddOrUpdate(key, connInfo, (k, v) => connInfo);
            //connInfoCache.TryAdd(key, connInfo);
            //如果使用链接池
            if (connInfo.UsePool && connInfo.PoolMinSize > 0)
            {
                //如果连接用于订阅，则根据appdomain ，制定连接数量配置连接，否则 根据连接字符窜指定的连接数生成连接
                if (useSubscribe)
                {
                    var domainCfg = AppdomainConfigurationManager.Builder.GetAppDomain(appId, code);
                    connInfo.SetConnPoolMinSize(domainCfg.ConnectionPoolSize);
                    CreateConnectionPool(connInfo, notify, key,shutdownNotify);
                }
                else
                {
                    CreateConnectionPool(connInfo, notify, key, shutdownNotify);
                }
            }
        }
        //创建链接池
        private void CreateConnectionPool(ConnectionInfo connInfo, IConnRecoveryNotify notify, string key, IConnShutdownNotify shutdownNotify = null)
        {
            log.Info("[CreateConnectionPool] appid:{0},connection pool init count:{1}", key, connInfo.PoolMinSize);
            var list = new List<AutorecoveringConnection>(Convert.ToInt32(connInfo.PoolMinSize.Value));
            for (var i = 0; i < connInfo.PoolMinSize; i++)
            {
                var conn = CreateConnection(connInfo);
                if (conn != null) //break;
                    list.Add(conn);
            }
            log.Info("[CreateConnectionPool] appid:{0},CreateConnection done,pool count:{1}", key, list.Count);          
            if (list.IsEmptyEnumerable())return;           
            //添加事件监听列表 
            if (!listEvents.ContainsKey(connInfo.Host))
            {
                var addOk = listEvents.TryAdd(connInfo.Host,
                    new MQServerEventListener(list.First(), notify, key, shutdownNotify));
                log.Debug("[CreateConnectionPool] MQServerEventListener Add done,host:{0},add result:{1}", connInfo.Host,
                    addOk);
            }
            //链接添加到连接池
            pool.TryAdd(key, list);
            #region
            //            //创建channel 连接池
//            var chPool=new ChannelPool(connInfo.ChannelPoolMax.Value,connInfo.ChannelIdleTimeOut);
//            var connPoolSize = pool[key].Count;
//            var poolSize = connInfo.ChannelPoolMin.Value / Convert.ToInt32(connPoolSize);
//            poolSize = poolSize <= 0 ? 1 : poolSize;
//            for (var i = 0; i < connPoolSize; i++)
//            {
//                chPool.CreateChannel(list[i], poolSize);
//            }           
//            channelPools.TryAdd(key, chPool);
//            log.Info("[CreateConnectionPool] channel poosiez:{0},key:{1}", chPool.Count,key);
#endregion
        }
      
        private void RmoveMQPoolItemsByAppId(string connKey,string listenerKey)
        {
            rwLock.EnterUpgradeableReadLock();
            try
            {
                rwLock.EnterWriteLock();
                try
                {
                    ConnectionInfo info;
                    connInfoCache.TryRemove(connKey, out info);
                    MQServerEventListener listener;
                    listEvents.TryRemove(listenerKey, out listener);
                    channelPool.Where(e => e.Key.StartsWith(connKey)).EachAction(c =>
                    {
                        IModel channel;
                        channelPool.TryRemove(c.Key, out channel);
                        channel.Close();
                    });

                    List<AutorecoveringConnection> list;
                    pool.TryRemove(connKey, out list);
                    if (list != null)
                    {
                        list.EachAction(c => TryCloseMQConnectionAndChannel(c));
                        list.Clear();
                    }
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
        //检查是否需要创建新的连接
        private void CheckNeedAddNewConnection(string connKey, IConnection conn, IModel channel)
        {
            var isNewConn = IsCreateNewConnection(conn, connKey, channel);
            if (!isNewConn) return;
           
            var _pool = GetPool(connKey);
            var cfg = GetCurrentConnectionInfo(connKey);
            if (_pool.Count < cfg.PoolMaxSize.Value)
            {
                var newConn = CreateConnection(cfg);
                _pool.Add(newConn);
                pool.TryAdd(connKey, _pool);
                log.Info("conn key:{0} channel channel max:{1},create new connection done.", connKey, cfg.ChannelMax);
            }
            else
            {
                var errStr = string.Format("conn key:{0} channel:{1},但链接池已到达最大上限 {2}", connKey, cfg.ChannelMax, cfg.PoolMaxSize);
                log.Info(errStr);
                throw new Exception<MQException>(message: errStr);
            }           
        }
        //检查是否需要创建新的连接
        private bool IsCreateNewConnection(IConnection conn, string connKey, IModel channel)
        {
            if (conn.ChannelMax > 0)
            {
                var cfg = GetCurrentConnectionInfo(connKey);
                if (cfg.ChannelMax != null && cfg.ChannelMax.HasValue)
                {
                    var _model = (channel as AutorecoveringModel);
                    if (_model.ChannelNumber == cfg.ChannelMax.Value - 1)
                    {
                        log.Info("connKey:{0},host:{1} channel 达到上限 {2}", connKey, cfg.Host, _model.ChannelNumber);
                        return true;
                    }
                }
                if (channel.ChannelNumber == conn.ChannelMax - 1)
                {
                    log.Info("connKey {0},channelNumber {1} 达到上限 {2}", connKey, channel.ChannelNumber, conn.ChannelMax);
                    return true;
                }
            }
            return false;
        }
        //根据appid 从连接池里获取一个链接
        private AutorecoveringConnection GetConnection(string connKey)
        {
            var tmp = GetPool(connKey);
            AutorecoveringConnection conn = null;
            var r = Interlocked.Increment(ref index);
            if (r == int.MaxValue)
            {
                index = 0;
                r = 0;
            }
            if (r < tmp.Count)
            {
                conn = tmp[r];
                log.Info("[GetConnection] connKey:{0}获取第{1}位置链接", connKey, r);
            }
            else
            {
                log.Info("[GetConnection] connKey:{0}获取第{1}位置链接", connKey, r % tmp.Count);
                conn = tmp[r % tmp.Count];
            }
            return conn;
        }
        //根据appId 从连接池里获取当前所有链接
        private List<AutorecoveringConnection> GetPool(string connKey)
        {

            rwLock.EnterReadLock();
            try
            {
                List<AutorecoveringConnection> tmp;
                if (!pool.TryGetValue(connKey, out tmp))
                {
                    throw new KeyNotFoundException("[GetPool] connkey:{0} 未启用链接池".Fomart(connKey));
                }
                return tmp;
            }
            finally
            {
                rwLock.ExitReadLock();
            }
        }

        //关闭channel
        private void TryCloseMQConnectionAndChannel(IConnection conn, string connKey = null, TimeSpan? timeOut = null)
        {
            try
            {
                if (timeOut != null && timeOut.HasValue)
                    conn.Close(Convert.ToInt32(timeOut.Value.TotalSeconds));
                else
                    conn.Close();
            }
            catch (Exception ex)
            {
                log.Error("[TryCloseMQConnectionAndChannel] appid:{0},close connection exception,{1}", connKey, ex);
            }
        }
        //根据appId 获取配置
        private ConnectionInfo GetCurrentConnectionInfo(string connKey)
        {
            ConnectionInfo val;
            connInfoCache.TryGetValue(connKey, out val);
            return val;
        }
        //根据配置创建新的链接
        private AutorecoveringConnection CreateConnection(ConnectionInfo connInfo)
        {
            var connFacory = new ConnectionFactory();
            connFacory.AutomaticRecoveryEnabled = connInfo.AutomaticRecoveryEnabled;
            connFacory.TopologyRecoveryEnabled = connInfo.TopologyRecoveryEnabled;
            connFacory.HostName = connInfo.Host;
            connFacory.VirtualHost = connInfo.VHost;
            connFacory.UserName = connInfo.UserNmae;
            connFacory.Password = connInfo.Password;
            connFacory.Port = connInfo.Port;
            connFacory.UseBackgroundThreadsForIO = connInfo.UseBackgroundThreads;         
            connInfo.RecoveryInterval.NullAction(v => connFacory.NetworkRecoveryInterval = v);           
            //connInfo.Heartbeat.NullAction(v => connFacory.RequestedHeartbeat = v);
            //connInfo.ChannelMax.NullAction(v => connFacory.RequestedChannelMax = v);
            connInfo.ConnTimeOut.NullAction(v => connFacory.RequestedConnectionTimeout = v);

            using (var mm = new MethodMonitor(log, 1, "[CreateConnection] {0} CreateConnection ok.".Fomart(connInfo.Host)))
            {
                try
                {
                    //create connection instance
                    var conn = (AutorecoveringConnection)connFacory.CreateConnection();
                    //
                    if (connInfo.RegMqHostMonitor)
                    {
                        var ip4Host = _Utils.GetIP4(connInfo.Host);
                        if (!ip4Host.IsEmpty())
                            conn.Init(new List<string>() {ip4Host});
                    }
                    return conn;
                }
                catch (Exception ex)
                {
                    ex.Handle(log, "[CreateConnection] error, host:{0},name:{1},pas:{2}.", connInfo.Host
                        , connInfo.UserNmae, connInfo.Password);
                    //throw;
                    return null;
                }
            }
        }
    }
}
