using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using YmatouMQNet4.Configuration;
using YmatouMQ.Common.Extensions;
using YmatouMQNet4.Utils;
using System.Web.Mvc;

namespace YmatouMQAdmin.WebApp2.Models
{
    public class BusConfigurationModel
    {
        public static void SetExchangCfg(ExchangeConfiguration targetCfg, MQConfigurationDto viewModelCfg)
        {
            if (viewModelCfg == null
               || viewModelCfg.MessageCfgList == null
               || viewModelCfg.MessageCfgList.ExchangeCfg == null) return;
            targetCfg.ExchangeName = viewModelCfg.MessageCfgList.ExchangeCfg.ExchangeName;
            targetCfg.Durable = viewModelCfg.MessageCfgList.ExchangeCfg.Durable;
            targetCfg.IsExchangeAutoDelete = viewModelCfg.MessageCfgList.ExchangeCfg.IsExchangeAutoDelete;
            targetCfg._ExchangeType = viewModelCfg.MessageCfgList.ExchangeCfg._ExchangeType;
            //targetCfg.Arguments = viewModelCfg.MessageCfgList.ExchangeCfg.Arguments;
        }
        public static void SetQueryCfg(QueueConfiguration targetCfg, MQConfigurationDto viewModelCfg)
        {
            if (viewModelCfg == null
                || viewModelCfg.MessageCfgList == null
                || viewModelCfg.MessageCfgList.QueueCfg == null) return;

            targetCfg.QueueName = viewModelCfg.MessageCfgList.QueueCfg.QueueName;
            targetCfg.IsDurable = viewModelCfg.MessageCfgList.QueueCfg.IsDurable;
            targetCfg.IsAutoDelete = viewModelCfg.MessageCfgList.QueueCfg.IsAutoDelete;
            //targetCfg.IsQueueExclusive = viewModelCfg.MessageCfgList.QueueCfg.IsQueueExclusive;
            //targetCfg.HeadArgs = viewModelCfg.MessageCfgList.QueueCfg.HeadArgs;
        }
        public static void SetCodeEnable(MessageConfiguration targetCfg, MQConfigurationDto viewModelCfg)
        {
            if (viewModelCfg == null || viewModelCfg.MessageCfgList == null)
                return;
            targetCfg.Enable = viewModelCfg.MessageCfgList.Enable;
        }
        public static void SetConnCfg(MQMainConfiguration targetCfg, MQConfigurationDto viewModelCfg)
        {
            if (viewModelCfg == null) return;
            targetCfg.OwnerHost = viewModelCfg.owerhost;
            targetCfg.ConnCfg.ConnectionString = GetConnectionString(viewModelCfg.host, viewModelCfg.port, viewModelCfg.vhost
                , viewModelCfg.userName, viewModelCfg.password, viewModelCfg.channelPool, viewModelCfg.connectionPool);
        }
        public static void SetVersion(MQMainConfiguration targetCfg, int IncrementVersion = 1)
        {
            if (IncrementVersion <= 0) throw new IndexOutOfRangeException("IncrementVersion 必须大于等于1");
            targetCfg.Version += IncrementVersion;
        }
        public static ConsumeConfiguration SetConsumeCfg(ConsumeConfiguration targetCfg, MQConfigurationDto viewModelCfg)
        {
            if (viewModelCfg == null
               || viewModelCfg.MessageCfgList == null
               || viewModelCfg.MessageCfgList.ConsumeCfg == null) return null;

            targetCfg = targetCfg ?? new ConsumeConfiguration();
            targetCfg.RoutingKey = viewModelCfg.MessageCfgList.ConsumeCfg.RoutingKey.ReplaceStringTarget("未设置");
            targetCfg.IsAutoAcknowledge = viewModelCfg.MessageCfgList.ConsumeCfg.IsAutoAcknowledge;
            targetCfg.HandleFailAcknowledge = viewModelCfg.MessageCfgList.ConsumeCfg.HandleFailAcknowledge;
            targetCfg.PrefetchCount = viewModelCfg.MessageCfgList.ConsumeCfg.PrefetchCount;
            targetCfg.MaxThreadCount = viewModelCfg.MessageCfgList.ConsumeCfg.MaxThreadCount;
            targetCfg.RetryTimeOut = viewModelCfg.MessageCfgList.ConsumeCfg.RetryTimeOut;
            targetCfg.HandleSuccessSendNotice = viewModelCfg.MessageCfgList.ConsumeCfg.HandleSuccessSendNotice;
            return targetCfg;
        }
        public static PublishConfiguration SetPublishCfg(PublishConfiguration targetCfg, MQConfigurationDto viewModelCfg)
        {
            if (viewModelCfg == null
                || viewModelCfg.MessageCfgList == null
                || viewModelCfg.MessageCfgList.PublishCfg == null) return null;
            targetCfg = targetCfg ?? new PublishConfiguration();
            targetCfg.PublisherConfirms = viewModelCfg.MessageCfgList.PublishCfg.PublisherConfirms;
            targetCfg.RouteKey = viewModelCfg.MessageCfgList.PublishCfg.RouteKey.ReplaceStringTarget("未设置");
            targetCfg.RetryCount = viewModelCfg.MessageCfgList.PublishCfg.RetryCount;
            targetCfg.RetryMillisecond = viewModelCfg.MessageCfgList.PublishCfg.RetryMillisecond;
            return targetCfg;
        }
        public static MessagePropertiesConfiguration SetMessagePropertiesCfg(MessagePropertiesConfiguration targetCfg
            , MQConfigurationDto viewModelCfg)
        {
            if (viewModelCfg == null
                || viewModelCfg.MessageCfgList == null
                || viewModelCfg.MessageCfgList.MessagePropertiesCfg == null) return null;
            targetCfg = targetCfg ?? new MessagePropertiesConfiguration();
            targetCfg.ContextType = viewModelCfg.MessageCfgList.MessagePropertiesCfg.ContextType.ReplaceStringTarget("未设置");
            targetCfg.ContentEncoding = viewModelCfg.MessageCfgList.MessagePropertiesCfg.ContentEncoding.ReplaceStringTarget("未设置");
            targetCfg.Expiration = viewModelCfg.MessageCfgList.MessagePropertiesCfg.Expiration;
            targetCfg.PersistentMessages = viewModelCfg.MessageCfgList.MessagePropertiesCfg.PersistentMessages;
            targetCfg.PersistentMessagesLocal = viewModelCfg.MessageCfgList.MessagePropertiesCfg.PersistentMessagesLocal;
            targetCfg.PersistentMessagesMongo = viewModelCfg.MessageCfgList.MessagePropertiesCfg.PersistentMessagesMongo;
            targetCfg.Priority = viewModelCfg.MessageCfgList.MessagePropertiesCfg.Priority;
            return targetCfg;
        }
        public static void SetCallbackCfg(string code, MQMainConfiguration targetCfg, IEnumerable<CallbackConfiguration> viewModelCfg)
        {
            if (viewModelCfg.IsEmptyEnumerable()) return;
            targetCfg.MessageCfgList.SingleOrDefault(c => c.Code == code).CallbackCfgList.EachAction(c =>
            {
                var callbackInfo = viewModelCfg.SingleOrDefault(v => v.CallbackKey == c.CallbackKey);
                if (callbackInfo != null)
                {
                    c.Enable = callbackInfo.Enable;
                    c.CallbackTimeOut = callbackInfo.CallbackTimeOut;
                    c.ContentType = callbackInfo.ContentType.ReplaceStringTarget("未设置");
                    c.HttpMethod = callbackInfo.HttpMethod.ReplaceStringTarget("未设置");
                    c.Url = callbackInfo.Url;
                    c.IsRetry = callbackInfo.IsRetry;
                }
            });
        }
        public static void ToAppCodeListModel(MQMainConfiguration main, MQMainConfiguration cfg, List<AppCodeListModel> codes)
        {
            BusConfigurationModel.CheckConfiguration(cfg, main);
            cfg.MessageCfgList.EachAction(c =>
            {
                codes.Add(new AppCodeListModel
                {
                    AppId = cfg.AppId,
                    Code = c.Code,
                    host = cfg.ConnCfg.ConnectionString.Split(';')[0],
                    ConsumeCfg_MaxThreadCount = c.ConsumeCfg.MaxThreadCount.Value,
                    ConsumeCfg_PrefetchCount = c.ConsumeCfg.PrefetchCount.Value,
                    ConsumeCfg_RetryTimeOut = c.ConsumeCfg.RetryTimeOut.Value,
                    IsEnable = c.Enable ? "是" : "否",
                    ExchangeCfg_ExchangeType = c.ExchangeCfg._ExchangeType.Value.ToString(),
                    ExchangeCfg_IsDurable = c.ExchangeCfg.Durable.Value.ToString(),
                    QueueCfg_IsDurable = c.QueueCfg.IsDurable.Value.ToString(),
                    CallbackList = c.CallbackCfgList
                    .VerifyIsEmptyOrNullEnumerable(Enumerable.Empty<CallbackConfiguration>())
                    .Select(_c =>
                    {
                        return new CallBackConfig
                        {
                            Url = _c.Url,
                            Enable = _c.Enable.Value.ToString(),
                            CallbackTimeOut = _c.CallbackTimeOut.Value,
                            IsRetry = IsRetryMessageString(_c.IsRetry)
                        };
                    })
                });
            });
        }

