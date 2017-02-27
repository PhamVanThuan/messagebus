using System;

namespace YmatouMQNet4.Dto
{
    public class MessageDto
    {
        public string AppId { get; set; }
        public string Code { get; set; }
        public string Ip { get; set; }
        public string MsgUniqueId { get; set; }
        public object Body { get; set; }
    }
}
