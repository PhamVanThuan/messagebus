using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using YmatouMessageBusClientNet4.Extensions;
using System.IO;
using Ymatou.CommonService;

namespace YmatouMessageBusClientNet4
{
    public class MessageBusClientCfg
    {
        private static readonly Lazy<MessageBusClientCfg> meccageBusClientCfg = new Lazy<MessageBusClientCfg>(() => new MessageBusClientCfg());
        private static readonly string cfg_file_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "messagebus.config");
        private bool LoadFileOk;
        private Dictionary<string, object> cfgDic = new Dictionary<string, object>();

        private MessageBusClientCfg()
        {

        }
        public static MessageBusClientCfg Instance
        {
            get { return meccageBusClientCfg.Value; }
        }
        public T DefaultConfigruation<T>(string cfgType, T defaultVal = default(T))
        {
            EnsureLoadCfg();

            var key = "default_{0}".F(cfgType);
            if (cfgDic.ContainsKey(key))
            {
                return cfgDic["default_{0}".F(cfgType)].ConvertTo<T>(defaultVal);
            }
            else
            {
                return AppCfgInfo2.default_Cfg[key].ConvertTo<T>(defaultVal);
            }
        }
        public T Configruation<T>(string appid, string code, string cfgType, T defaultVal = default(T))
        {
            EnsureLoadCfg();

            var key = cfgType == AppCfgInfo2.bushost_primary || cfgType == AppCfgInfo2.bushost_secondary
                                ? "{0}_{1}".F(appid, AppCfgInfo2.key_maping[cfgType])
                                : "{0}_{1}_{2}".F(appid, code, AppCfgInfo2.key_maping[cfgType]);
            if (cfgDic.ContainsKey(key))
            {
                var val = (cfgDic[key] ?? cfgDic["default_{0}".F(cfgType)]);
                return val.ConvertTo<T>(defaultVal); ;
            }
            else
            {
                var default_key = "default_{0}".F(cfgType);
                if (cfgDic.ContainsKey(default_key))
                {
                    var val = cfgDic[default_key];
                    return val == null ? defaultVal : val.ConvertTo<T>(defaultVal); ;
                }
                else
                {
                    return AppCfgInfo2.default_Cfg[default_key].ConvertTo<T>(defaultVal);
                }
            }
        }
        //
        public void LoadCfg()
        {
            if (!File.Exists(cfg_file_path))
            {
                ApplicationLog.Error("文件{0}不存在，使用默认配置".F(cfg_file_path));
                cfgDic = AppCfgInfo2.default_Cfg;
                return;
            }
            cfgDic = ReadCfgFile().FromJsonTo<Dictionary<string, object>>(AppCfgInfo2.default_Cfg);
            LoadFileOk = true;
        }
        public bool LoadConfigurationOk { get { return LoadFileOk; } }
        //
        public void SaveCfg(Dictionary<string, object> cfgDic)
        {
            if (cfgDic == null)
                WriteCfgFile(AppCfgInfo2.default_Cfg.ToJson());
            else
                WriteCfgFile(cfgDic.ToJson());
        }
        private string ReadCfgFile()
        {
            try
            {
                using (var fileStream = new FileStream(cfg_file_path, FileMode.Open, FileAccess.Read))
                using (var readStream = new StreamReader(fileStream, Encoding.GetEncoding("utf-8")))
                {
                    return readStream.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                ApplicationLog.Error("消息总线读取配置文件异常", ex);
                return string.Empty;
            }
        }
        private void WriteCfgFile(string val)
        {
            using (var fileStream = new FileStream(cfg_file_path, FileMode.Truncate, FileAccess.Write))
            using (var writeStream = new StreamWriter(fileStream, Encoding.GetEncoding("utf-8")))
            {
                writeStream.Write(val);
            }
        }
        private void EnsureLoadCfg()
        {
            if (!LoadConfigurationOk || !cfgDic.Any())
            {
                LoadCfg();
            }
        }
    }
    public class AppCfgInfo2
    {
        public const string bushost_primary = "primarybushost";
        public const string bushost_secondary = "secondaryhost";
        public const string storepath = "storepath";
        public const string retrytime = "retrytime";
        public const string queuelimit = "queuelimit";
        public const string queuelimitfileSize = "queuelimitfileSize";
        public const string batchMessageRequestPath = "batchMessageRequestPath";
        public const string busHttpConnectionLimit = "busHttpConnectionLimit";
        public const string batchMessageLimit = "batchMessageLimit";
        public const string retrytimeout = "retrytimeout";
        public const string publishtimeout = "publishtimeout";
        public const string publishasync = "publishasync";
        public const string requestpath = "requestpath";
        public const string journalpath = "journalpath";
        public const string journalsize = "journalsize";
        public const string journalbuffersize = "journalbuffersize";
        public const string journalenable = "journalenable";
        public const string messagesendlogpath = "messagesendlogpath";
        public const string messagesendlogsize = "messagesendlogsize";
        public const string messagesendlogbuffersize = "messagesendlogbuffersize";
        public const string sendagentversiontoserver = "issendversion";
        public const string agentappid = "agentappid";
        public const string sendagentappidtoserver = "issendappid";
        public const string journaltype = "journaltype";

