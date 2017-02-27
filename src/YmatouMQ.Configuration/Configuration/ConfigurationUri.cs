using System;
using System.Configuration;

namespace YmatouMQNet4.Configuration
{
    public class ConfigurationUri
    {
        //public const string default_Cfg = "http://mqadmin.ymatou.com/api/MQDefaultCfg/";
        //public const string app_Cfg = "http://mqadmin.ymatou.com/api/MQAppCfg/";
        //public const string sys_Cfg = "http://mqadmin.ymatou.com/api/MQSysCfg/";
        private static readonly string def_cfgHost = "http://mqadmin.ymatou.com/";

        private static string Cfg_Host
        {
            get
            {
                var cfgHost = ConfigurationManager.AppSettings["mqcfgserverhost"];
                if (string.IsNullOrEmpty(cfgHost))
                    return def_cfgHost;
                return cfgHost;
            }
        }
        /// <summary>
        /// 默认配置
        /// </summary>
        public static string default_Cfg { get { return string.Format("{0}{1}", Cfg_Host, "api/MQAppCfg/?appid=default"); } }
        /// <summary>
        /// 具体配置
        /// </summary>
        public static string app_Cfg { get { return string.Format("{0}{1}", Cfg_Host, ConfigurationManager.AppSettings["conn_url_sub_requestpath"] ?? "api/MQAppCfg/"); } }
        public static string sys_Cfg { get { return string.Format("{0}{1}", Cfg_Host, "api/MQSysCfg/"); } }
        /// <summary>
        /// domain配置
        /// </summary>
        public static string domain_Cfg { get { return string.Format("{0}{1}", Cfg_Host, "api/MQAppDomainCfg/"); } }
        /// <summary>
        /// 推送服务默认domain配置
        /// </summary>
        public static string subDomainCfg { get { return string.Format("{0}{1}", Cfg_Host, "api2/sub/maindomain/cfg/"); } }
        /// <summary>
        /// 接收服务默认domain配置
        /// </summary>
        public static string pubDomainCfg { get { return string.Format("{0}{1}", Cfg_Host, ConfigurationManager.AppSettings["conn_url_pub_requestpath"] ?? "api2/pub/maindomain/cfg/"); } }
        /// <summary>
        /// 补偿服务配置
        /// </summary>
        public static string compensateCfg { get { return string.Format("{0}{1}", Cfg_Host, "api2/compensate/compensateDomain/cfg/"); } }
    }
}
