using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using RabbitMQ.Client;
using YmatouMQNet4.Configuration;

namespace YmatouMQNet4.Core
{
    class MQDeclareCache
    {
        private readonly ConcurrentDictionary<string, byte> exchange = new ConcurrentDictionary<string, byte>();

        public void EnsureExchangeDeclare(IModel channel, string appId, string code)
        {
            exchange.GetOrAdd(_keyBuilder(appId, code, "dec"), key =>
            {
                var cfg = MQMainConfigurationManager.Builder.GetConfiguration(appId, code).ExchangeCfg;

                channel.ExchangeDeclare(cfg.ExchangeName, cfg._ExchangeType.Value.ToString(), cfg.Durable.Value, cfg.IsExchangeAutoDelete.Value, cfg.Arguments);

                return 1;
            });
        }
        public void EnsureQueueDeclare(IModel channel, string appId, string code)
        {
            exchange.GetOrAdd(_keyBuilder(appId, code, "queue"), key =>
            {
                var cfg = MQMainConfigurationManager.Builder.GetConfiguration(appId, code);

                channel.QueueDeclare(cfg.QueueCfg.QueueName, cfg.QueueCfg.IsDurable.Value, cfg.QueueCfg.IsQueueExclusive.Value, cfg.QueueCfg.IsAutoDelete.Value, cfg.QueueCfg.Args);

                return 1;
            });
        }
        public void EnsureExchangeBind(IModel channel, string appId, string code, string target, string routKey)
        {
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
        public void RemoveExchangeCache(string appid, string code)
        {
            byte b;
            exchange.TryRemove(_keyBuilder(appid, code, "dec"), out b);
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
