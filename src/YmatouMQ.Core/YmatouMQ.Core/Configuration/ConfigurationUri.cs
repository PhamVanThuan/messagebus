using System;
using System.Configuration;

namespace YmatouMQNet4.Configuration
{
    class ConfigurationUri
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

        public static string default_Cfg { get { return string.Format("{0}{1}", Cfg_Host, "api/MQAppCfg/?appid=default"); } }
        public static string app_Cfg { get { return string.Format("{0}{1}", Cfg_Host, "api/MQAppCfg/"); } }
        public static string sys_Cfg { get { return string.Format("{0}{1}", Cfg_Host, "api/MQSysCfg/"); } }
        public static string domain_Cfg { get { return string.Format("{0}{1}", Cfg_Host, "api/MQAppDomainCfg/"); } }
    }
}
