using System;
using YmatouMQNet4.Core;

namespace YmatouMQNet4.Dto
{
    public class MQMessageStatusDto
    {
        public string AppId { get; set; }
        public string MsgUniqueId { get; set; }
        public string Code { get; set; }
        public Status Status { get; set; }
    }
}
