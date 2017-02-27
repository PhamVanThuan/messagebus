using System;
using System.Linq;
using System.Linq.Expressions;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using YmatouMQNet4.Configuration;

namespace YmatouMQMessageMongodb.Domain.Specifications
{
    /// <summary>
    /// 消息总线配置规则
    /// </summary>
    public class MQConfigurationSpecifications
    {
        public static IMongoQuery MatchAllAppId()
        {
            return Query.Null;
        }

        /// <summary>
        /// 匹配指定的配置（应用，消息类型），忽略 enable 的appid
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public static IMongoQuery MmatchAppCfg3(string appId, string code)
        {
            if (string.IsNullOrEmpty(appId) && string.IsNullOrEmpty(code)) return Query.Null;
            else if (!string.IsNullOrEmpty(appId) && !string.IsNullOrEmpty(code)) return Query.And(Query.EQ("_id", appId), Query.ElemMatch("MessageCfgList", Query.EQ("Code", code)));
            else if (!string.IsNullOrEmpty(appId)) return Query.EQ("_id", appId);
            else return Query.Null;
        }
        /// <summary>
        /// 匹配默认（全局）配置
        /// </summary>
        /// <returns></returns>
        public static IMongoQuery MmatchDefaultCfg()
        {
            return Query.EQ("_id", "default");
        }
        /// <summary>
        /// 匹配指定的消息应用（appid）
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public static Expression<Func<MQMainConfiguration, bool>> MmatchAppCfg(string appId = "default")
        {
            var __appId = appId.ToLower();
            return e => e.AppId == __appId;
        }
        /// <summary>
        /// 匹配指定的RabbitMQ连接类型（主，从）
        /// </summary>
        /// <param name="connType"></param>
        /// <returns></returns>
        public static IMongoQuery MatchConnectionId(string connType)
        {
            if (string.IsNullOrEmpty(connType)) return Query.Null;
            return Query.EQ("_id", connType);
        }
        /// <summary>
        /// 匹配一个或全部appdomain配置。domainName 为空则匹配所有domain配置
        /// </summary>
        /// <param name="domainName">domian名称</param>
        /// <returns></returns>
        public static IMongoQuery _MatchOneOrAllAppdomain(string domainName)
        {
            if (string.IsNullOrEmpty(domainName) || domainName == "_all") return Query.Null;
            domainName = domainName.ToLower();
            return Query.And(Query<AppdomainConfiguration>.EQ(e => e.DomainName, domainName));
        }
        /// <summary>
        /// 配置库ＤＢ
        /// </summary>
        public static string ConfigurationDb { get { return "MQ_Configuration_201505"; } }
        /// <summary>
        /// 配置库默认全局配置table
        /// </summary>
        public static string ConfigurationDefaultCfgTb { get { return "MQ_Default_Cfg"; } }
        /// <summary>
        /// 配置库具体配置table
        /// </summary>
        public static string ConfigurationAppDetailsTb { get { return "MQ_App_Cfg"; } }
        /// <summary>
        /// 配置库appdomain table
        /// </summary>
        public static string AppDomainTb { get { return "MQ_Appdomain_Cfg"; } }
        /// <summary>
        /// 匹配RabbitMQ主从连接table
        /// </summary>
        public static string RabbitMQConnStringTb { get { return "MQ_Connection_Cfg"; } }
        public static string primaryConn = "primary";
        public static string secondaryConn = "secondary";
    }
}
