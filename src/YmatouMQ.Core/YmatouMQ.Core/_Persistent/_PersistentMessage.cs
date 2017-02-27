using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using YmatouMQNet4.Utils;
using YmatouMQNet4.Extensions;
using YmatouMQNet4.Extensions._Task;
using YmatouMQNet4.Core;
using System.Net;
using YmatouMQNet4.Dto;
using YmatouMQNet4.Extensions.Serialization;
using System.Configuration;

namespace YmatouMQNet4._Persistent
{
    public class _PersistentMessageToLocal
    {
        private static readonly ILog log = LogFactory.GetLogger(LogFactory._LogType, "YmatouMQNet4._Persistent._PersistentMessage");
        public static Task LocalStore<TMessage>(TMessage msg, string appId, string code,string msgId, Status status)
        {
            var _msg = new _PMessage<TMessage>(msg, appId, code, status);
            var by = _msg.ToProtoBuf();
            var directoryPath = generatedirectorypath(appId);
            EnsureDirectoryExists(directoryPath);
            return FileAsync.WriteAllBytes(generatefilepath(directoryPath, appId, status), by);
        }
        public static Task MongoStore<TMessage>(TMessage msg, string appId, string code, string msgId, Status status)
        {
            var dto = new MessagePersistentDto { AppId = appId, Code = code, MsgUniqueId = msgId, Status = status, Ip = null, Body = msg.JSONSerializationToString() };
            var by = dto.JSONSerializationToByte();
            var host = ConfigurationManager.AppSettings["mqadmin"] ?? "http://mqadmin.ymatou.com/";
            var uri = string.Format("{0}{1}", host, "mq/admin/m/MessagePersistentDto/");
            return WebRequestExtensions.Post(by, uri, ex => log.Error("持久化消息异常 {0}", ex.ToString()));
        }      
        public static Task SendNotice(string appId, string code, string msgId, Status status)
        {
            var dto = new MQMessageStatusDto { AppId = appId, Code = code, MsgUniqueId = msgId, Status = status };
            var by = dto.JSONSerializationToByte();
            var host = ConfigurationManager.AppSettings["mqadmin"] ?? "http://mqadmin.ymatou.com/";
            var uri = string.Format("{0}{1}", host, "mq/admin/m/MQMessageStatus/");
            return WebRequestExtensions.Post(by, uri, ex => log.Error("发送消息状态通知异常 {0}", ex.ToString()));
        }
       
        private static string generatedirectorypath(string appid)
        {
           return AppDomain.CurrentDomain.BaseDirectory + "mqjournal\\";            
        }
        public static string generatefilepath(string directoryPath, string appId, Status status)
        {
            return string.Format("{0}/{1}_{2}_{3}", directoryPath, DateTime.Now.ToString("yyyyMMdd"), appId, Convert.ToInt32(status));
        }
        private static void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        }
    }
}