        public static readonly string default_Store_Path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "messagebusdata");

        private const string default_primary_bushost = "default_primarybushost";
        private const string default_secondary_bushost = "default_secondaryhost";
        private const string default_journalpath = "default_journalpath";
        private const string default_journalsize = "default_journalsize";
        private const string default_journalbuffersize = "default_journalbuffersize";
        public const string default_journalenable = "default_journalenable";
        public const string default_messagesendlogpath = "default_messagesendlogpath";
        public const string default_messagesendlogsize = "default_messagesendlogsize";
        public const string default_messagesendlogbuffersize = "default_messagesendlogbuffersize";
        public const string default_agentappid = "default_agentappid";
        public const string default_sendagentversiontoserver = "default_issendversion";
        public const string default_sendagentappidtoserver = "default_issendappid";
        private const string default_journaltype = "default_journaltype";

        private const string default_storepath = "default_storepath";
        private const string default_retrytime = "default_retrytime";
        private const string default_queuelimit = "default_queuelimit";
        private const string default_queuelimitfileSize = "default_queuelimitfileSize";
        private const string default_batchMessageRequestPath = "default_batchMessageRequestPath";
        private const string default_busHttpConnectionLimit = "default_busHttpConnectionLimit";
        private const string default_batchMessageLimit = "default_batchMessageLimit";
        private const string default_retrytimeout = "default_retrytimeout";
        private const string default_publishtimeout = "default_publishtimeout";
        private const string default_publishasync = "default_publishasync";
        private const string default_requestpath = "default_requestpath";


        internal const string _appid = "appid";
        internal const string _code = "code";
        internal const string _body = "body";
        internal const string _ip = "ip";
        internal const string _messageid = "msguniqueid";
        internal const string _version = "_v";
        internal const string _clientappid = "_appid";

        internal const string agent_version = "2.0.0.0";

        public static readonly Dictionary<string, object> default_Cfg = new Dictionary<string, object> 
        {
            {default_primary_bushost,"http://api.mq.ymatou.com:1234"},
            {default_secondary_bushost,"http://api.mq.secondary.ymatou.com"},
            {default_storepath,""},
            {default_retrytime,3000},
            {default_queuelimit,30000},
            {default_queuelimitfileSize,1},
            {default_batchMessageRequestPath,"/message/publish"},
            {default_busHttpConnectionLimit,500},
            {default_batchMessageLimit,Environment.ProcessorCount},
            {default_retrytimeout,TimeSpan.FromMinutes(3)},
            {default_publishtimeout,5000},
            {default_publishasync,true},
            {default_requestpath,"/message/publish"}, 
            {default_journalpath,"."},//当前程序运行目录
            {default_journalsize,10},
            {default_journalbuffersize,4096},
            {default_journaltype,"all"},
            {default_messagesendlogpath,"."},
            {default_messagesendlogsize,10},
            {default_messagesendlogbuffersize,4096},
            {default_journalenable,true},
            {default_agentappid,null},            
            {default_sendagentversiontoserver,true},
            {default_sendagentappidtoserver,true}
        };
        public static readonly Dictionary<string, string> key_maping = new Dictionary<string, string> 
        {
           {bushost_primary,"primarybushost"} ,
           {bushost_secondary,"secondaryhost"},
           {storepath,"storepath"},
           {retrytime,"retrytime"},
           {queuelimit,"queuelimit"},
           {queuelimitfileSize,"queuelimitfileSize"},
           {batchMessageRequestPath,"batchMessageRequestPath"},
           {busHttpConnectionLimit,"busHttpConnectionLimit"},
           {batchMessageLimit,"batchMessageLimit"},
           {retrytimeout,"retrytimeout"},
           {publishtimeout,"publishtimeout"},
           {publishasync,"publishasync"},
           {requestpath,"requestpath"}, 
           {journalpath,"journalpath"},
           {journalsize,"journalsize"},
           {journalbuffersize,"journalbuffersize"},
           {journaltype,"journaltype"}  ,
           {messagesendlogpath,messagesendlogpath},
           {messagesendlogsize,messagesendlogsize},
           {messagesendlogbuffersize,messagesendlogbuffersize},
           {journalenable,journalenable},
           {agentappid,agentappid},
           {sendagentversiontoserver,sendagentversiontoserver},
           {sendagentappidtoserver,sendagentappidtoserver}
        };
    }
}