        private static string IsRetryMessageString(int? isretry)
        {
            if (isretry == null) return "未设置";
            if (isretry.Value == 1) return "是";
            return "否";
        }
        public static void CheckConfiguration(MQMainConfiguration currentCfg, MQMainConfiguration defaultCfg)
        {
            YmtSystemAssert.AssertArgumentNotNull(defaultCfg, "默认全局配置不能为空");
            currentCfg.NullObjectReplace(v => currentCfg = v, defaultCfg);

            currentCfg.ConnCfg.NullObjectReplace(v => currentCfg.ConnCfg = v, defaultCfg.ConnCfg);
            currentCfg.ConnCfg.ConnectionString.NullObjectReplace(v => currentCfg.ConnCfg.ConnectionString = v, defaultCfg.ConnCfg.ConnectionString);
            if (currentCfg.MessageCfgList.IsEmptyEnumerable()) 
            {
                currentCfg.MessageCfgList = defaultCfg.MessageCfgList;
                return;
            }
            var defCfg = defaultCfg.MessageCfgList.First();

            foreach (var item in currentCfg.MessageCfgList)
            {
                //检查消费者配置属性
                item.ConsumeCfg.NullObjectReplace(v => item.ConsumeCfg = v, defCfg.ConsumeCfg);
                item.ConsumeCfg.Args.NullObjectReplace(v => item.ConsumeCfg.Args = v, defCfg.ConsumeCfg.Args);
                item.ConsumeCfg.IsAutoAcknowledge.NullAction(v => item.ConsumeCfg.IsAutoAcknowledge = v, defCfg.ConsumeCfg.IsAutoAcknowledge ?? false);
                item.ConsumeCfg.MaxThreadCount.NullAction(v => item.ConsumeCfg.MaxThreadCount = v, defCfg.ConsumeCfg.MaxThreadCount ?? 512);
                item.ConsumeCfg.PrefetchCount.NullAction(v => item.ConsumeCfg.PrefetchCount = v, defCfg.ConsumeCfg.PrefetchCount /*?? 500*/);
                item.ConsumeCfg.UseMultipleThread.NullAction(v => item.ConsumeCfg.UseMultipleThread = v, defCfg.ConsumeCfg.UseMultipleThread ?? false);
                item.ConsumeCfg.HandleFailAcknowledge.NullAction(v => item.ConsumeCfg.HandleFailAcknowledge = v, defCfg.ConsumeCfg.HandleFailAcknowledge ?? true);
                item.ConsumeCfg.HandleFailRQueue.NullAction(v => item.ConsumeCfg.HandleFailRQueue = v, defCfg.ConsumeCfg.HandleFailRQueue ?? false);
                item.ConsumeCfg.RoutingKey.NullObjectReplace(v => item.ConsumeCfg.RoutingKey = v, defCfg.ConsumeCfg.RoutingKey ?? "#.#");
                item.ConsumeCfg.CallbackMethodType.NullObjectReplace(v => item.ConsumeCfg.CallbackMethodType = v, defCfg.ConsumeCfg.CallbackMethodType ?? "POST");
                item.ConsumeCfg.CallbackTimeOut.NullAction(v => item.ConsumeCfg.CallbackTimeOut = v, defCfg.ConsumeCfg.CallbackTimeOut ?? 10000);
                item.ConsumeCfg.CallbackTimeOutAck.NullAction(v => item.ConsumeCfg.CallbackTimeOutAck = v, defCfg.ConsumeCfg.CallbackTimeOutAck ?? true);
                item.ConsumeCfg.RetryCount.NullAction(v => item.ConsumeCfg.RetryCount = v, defCfg.ConsumeCfg.RetryCount ?? 1);
                item.ConsumeCfg.RetryMillisecond.NullAction(v => item.ConsumeCfg.RetryMillisecond = v, defCfg.ConsumeCfg.RetryMillisecond ?? 1000);
                item.ConsumeCfg.HandleFailPersistentStore.NullAction(v => item.ConsumeCfg.HandleFailPersistentStore = v, defCfg.ConsumeCfg.HandleFailPersistentStore ?? true);
                item.ConsumeCfg.HandleSuccessSendNotice.NullAction(v => item.ConsumeCfg.HandleSuccessSendNotice = v, defCfg.ConsumeCfg.HandleSuccessSendNotice ?? true);
                //item.ConsumeCfg.ConsumeCount.NullAction(v => item.ConsumeCfg.ConsumeCount = v, defCfg.ConsumeCfg.ConsumeCount);
                item.ConsumeCfg.HandleFailMessageToMongo.NotNullAction(v => item.ConsumeCfg.HandleFailMessageToMongo = v, defCfg.ConsumeCfg.HandleFailMessageToMongo ?? false);
                item.ConsumeCfg.RetryTimeOut.NullAction(v => item.ConsumeCfg.RetryTimeOut = v, defCfg.ConsumeCfg.RetryTimeOut);
                //检查发布配置属性
                item.PublishCfg.NullObjectReplace(v => item.PublishCfg = v, defCfg.PublishCfg);
                item.PublishCfg.PublisherConfirms.NullAction(v => item.PublishCfg.PublisherConfirms = v, defCfg.PublishCfg.PublisherConfirms ?? false);
                item.PublishCfg.RetryCount.NullAction(v => item.PublishCfg.RetryCount = v, defCfg.PublishCfg.RetryCount ?? 1);
                item.PublishCfg.RetryMillisecond.NullAction(v => item.PublishCfg.RetryMillisecond = v, defCfg.PublishCfg.RetryMillisecond ?? 500);
                item.PublishCfg.UseTransactionCommit.NullAction(v => item.PublishCfg.UseTransactionCommit = v, defCfg.PublishCfg.UseTransactionCommit ?? false);
                item.PublishCfg.RouteKey.NullObjectReplace(v => item.PublishCfg.RouteKey = v, defCfg.PublishCfg.RouteKey ?? "#.#");
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
                item.MessagePropertiesCfg.PersistentMessages.NullAction(v => item.MessagePropertiesCfg.PersistentMessages = v, defCfg.MessagePropertiesCfg.PersistentMessages ?? false);
                item.MessagePropertiesCfg.PersistentMessagesLocal.NullAction(v => item.MessagePropertiesCfg.PersistentMessagesLocal = v, defCfg.MessagePropertiesCfg.PersistentMessagesLocal ?? true);
                item.MessagePropertiesCfg.PersistentMessagesMongo.NullAction(v => item.MessagePropertiesCfg.PersistentMessagesMongo = v, defCfg.MessagePropertiesCfg.PersistentMessagesMongo ?? true);
                item.MessagePropertiesCfg.Priority.NullAction(v => item.MessagePropertiesCfg.Priority = v, defCfg.MessagePropertiesCfg.Priority);
                item.MessagePropertiesCfg.Expiration.NullAction(v => item.MessagePropertiesCfg.Expiration = v, defCfg.MessagePropertiesCfg.Expiration);
                item.MessagePropertiesCfg.ContextType.NullObjectReplace(v => item.MessagePropertiesCfg.ContextType = v, defCfg.MessagePropertiesCfg.ContextType ?? "application/json");
                item.MessagePropertiesCfg.ContentEncoding.NullObjectReplace(v => item.MessagePropertiesCfg.ContentEncoding = v, defCfg.MessagePropertiesCfg.ContentEncoding ?? "utf-8");
                //回调属性检查
                item.CallbackCfgList.EachAction(c =>
                {
                    c.Enable.NullAction(v => c.Enable = v, defCfg.CallbackCfgList.FirstOrDefault() == null ? true : defCfg.CallbackCfgList.FirstOrDefault().Enable ?? true);
                    c.HttpMethod.NullObjectReplace(v => c.HttpMethod = v, defCfg.CallbackCfgList.First().HttpMethod ?? "POST");
                    c.Url.NullObjectReplace(v => c.Url = v, defCfg.CallbackCfgList.First().Url);
                    c.Priority.NullAction(v => c.Priority = v, defCfg.CallbackCfgList.First().Priority);
                    c.ContentType.NullObjectReplace(v => c.ContentType = v, defCfg.CallbackCfgList.First().ContentType ?? "application/json");
                    c.CallbackTimeOut.NullAction(v => c.CallbackTimeOut = v, defCfg.CallbackCfgList.First().CallbackTimeOut ?? 2000);
                    c.CallbackKey.NullObjectReplace(v => c.CallbackKey = v, defCfg.CallbackCfgList.First().CallbackKey);
                    c.IsRetry.NullAction(v => c.IsRetry = v, defCfg.CallbackCfgList.First().IsRetry ?? 1);
                });
            }
        }
        private static string GetConnectionString(string host, string port, string vhost, string userName
            , string password, string channelPool, string connectionPool)
        {
            var _channelPool = ChannelPoolString(channelPool);
            var _connectionPool = ConnectionPool(connectionPool);
            return string.Format(@"host={0};port={1};vHost={2};uNmae={3};pas={4};heartbeat=5000;recoveryInterval=5;channelMax=100;useBackgroundThreads=true{5}{6}"
                                , host, port, vhost, userName, password, _connectionPool, _channelPool);
        }
        public static Dictionary<string, string> ConnectionStringToDictionary(string connection)
        {
            YmtSystemAssert.AssertArgumentNotEmpty(connection, "链接字符窜不能为空");
            var connArray = connection.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            var connDic = new Dictionary<string, string>();
            foreach (var item in connArray)
            {
                var itemArray = item.Split(new char[] { '=' });
                var key = itemArray[0].ToLower();
                var value = itemArray[1];
                connDic[key] = value;
            }
            return connDic;
        }
        private static string ConnectionPool(string connectionPool)
        {
            var _connectionPool = ";pooMinSize=3;pooMaxSize=10";
            if (!string.IsNullOrEmpty(connectionPool))
            {
                var channel = connectionPool.Split('-');
                if (Convert.ToInt32(channel[0]) <= 0)
                    channel[0] = "3";
                if (Convert.ToInt32(channel[0]) >= 500)
                    channel[0] = "500";
                _connectionPool = ";pooMinSize={0};pooMaxSize={1}".Fomart(channel[0], channel[1]);
            }
            return _connectionPool;
        }
        private static string ChannelPoolString(string channelPool)
        {
            var _channelPool = string.Empty;
            if (!string.IsNullOrEmpty(channelPool))
            {
                var channel = channelPool.Split('-');
                _channelPool = ";channelPoolMinSize={0};channelPoolMaxSize={1}".Fomart(channel[0], channel[1]);
            }
            return _channelPool;
        }

    }
    public static class _Extensions
    {
        public static IEnumerable<T> VerifyIsEmptyOrNullEnumerable<T>(this IEnumerable<T> val
            , IEnumerable<T> defVal = default(IEnumerable<T>))
        {
            if (val == null || !val.Any()) return defVal;
            return val;
        }
        public static IEnumerable<T> AddItem<T>(this IEnumerable<T> val, T obj)
        {
            var list = val is List<T> ? val as List<T> : new List<T>();
            list.Add(obj);
            return list;
        }
        public static bool CheckSessionExists(this HttpSessionStateBase session, string key)
        {
            if (session == null) return false;
            if (session[key] == null) return false;
            return true;
        }
        public static T To<T>(this HttpSessionStateBase session, string key, T defVal = default(T))
        {
            if (session == null) return defVal;
            if (session[key] == null) return defVal;
            return (T)session[key];
        }
        public static ActionResult JavaScriptResultResponse(this Controller controller, string function)
        {
            var js = new JavaScriptResult();
            js.Script = function;
            return js;
        }
        public static string ReplaceStringTarget(this string val, string replaceVal, string newVal = null)
        {
            if (val == null) return val;
            if (val.Equals(replaceVal, StringComparison.OrdinalIgnoreCase))
                return newVal;
            return val;
        }
    }
}