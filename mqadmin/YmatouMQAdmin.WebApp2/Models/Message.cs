using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace YmatouMQAdmin.WebApp2.Models
{
    public class Message
    {
        public string AppId { get; set; }
        public string Code { get; set; }
        public string Body { get; set; }
        public int Num { get; set; }
        public bool UseWebClient { get; set; }
    }
}