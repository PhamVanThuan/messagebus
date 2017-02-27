using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using YmatouMQ.Common.Extensions;
using YmatouMQ.Common.MessageHandleContract;
using YmatouMQ.Common.Utils;

namespace YmatouMQMessageMongodb.Domain.Module
{
    public class MQMessageStatus
    {
        [IgnoreDataMember]
        public string _sid { get; set; }

        [IgnoreDataMember]
        public string MessageId { get; set; }

        /// <summary>
        /// 推送状态
        /// </summary>
        public string Status { get; set; }

        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 处理来源
        /// </summary>
        public string HandleSource { get; set; }

        [IgnoreDataMember]
        public string AppId { get; set; }

        public string[] CallbackId { get; set; }
        public string ReceivedMessageIp { get; set; }
        public string MessageUuid { get; set; }
        //是否是重复推送
        [IgnoreDataMember]
        public bool IsRepeat { get; set; }

        public MQMessageStatus(string messageid, MessagePublishStatus pushStatus, string appId, string handleSource
            , string[] callbackId, string uuid = null, bool isRepeat = false)
        {
            this._sid = uuid;
            this.MessageId = messageid;
            this.Status = Format(pushStatus);
            this.CreateTime = DateTime.Now;
            this.AppId = appId;
            this.HandleSource = isRepeat ? "{0}_Repeat".Fomart(handleSource) : handleSource;
            this.CallbackId = callbackId;
            this.ReceivedMessageIp = _Utils.GetLocalHostIp();
            this.MessageUuid = uuid;
            this.IsRepeat = isRepeat;
        }

        public static string GetCollectionName(string appid)
        {
            return string.Format("mq_subscribe_ok_{0}", appid);
        }

        public static string GetDbName()
        {
            return string.Format("MQ_Message_Status_{0}", DateTime.Now.ToString("yyyyMM"));
        }

        private static string Format(MessagePublishStatus pushStatus)
        {
            return pushStatus.ToString();
        }

        protected MQMessageStatus()
        {
        }
    }
}
