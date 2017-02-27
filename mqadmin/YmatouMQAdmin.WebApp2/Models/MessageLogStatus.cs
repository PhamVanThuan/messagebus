using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace YmatouMQAdmin.WebApp2.Models
{
    public class MessageLogStatus
    {
        public string messageId { get; set; }

        public string message_aid { get; set; }

        public string message_code { get; set; }

        public string message_ip { get; set; }

        public string message_body { get; set; }
        public string message_full_body { get; set; }
        public DateTime message_time { get; set; }

        public string status { get; set; }

        public string status_source { get; set; }

        public DateTime status_time { get; set; }

        public string[] status_cid { get; set; }

        public string BusReceivedServerIp { get; set; }
        public string BusPushServerIp { get; set; }
    }
}