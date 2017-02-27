using System;
using System.Collections.Generic;
using YmatouMQNet4.Utils;
using YmatouMQNet4.Extensions;

namespace YmatouMQNet4.Connection
{
    /// <summary>
    /// MQ链接实体
    /// </summary>
    public class ConnectionInfo
    {
        /// <summary>
        /// host
        /// </summary>
        public string Host { get; private set; }
        /// <summary>
        /// port
        /// </summary>
        public int Port { get; private set; }
        /// <summary>
        /// 链接超时时间
        /// </summary>
        public int? ConnTimeOut { get; private set; }
        /// <summary>
        /// virtual host
        /// </summary>
        public string VHost { get; private set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string UserNmae { get; private set; }
        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; private set; }
        /// <summary>
        /// 心跳时间
        /// </summary>
        public ushort? Heartbeat { get; private set; }
        /// <summary>
        /// 链接恢复时间
        /// </summary>
        public TimeSpan? RecoveryInterval { get; private set; }
        /// <summary>
        /// 最大通道数量
        /// </summary>
        public ushort? ChannelMax { get; private set; }
        /// <summary>
        /// MQ是否使用后端线程维护链接
        /// </summary>
        public bool UseBackgroundThreads { get; private set; }
        /// <summary>
        /// 是否是链接池
        /// </summary>
        public bool UsePool { get; private set; }
        /// <summary>
        /// 链接池最大链接数
        /// </summary>
        public uint? PoolMaxSize { get; private set; }
        /// <summary>
        /// 链接池最小链接数
        /// </summary>
        public uint? PoolMinSize { get; private set; }
        /// <summary>
        /// 自动恢复链接
        /// </summary>
        public bool AutomaticRecoveryEnabled { get; private set; }
        /// <summary>
        /// 链接恢复时自动恢复（交换机，队列）
        /// </summary>
        public bool TopologyRecoveryEnabled { get; private set; }
        /// <summary>
        /// 是否重新构建链接
        /// </summary>
        public bool IsRBuildConn { get; private set; }
        private ConnectionInfo() { }

        /// <summary>
        /// MQ 链接字符窜解析
        /// </summary>
        /// <param name="connection">格式：host=x.x.x.x;port:xx;vHost=/;uNmae=guest;pas=guest;heartbeat=xx;recoveryInterval=5;channelMax=100;useBackgroundThreads=true;connTimeOut=3000;pooSize=10;usepool=true</param>
        /// <returns></returns>
        public static ConnectionInfo Build(string connection)
        {
            YmtSystemAssert.AssertArgumentNotEmpty(connection, "链接字符窜不能为空");
            var dic = ParseToDictionary(connection);
            YmtSystemAssert.AssertArgumentNotNull(dic, "链接字符解析错误");

            var host = dic.TryGetVal("host", null);
            YmtSystemAssert.AssertArgumentNotEmpty(host, "请指定host");

            var port = dic.TryGetVal("port", "5672").ToInt32(5672);
            var vHost = dic.TryGetVal("vhost", "/");
            var uName = dic.TryGetVal("uname", "guest");
            var pas = dic.TryGetVal("pas", "guest");
            var heartbeat = dic.TryGetVal("heartbeat", null).ToUshort(null);
            var recoveryInterval = dic.TryGetVal("recoveryInterval", null).ToTimeSpan(null);
            // var channelMax = dic.TryGetVal("channelmax", null).ToUshort(ushort.MaxValue);
            var useBackgroundThreads = dic.TryGetVal("usebackgroundthreads", "true").ToBoole(true);
            var connTimeOut = dic.TryGetVal("conntimeout", null).ToInt32(null);
            var poolMaxSize = dic.TryGetVal("poolmaxsize", "10").ToUInt32(10) ?? null;
            var poolMinSize = dic.TryGetVal("poolminsize", "3").ToUInt32(3) ?? null;
            var automaticRecoveryEnabled = dic.TryGetVal("automaticrecovery", "true").ToBoole(true);
            var topologyRecoveryEnabled = dic.TryGetVal("topologyrecovery", "true").ToBoole(true);
            var usepool = poolMinSize.Value > 0 || poolMaxSize.Value > 0;
            var isRbuildConn = dic.TryGetVal("rconn", "false").ToBoole(false);

            var connInfo = new ConnectionInfo
            {
                Host = host,
                Port = port,
                VHost = vHost,
                UserNmae = uName,
                Password = pas,
                Heartbeat = heartbeat,
                //ChannelMax = channelMax,
                UseBackgroundThreads = useBackgroundThreads,
                RecoveryInterval = recoveryInterval,
                ConnTimeOut = connTimeOut,
                PoolMaxSize = poolMaxSize,
                PoolMinSize = poolMinSize,
                UsePool = usepool,
                AutomaticRecoveryEnabled = automaticRecoveryEnabled,
                TopologyRecoveryEnabled = topologyRecoveryEnabled,
                IsRBuildConn = isRbuildConn
            };

            return connInfo;
        }
        public bool isRbuildConn()
        {
            return this.IsRBuildConn;
        }
        public bool isAddConn(ConnectionInfo info, out uint num)
        {
            num = info.PoolMinSize.Value - this.PoolMinSize.Value;
            return this.PoolMinSize < info.PoolMinSize;
        }
        public bool isDecrementConn(ConnectionInfo info, out uint num)
        {
            num = this.PoolMinSize.Value - info.PoolMinSize.Value;
            return this.PoolMinSize > info.PoolMinSize;
        }
        public bool IsConnHostModify(ConnectionInfo info)
        {
            return this.Host != info.Host || this.Port != info.Port;
        }
        public void SetConnPoolMinSize(uint size)
        {
            if (size <= 0)
                this.PoolMinSize = 3;
            else
                this.PoolMinSize = size;
        }
        //帮助方法
        private static Dictionary<string, string> ParseToDictionary(string connection)
        {
            YmtSystemAssert.AssertArgumentNotEmpty(connection, "链接字符窜不能为空");
            var connArray = connection.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            Dictionary<string, string> connDic = new Dictionary<string, string>();
            foreach (var item in connArray)
            {
                var itemArray = item.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                var key = itemArray[0].ToLower();
                var value = itemArray[1];
                connDic[key] = value;
            }
            return connDic;
        }
    }
}
