using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using YmatouMQAdmin.Domain.Common;

namespace YmatouMQAdmin.Domain.Module
{
    public class MQMessageStatus
    {
        [IgnoreDataMember]
        public string _sid { get; set; }
        [IgnoreDataMember]
        public string MessageId { get; set; }
        public string Status { get; set; }
        public DateTime CreateTime { get; set; }
        [IgnoreDataMember]
        public string AppId { get; set; }
        public MQMessageStatus(string messageid, string status, string appId)
        {
            this._sid = Guid.NewGuid().ToString("N");
            this.MessageId = messageid;
            this.Status = status;
            this.CreateTime = DateTime.Now;
            this.AppId = appId;
        }
        public string AssignCollectionName()
        {
            var _status = Status.ToLower();
            if (string.IsNullOrEmpty(_status)) return string.Format("mq_p_s_{0}_{1}", AppId, 0);
            if (_status == "normal") return string.Format("mq_p_s_{0}_{1}", AppId, 0);
            if (_status == "exception") return string.Format("mq_p_s_{0}_{1}", AppId, 1);
            if (_status == "memoryqueuegtlimit") return string.Format("mq_p_s_{0}_{1}", AppId, 2);
            if (_status == "handleexception") return string.Format("mq_c_s_{0}_{1}", AppId, 3);
            if (_status == "handlesuccess") return string.Format("mq_c_s_{0}_{1}", AppId, 4);
            return string.Format("mq_status_{0}_{1}", AppId, 0);
        }
        protected MQMessageStatus() { }
    }
}
