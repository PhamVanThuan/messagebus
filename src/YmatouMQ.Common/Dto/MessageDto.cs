using System;
using System.Collections.Generic;

namespace YmatouMQ.Common.Dto
{
    public class MessageDto
    {
        public string AppId { get; set; }
        public string Code { get; set; }
        public string Ip { get; set; }
        public string MsgUniqueId { get; set; }
        public object Body { get; set; }
    }

    public class MessageBatchDto
    {
        public string AppId { get; set; }
        public string Code { get; set; }
        public string Ip { get; set; }
        public IEnumerable<MessageItemDto> Items { get; set; }
    }

    public class MessageItemDto
    {
        public string MsgUniqueId { get; set; }
        public object Body { get; set; }
    }
}
