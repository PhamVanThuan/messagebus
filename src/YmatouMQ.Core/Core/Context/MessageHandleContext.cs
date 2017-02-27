using System;

namespace YmatouMQNet4.Core
{
    /// <summary>
    /// 消息处理上下文
    /// </summary>
    public class MessageHandleContext<TMessage>
    {
        /// <summary>
        /// 消息
        /// </summary>
        public TMessage Message { get; private set; }
        /// <summary>
        /// 是否为重新发送
        /// </summary>
        public bool Redelivered { get; private set; }
        public string CallbackUrl { get; private set; }
        public string HttpMethod { get; private set; }
        public int CallbackTimeOut { get; private set; }
        public string AppId { get; set; }
        public string Code { get; set; }
        public string MessageId { get; set; }
        public MessageHandleContext(TMessage msg, bool redelivered)
            : this(msg, redelivered, null, null, 0,null,null,null)
        {

        }
        public MessageHandleContext(TMessage msg, bool redelivered, string callUrl, string httpMethod, int callTimeOut, string appid, string code, string messageid)
        {
            this.Message = msg;
            this.Redelivered = redelivered;
            this.CallbackUrl = callUrl;
            this.HttpMethod = httpMethod;
            this.CallbackTimeOut = callTimeOut;
            this.AppId = appid;
            this.Code = code;
            this.MessageId = messageid;
        }
    }
}
