using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace YmatouMQAdmin.WebApp2.Models
{
    public class RetryMessageLogStatus
    {
        public string messageId { get; set; }

        public string body { get; set; }

        public string status { get; set; }

        public DateTime createTime { get; set; }

        public DateTime expiredTime { get; set; }

        public string RetryTime { get; set; }

        public string appKey { get; set; }

        public int retryCount { get; set; }

        //public string callbackKey { get; set; }

        //public string callbackStatus { get; set; }
        public IList<CallbackInfoModels> CallbackKey { get; set; }
    }
    public class CallbackInfoModels
    {
        public string CallbackUrl { get; set; }
        public string FullCallbackUrl { get; set; }
        public string Status { get; set; }
        public int RetryCount { get; set; }
    }
}