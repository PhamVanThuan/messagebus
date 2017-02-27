using System;

namespace YmatouMQ.Common.MessageHandleContract
{
    /// <summary>
    /// 消息处理上下文
    /// </summary>
    public struct MessageHandleContext<TMessage>
    {
        /// <summary>
        /// 消息
        /// </summary>
        public TMessage Message { get; private set; }
        /// <summary>
        /// 是否为重新发送
        /// </summary>
        public bool Redelivered { get; private set; }
        public string AppId { get; private set; }
        public string Code { get; private set; }
        public string MessageId { get; private set; }
        public string Uuid { get; private set; }
        /// <summary>
        /// 消息来源
        /// </summary>
        public string Source { get; private set; }
        public string Url { get; set; }
        public int? TimeOut { get; set; }
        public string ContextType { get; set; }
        public string HttpMethodType { get; set; }
        /// <summary>
        /// 补偿消息对应的业务端KEY
        /// </summary>
        public string[] RetryCallbackKey { get; set; }
        /// <summary>
        /// 是否检查业务端禁止重试（当消息来源为接收服务时，第一次推送消息忽略是否禁止重试属性）
        /// </summary>
        public bool IsCheckEableRetry { get; private set; }

        public MessageHandleContext(TMessage msg, bool redelivered, string appid, string code, string messageid, string source
            , string url = null, int? timeOut = null, string contextType = null, string httpMethodType = null, string[] retryCallbackKey=null,string uuid=null)
            :this()
        {
            Message = msg;
            Redelivered = redelivered;
            AppId = appid;
            Code = code;
            MessageId = messageid;
            Source = source;
            Url = url;
            TimeOut = timeOut;
            ContextType = contextType;
            HttpMethodType = httpMethodType;
            RetryCallbackKey = retryCallbackKey;
            Uuid = uuid;
            IsCheckEableRetry = true;
        }

        public void SetIsCheckEableRetry(bool isCheck)
        {
            this.IsCheckEableRetry = isCheck;
        }

        /// <summary>
        /// 设置回调业务端属性
        /// </summary>
        /// <param name="url"></param>
        /// <param name="timeOut"></param>
        /// <param name="contextType"></param>
        /// <param name="httpMethodType"></param>
        public void SetCallback(string url, int? timeOut, string contextType, string httpMethodType)
        {
            this.Url = url;
            this.TimeOut = timeOut;
            this.ContextType = contextType;
            this.HttpMethodType = httpMethodType;
        }
        public void SetCallback(string[] callbackKey)
        {
            this.RetryCallbackKey = callbackKey;
        }
    }
}
