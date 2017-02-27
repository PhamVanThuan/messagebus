using System;
using YmatouMQ.Common.Utils;

namespace YmatouMQMessageMongodb.Domain.Module
{
    public class MQMessage
    {
        public string _id { get; set; }
        public string AppId { get; set; }
        public string Code { get; set; }
        public string Ip { get; set; }
        public string MsgId { get; set; }
        public object Body { get; set; }//json
        public DateTime CreateTime { get; set; }
        public MQMessageStatus[] Status { get; set; }
        public string BusReceivedServerIp { get; set; }
        public string UuId { get; set; }
        /// <summary>
        /// 推送状态（注：0：未推送，null,1000：已推送到业务端）
        /// </summary>
        public int PushStatus { get; set; }
        public DateTime PushTime { get; set; }
      
        /// <summary>
        /// 已推送 [1000] 
        /// </summary>
        public static readonly int AlreadyPush = 1000;
        /// <summary>
        /// 已重试 [1300]
        /// </summary>
        public static readonly int AlreadyRetry = 1300;        
        /// <summary>
        /// 初始化，未推送 [0]
        /// </summary>
        public static readonly int Init = 0;

        public MQMessage(string appId, string code, string ip, string msgid, object body, string status,string uuid=null,int pushStatus=0)
        {
            this._id = uuid ?? Guid.NewGuid().ToString("N");
            this.AppId = appId;
            this.Code = code;
            this.Ip = ip;
            this.MsgId = msgid;
            this.Body = body;
            this.CreateTime = DateTime.Now;
            this.BusReceivedServerIp = _Utils.GetLocalHostIp();
            this.UuId = uuid;
            this.PushStatus = pushStatus;
            this.PushTime = DateTime.Now;
        }
       
        public void SetPushStatus(int status = 1000)
        {
            this.PushStatus = status;
        }

        protected MQMessage()
        {
        }
    }
}
