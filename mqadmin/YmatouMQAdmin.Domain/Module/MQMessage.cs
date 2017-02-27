using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YmatouMQAdmin.Domain.Common;

namespace YmatouMQAdmin.Domain.Module
{
    public class MQMessage
    {
        public string _id { get; set; }
        public string AppId { get; set; }
        public string Code { get; set; }
        public string Ip { get; set; }
        public string MsgId { get; set; }
        public string Body { get; set; }//json
        public DateTime CreateTime { get; set; }

        public MQMessageStatus[] Status { get; set; }

        public MQMessage(string appId, string code, string ip, string msgid, string body, string status)
        {
            this._id = Guid.NewGuid().ToString("N");
            this.AppId = appId;
            this.Code = code;
            this.Ip = ip;
            this.MsgId = msgid;
            this.Body = body;
            this.CreateTime = DateTime.Now;
        }

        protected MQMessage()
        {
        }
    }
}
