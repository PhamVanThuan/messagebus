using System;
using YmatouMQ.Common.MessageHandleContract;

namespace YmatouMQ.Common.Dto
{
    public class MQMessageStatusDto
    {
        public string AppId { get; set; }
        public string MsgUniqueId { get; set; }
        public string Code { get; set; }
        public Status Status { get; set; }
    }
}
