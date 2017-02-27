using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using RabbitMQ.Client;
using YmatouMQ.Common;
using YmatouMQ.Common.Utils;
using YmatouMQNet4.Configuration;
using YmatouMQ.ConfigurationSync;
using YmatouMQ.Log;

namespace YmatouMQNet4.Core
{
    class MQDeclareCache
    {
        private readonly ConcurrentDictionary<string, byte> exchange = new ConcurrentDictionary<string, byte>();
        private readonly ILog log = LogFactory.GetLogger(LogEngineType.RealtimelWriteFile, "YmatouMQ.Core.MQDeclareCache");
        /// <summary>
        /// 声明交换机，如果channel为空或者Closed，则忽略
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="appId"></param>
        /// <param name="code"></param>
        public void EnsureExchangeDeclare(IModel channel, string appId, string code)
        {
            if (channel == null || channel.IsClosed) return;
            if (exchange.ContainsKey(_keyBuilder(appId, code, "dec")))return;
            exchange.GetOrAdd(_keyBuilder(appId, code, "dec"), key =>
            {
                var cfg = MQMainConfigurationManager.Builder.GetConfiguration(appId, code).ExchangeCfg;
                using (var localMM = new MethodMonitor(null, 1))
                {
                    channel.ExchangeDeclare(cfg.ExchangeName, cfg._ExchangeType.Value.ToString(), cfg.Durable.Value,
                        cfg.IsExchangeAutoDelete.Value, cfg.Arguments);
                    log.Info("[EnsureExchangeDeclare] run:{0:N0} ms,appid:{1},code:{2}",localMM.GetRunTime2,appId,code);
                }
                return 1;
            });
        }
        /// <summary>
        /// 声明队列，如果channel为空或者Closed，则忽略
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="appId"></param>
        /// <param name="code"></param>
        public void EnsureQueueDeclare(IModel channel, string appId, string code)
        {
            if (channel == null || channel.IsClosed) return;
            exchange.GetOrAdd(_keyBuilder(appId, code, "queue"), key =>
            {
                var cfg = MQMainConfigurationManager.Builder.GetConfiguration(appId, code);

                channel.QueueDeclare(cfg.QueueCfg.QueueName, cfg.QueueCfg.IsDurable.Value,
                    cfg.QueueCfg.IsQueueExclusive.Value, cfg.QueueCfg.IsAutoDelete.Value, cfg.QueueCfg.Args);

                return 1;
            });
        }
        /// <summary>
        /// 绑定交换机，如果channel为空或者Closed，则忽略
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="appId"></param>
        /// <param name="code"></param>
        /// <param name="target"></param>
        /// <param name="routKey"></param>
        public void EnsureExchangeBind(IModel channel, string appId, string code, string target, string routKey)
        {
            if (channel == null || channel.IsClosed) return;
            exchange.GetOrAdd(_keyBuilder(appId, code, "bind"), key =>
            {
                var cfg = MQMainConfigurationManager.Builder.GetConfiguration(appId, code).ExchangeCfg;
                channel.ExchangeBind(target, cfg.ExchangeName, routKey);
                return 1;
            });
        }

        public void RemoveQueueCache(string appid, string code)
        {
            byte b;
            exchange.TryRemove(_keyBuilder(appid, code, "queue"), out b);
        }

        public bool RemoveExchangeCache(string appid, string code)
        {
            byte b;
            return exchange.TryRemove(_keyBuilder(appid, code, "dec"), out b);
        }

        public void Clear()
        {
            exchange.Clear();
        }

        private string _keyBuilder(string appid, string code, string type)
        {
            return string.Format("{0}_{1}_{2}", appid, code, type);
        }
    }
}
