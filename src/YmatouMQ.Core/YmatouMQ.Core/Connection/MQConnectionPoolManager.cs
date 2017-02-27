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
using RabbitMQ.Client.Impl;
using YmatouMQNet4.Configuration;
using YmatouMQNet4.Extensions;
using YmatouMQNet4.Utils;
using YmatouMQNet4.Extensions._Task;

namespace YmatouMQNet4.Connection
{
    /// <summary>
    /// 链接池管理
    /// </summary>
    [Serializable]
    public sealed class MQConnectionPoolManager : DisposableObject
    {
        private readonly ILog log = LogFactory.GetLogger(LogFactory._LogType, "YmatouMQ.Connection.MQConnectionPoolManager");
        private readonly ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        private readonly ConcurrentDictionary<string, List<IConnection>> pool = new ConcurrentDictionary<string, List<IConnection>>();
        private readonly ConcurrentDictionary<string, ConnectionInfo> connInfoCache = new ConcurrentDictionary<string, ConnectionInfo>();
        private readonly ConcurrentDictionary<string, IModel> channelPool = new ConcurrentDictionary<string, IModel>();
        private readonly ConcurrentDictionary<string, MQServerEventListener> listEvents = new ConcurrentDictionary<string, MQServerEventListener>();
        private int index = 0;

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
            return Convert.ToUInt32(pool[appId].Count);
        }
        /// <summary>
        /// 初始化池（用户发布消息）
        /// </summary>
        /// <param name="appId">应用Id</param>
        /// <param name="connectionStr">链接字符窜</param>
        /// <param name="notify">链接异常恢复后通知</param>
        public void InitPool(string appId, string connectionStr, IConnRecoveryNotify notify)
        {
            InitPool(appId, null, connectionStr, notify, false);
        }
        /// <summary>
        /// 初始化池（用于订阅消息）
        /// </summary>
        /// <param name="appId">应用Id</param>
        /// <param name="code">业务类型</param>
        /// <param name="connectionStr">连接窜</param>
        /// <param name="notify">链接异常恢复后通知</param>
        public void InitPool(string appId, string code, string connectionStr, IConnRecoveryNotify notify, bool useSubscribe = false)
        {
            try
            {
                var connInfo = ConnectionInfo.Build(connectionStr);
                rwLock.EnterWriteLock();
                private_initMQPool(appId, code, connInfo, notify, useSubscribe);
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
        public void ReBuilderPool(_UpdateConnectionInfo info, IConnRecoveryNotify notify)
        {
            try
            {
                var key = GenerateConnKey(info.AppId, info.Code);
                log.Debug("开始重新构建{0}关联链接", key);
                if (info.UpType == CfgUpdateType.ConnRinit || info.UpType == CfgUpdateType.ConnStringModify)
                {
                    log.Debug("检测到链接配置变更{0}，需要移除原来的链接", key);
                    RmoveMQPoolItemsByAppId(key);
                    log.Debug("移除{0}关联链接完成", info.AppId, info.Code);
                    private_initMQPool(info.AppId, info.Conn, notify);
                }
                else if (info.UpType == CfgUpdateType.AddNewConn)
                {
                    private_initMQPool(info.AppId, info.Conn, notify);
                }
                else
                {
                    if (info.UpType == CfgUpdateType.ConnNumAddModify)
                        AddPoolConn(key, info.Conn, info.ConnNum);
                    else
                        RemoveConnection(key, info.ConnNum);
                    log.Debug("{0}个链接", info.UpType, info.ConnNum);
                }
                log.Debug("重新构建{0}关联链接完成", info.AppId);
            }
            catch (Exception ex)
            {
                log.Error("重新构建链接失败 {0},{1}", info.AppId, ex.ToString());
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
                Func<IModel> fn = () =>
                {
                    var conn = GetConnection("{0}_{1}".Fomart(appId, code));
                    IModel channel = conn.CreateModel();
                    log.Debug("key {0} 创建一个channel ", key);
                    PoolAddNewConnection("{0}_{1}".Fomart(appId, code), conn, channel);
                    return channel;
                };
                _channel = channelPool.GetOrAdd(key, k => fn());
                //todo:检查channel 状态
                if (_channel.IsClosed)
                {
                    IModel channel;
                    channelPool.TryRemove(key, out channel);
                }
                _channel = channelPool.GetOrAdd(key, k => fn());
                return _channel;
            }
            catch (ChannelAllocationException ex)
            {
                log.Error("创建MQ channel 异常,channel max已达到上限 , error msg {0}", ex.ToString());
                channelPool.TryRemove(key, out _channel);
                return null;
            }
            catch (Exception ex)
            {
                log.Error("创建MQ channel 异常 {0}", ex.ToString());
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
        /// 获取一个短链接
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public IConnection DirectConnection(string appId, string code = null)
        {
            log.Debug("创建一个短链接 {0}", appId);
            var connInfo = GetCurrentConnectionInfo(GenerateConnKey(appId, code));
            var conn = CreateConnection(connInfo);
            log.Debug("短链接创建完成");
            return conn;
        }
        /// <summary>
        /// 清除链接池
        /// </summary>
        public void Clear()
        {
            InternalDispose();
        }
        protected override void InternalDispose()
        {
            pool.Values.SelectMany(e => e).EachAction(e =>
            {
                TryCloseMQConnectionAndChannel(e);
                log.Debug("{0}链接释放", e.Endpoint.HostName);
            });
            listEvents.EachAction(e => e.Value.UnRegisterMQServerEvent());
            listEvents.Clear();
            pool.Clear();
            channelPool.Clear();
            log.Debug("MQBUS应用完成链接池链接释放");
        }
        //链接池增加新的链接
        private void AddPoolConn(string connkey, ConnectionInfo conn, uint num = 1)
        {
            var pool = GetPool(connkey);
            if (pool.Count >= conn.PoolMaxSize)
            {
                throw new Exception<MQException>(string.Format("应用 {0} ，链接池已到达最大上限 {1}，无法创建新的链接", connkey, conn.PoolMaxSize));
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
                log.Warning("应用{0},链接池链接小于等于零，无法移除链接", connkey);
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
        private void private_initMQPool(string appId, string code, ConnectionInfo connInfo, IConnRecoveryNotify notify, bool useSubscribe = false)
        {
            var key = GenerateConnKey(appId, code);
            if (pool.ContainsKey(key))
            {
                return;
            }
            connInfoCache.TryAdd(key, connInfo);
            //如果使用链接池
            if (connInfo.UsePool && connInfo.PoolMinSize > 0)
            {
                //如果连接用于订阅，则根据appdomain ，制定连接数量配置连接，否则 根据连接字符窜指定的连接数生成连接
                if (useSubscribe)
                {
                    var domainCfg = AppdomainConfigurationManager.Builder.GetAppDomain(appId, code);
                    connInfo.SetConnPoolMinSize(domainCfg.ConnectionPoolSize);
                    CreateConnectionPool(connInfo, notify, key);
                }
                else
                {
                    CreateConnectionPool(connInfo, notify, key);
                }
            }
        }

        private void CreateConnectionPool(ConnectionInfo connInfo, IConnRecoveryNotify notify, string key)
        {
            log.Debug("appid:{0},初始化链接池数量:{1}", key, connInfo.PoolMinSize);
            var list = new List<IConnection>(Convert.ToInt32(connInfo.PoolMinSize.Value));
            for (var i = 0; i < connInfo.PoolMinSize; i++)
            {
                var conn = CreateConnection(connInfo);
                list.Add(conn);
            }
            //添加事件监听列表
            listEvents.TryAdd(key, new MQServerEventListener(list.First(), notify, key));
            //链接添加到连接池
            pool.TryAdd(key, list);
            log.Debug("appid:{0},链接池创建完成", key);
        }
        private void private_initMQPool(string appId, ConnectionInfo connInfo, IConnRecoveryNotify notify)
        {
            private_initMQPool(appId, null, connInfo, notify, false);
        }
        private void RmoveMQPoolItemsByAppId(string connKey)
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
                    listEvents.TryRemove(connKey, out listener);
                    channelPool.Where(e => e.Key.StartsWith(connKey)).EachAction(c =>
                    {
                        IModel channel;
                        channelPool.TryRemove(c.Key, out channel);
                        channel.Close();
                    });

                    List<IConnection> list;
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
        //创建新的连接
        private void PoolAddNewConnection(string connKey, IConnection conn, IModel channel)
        {
            var isNewConn = IsCreateNewConnection(conn, connKey, channel);
            if (isNewConn)
            {
                var _pool = GetPool(connKey);
                var cfg = GetCurrentConnectionInfo(connKey);
                if (_pool.Count < cfg.PoolMaxSize.Value)
                {
                    var newConn = CreateConnection(cfg);
                    _pool.Add(newConn);
                    pool.TryAdd(connKey, _pool);
                    log.Debug("链接{0} channel 数量到达上限 {1},创建一个新的链接完成", cfg.Host, cfg.ChannelMax);
                }
                else
                {
                    var errStr = string.Format("链接{0} channel 数量到达上限 {1},但链接池已到达最大上限 {2}", cfg.Host, cfg.ChannelMax, cfg.PoolMaxSize);
                    log.Debug(errStr);
                    throw new Exception<MQException>(message: errStr);
                }
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
                    log.Debug("{0}当前链接channel数量{1}", connKey, _model.ChannelNumber);
                    if (_model.ChannelNumber == cfg.ChannelMax.Value - 1)
                    {
                        log.Debug(string.Format("appid:{0},host:{1} channel 达到上限 {2}", connKey, cfg.Host, _model.ChannelNumber));
                        return true;
                    }
                }
            }
            return false;
        }
        //根据appid 从连接池里获取一个链接
        private IConnection GetConnection(string connKey)
        {
            var tmp = GetPool(connKey);
            IConnection conn = null;
            var r = Interlocked.Increment(ref index);
            if (r == int.MaxValue)
            {
                index = 0;
                r = 0;
            }
            if (r < tmp.Count)
            {
                conn = tmp[r];
                log.Debug("{0}获取第{1}位置链接", connKey, r);
            }
            else
            {
                log.Debug("{0}获取第{1}位置链接", connKey, r % tmp.Count);
                conn = tmp[r % tmp.Count];
            }
            return conn;
        }
        //根据appId 从连接池里获取当前所有链接
        private List<IConnection> GetPool(string connKey)
        {
            rwLock.EnterReadLock();
            try
            {
                List<IConnection> tmp;
                if (!pool.TryGetValue(connKey, out tmp))
                {
                    throw new KeyNotFoundException("{0} 未启用链接池".Fomart(connKey));
                }
                return tmp;
            }
            finally
            {
                rwLock.ExitReadLock();
            }
        }
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
                log.Error("应用{0}关闭链接异常{1}", connKey, ex);
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
        private IConnection CreateConnection(ConnectionInfo connInfo)
        {
            var connFacory = new ConnectionFactory();
            connFacory.AutomaticRecoveryEnabled = connInfo.AutomaticRecoveryEnabled;
            connFacory.TopologyRecoveryEnabled = connInfo.TopologyRecoveryEnabled;
            connFacory.HostName = connInfo.Host;
            connFacory.VirtualHost = connInfo.VHost;
            connFacory.UserName = connInfo.UserNmae;
            connFacory.Password = connInfo.Password;
            connFacory.UseBackgroundThreadsForIO = connInfo.UseBackgroundThreads;
            connInfo.RecoveryInterval.NullAction(v => connFacory.NetworkRecoveryInterval = v);
            connInfo.Heartbeat.NullAction(v => connFacory.RequestedHeartbeat = v);
            //connInfo.ChannelMax.NullAction(v => connFacory.RequestedChannelMax = v);
            connInfo.ConnTimeOut.NullAction(v => connFacory.RequestedConnectionTimeout = v);

            try
            {
                return connFacory.CreateConnection();
            }
            catch (Exception ex)
            {
                ex.Handle(log, "CreateConnection error {0}", connInfo.Host);
                throw;
            }
        }
    }
}
