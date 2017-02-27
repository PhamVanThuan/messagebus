using System;
using System.Collections.Generic;

namespace YmatouMQMessageMongodb.Domain.Module
{
    public class RetryMessage
    {

   

        public string _id { get; private set; }
        public DateTime CreateTime { get; private set; }
        public DateTime? RetryTime { get; private set; }
        public DateTime RetryExpiredTime { get; set; }
        public RetryStatus Status { get; private set; }
        public string AppId { get; set; }
        public string Code { get; set; }
        public object Body { get; set; }
        public string MessageId { get; set; }
        public string AppKey { get; private set; }
        /// <summary>
        /// 重新设置补偿状态（yes,no）
        /// </summary>
        public string IsReSetRetryStatus { get; private set; }
        public List<CallbackInfo> CallbackKey { get; private set; }
        public int RetryCount { get; private set; }
        public string Desc { get; private set; }
        /// <summary>
        /// 消息来源
        /// </summary>
        public string MessageSource { get; private set; }

        public RetryMessage(string appid, string code, string messageid, object body, DateTime rExpirerTime,
            List<string> callBackKey, string desc = null, string uuid = null, string messageSource = null)
        {
            this._id = uuid ?? Guid.NewGuid().ToString("N");
            this.CreateTime = DateTime.Now;
            this.AppId = appid;
            this.Code = code;
            this.Body = body;
            this.Status = RetryStatus.NotRetry;
            this.RetryExpiredTime = rExpirerTime;
            this.AppKey = "*";
            this.MessageId = messageid;
            this.InitCallbackKey(callBackKey);
            this.Desc = desc;
            this.MessageSource = messageSource;
        }

        public void SetCallbackKey(List<CallbackInfo> callbackKey)
        {
            this.CallbackKey = callbackKey;
        }

        public void InitCallbackKey(List<string> callbackKey)
        {
            var list = new List<CallbackInfo>();
            foreach (var item in callbackKey)
            {
                list.Add(new CallbackInfo {CallbackKey = item});
            }
            this.CallbackKey = list;
        }

        public void SetStatus(RetryStatus status)
        {
            this.Status = status;
        }

        protected RetryMessage()
        {
        }
    }

    public class CallbackInfo
    {
        public string CallbackKey { get; set; }
        public RetryStatus Status { get; set; }
        public int RetryCount { get; set; }

        public int AddRetryCount(int rawValue, int value = 1)
        {
            return System.Threading.Interlocked.Add(ref rawValue, value);
        }
    }

    /// <summary>
    /// 消息补充状态
    /// </summary>
    public enum RetryStatus : byte
    {
        /// <summary>
        /// 未补发（初始化）
        /// </summary>
        NotRetry = 0,

        /// <summary>
        /// 补发中 
        /// </summary>
        Retrying = 1,

        /// <summary>
        /// 补发成功
        /// </summary>
        RetryOk = 2,

        /// <summary>
        /// 补发失败
        /// </summary>
        RetryFail = 3
    }
}
