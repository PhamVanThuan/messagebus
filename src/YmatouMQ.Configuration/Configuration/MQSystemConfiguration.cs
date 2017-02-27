using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using YmatouMQ.Common.Extensions._Task;
using YmatouMQ.Common.Extensions.Serialization;

namespace YmatouMQNet4.Configuration
{
    /// <summary>
    /// 码头ＭＱ框架配置
    /// </summary>
    [DataContract(Name = "MQCfg")]
    [Obsolete("can't use")]
    public class MQSystemConfiguration
    {
        /// <summary>
        /// 应用Id
        /// </summary>
        [DataMember(Name = "id")]
        public string AppId { get; private set; }
        /// <summary>
        /// 刷新MQ配置时间戳（毫秒）
        /// </summary>
        [DataMember(Name = "fulshCfg")]
        public int? FulshMQConfigurationTimestamp { get; private set; }
        /// <summary>
        /// 本地日志定时刷新时间（秒），不设置则实时写入日志
        /// </summary>
        [DataMember(Name = "fulshLog")]
        public int? FulshLogTimestamp { get; private set; }
        /// <summary>
        /// 日志文件大小（M）
        /// </summary>
        [DataMember(Name = "logSize")]
        public int? LogSize { get; private set; }
        /// <summary>
        /// 是否启用发布消息执行耗时
        /// </summary>
        [DataMember(Name = "trackPub")]
        public bool? EnableTrackPubRunTime { get; private set; }
        /// <summary>
        /// 是否启用订阅消息执行耗时
        /// </summary>
        [DataMember(Name = "trackSub")]
        public bool? EnableTrackSubRunTime { get; private set; }
        /// <summary>
        /// 链接异常断开时，消息是否进入本地队列
        /// </summary>
        [DataMember(Name = "csMLE")]
        public bool? ConnShutdownMessageLocalEnqueue { get; private set; }
        /// <summary>
        /// 异步发送消息最大线程数量（默认为CPU数减1）
        /// </summary>
        [DataMember(Name = "maxThPub")]
        public int? MaxThreadPublishAsync { get; private set; }
        /// <summary>
        /// 日志文件路径
        /// </summary>
        [DataMember(Name = "logPath")]
        public string LogFilePath { get; private set; }
        /// <summary>
        /// 异步发送消息内存队列大小（未使用）
        /// </summary>
        [DataMember(Name = "mqlimit")]
        public int PubMessageMemeoryQueueLimit { get; private set; }
        [DataMember(Name = "dlog")]
        public bool DebugLogEnable { get; private set; }
        [DataMember(Name = "elog")]
        public bool ErrorLogEnable { get; private set; }
        [DataMember(Name = "ilog")]
        public bool InfoLogEnable { get; private set; }
        private static string cfgPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "YamtouMQSystem.Config");

        private static readonly MQSystemConfiguration mqCfg = new MQSystemConfiguration
        {
            AppId = "mqsys",
            LogSize = 5,
            EnableTrackPubRunTime = false,
            EnableTrackSubRunTime = false,
            FulshLogTimestamp = 30,
            FulshMQConfigurationTimestamp = 3000,
            MaxThreadPublishAsync = Environment.ProcessorCount - 1,
            ConnShutdownMessageLocalEnqueue = true,
            LogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log\\"),
            PubMessageMemeoryQueueLimit = 300000,
            DebugLogEnable = true,
            ErrorLogEnable = true,
            InfoLogEnable = true
        };

        public static MQSystemConfiguration DefaultCfg { get { return mqCfg; } }

        public MQSystemConfiguration() { }
        /// <summary>
        /// 初始化配置
        /// </summary>
        /// <param name="fulshMQConfigurationTimestamp"> 刷新MQ配置时间戳（毫秒）</param>
        /// <param name="fulshLogTimestamp">定时刷新日志时间（秒）</param>
        /// <param name="enableTrackPubRunTime">是否启用发布消息执行耗时</param>
        /// <param name="enableTrackSubRunTime">是否启用订阅消息执行耗时</param>
        public MQSystemConfiguration(int? fulshMQConfigurationTimestamp
            , int? fulshLogTimestamp
            , int? logSize
            , bool enableTrackPubRunTime
            , bool enableTrackSubRunTime
            , int maxThreadPublishAsync
            , bool connShutdownMessageLocalEnqueue
            , string logfilePath)
        {
            this.FulshMQConfigurationTimestamp = fulshMQConfigurationTimestamp;
            this.FulshLogTimestamp = fulshLogTimestamp;
            this.LogSize = logSize;
            this.EnableTrackPubRunTime = enableTrackPubRunTime;
            this.EnableTrackSubRunTime = enableTrackSubRunTime;
            this.MaxThreadPublishAsync = maxThreadPublishAsync > Environment.ProcessorCount ? Environment.ProcessorCount - 1 : maxThreadPublishAsync;
            this.ConnShutdownMessageLocalEnqueue = connShutdownMessageLocalEnqueue;
            this.LogFilePath = logfilePath;
            this.AppId = "mqsys";
        }

        /// <summary>
        /// 保存配置
        /// </summary>
        public void SaveCfg()
        {
            if (this == null) return;
            FileAsync.WriteAllText(cfgPath, this.JSONSerializationToString()).IgnoreExceptions();
        }
        public static MQSystemConfiguration GetMQSysConfiguration()
        {
            return DefaultCfg;
        }
        /// <summary>
        /// 加载MQ系统配置
        /// </summary>
        /// <returns></returns>
        public static MQSystemConfiguration LoadMQSysConfiguration()
        {
            if (!File.Exists(cfgPath)) return DefaultCfg;
            using (var fileStream = FileAsync.OpenRead(cfgPath))
            using (var streamRead = new StreamReader(fileStream, Encoding.GetEncoding("utf-8")))
            {
                return Task.Factory.StartNew(() => streamRead.ReadToEnd())
                                      .ContinueWith(str => str.Result.JSONDeserializeFromString<MQSystemConfiguration>(), TaskContinuationOptions.OnlyOnRanToCompletion)
                                      .GetResultSync(true,null) ?? DefaultCfg;
            }
        }

    }

}
