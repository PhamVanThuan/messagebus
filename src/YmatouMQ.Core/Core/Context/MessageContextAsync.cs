using System;
using RabbitMQ.Client;

namespace YmatouMQNet4.Core
{
    /// <summary>
    /// 异步发送消息上下文
    /// </summary>
    public class MessageContextAsync
    {
        public object Body { get; private set; }
        public string AppId { get; private set; }
        public string code { get; private set; }
        public string MsgId { get; private set; }
        public IModel Channel { get; private set; } 

        public MessageContextAsync(object body, string appId, string code,string msgId,IModel channel)
        {
            this.Body = body;
            this.AppId = appId;
            this.code = code;
            this.MsgId = msgId;
            this.Channel = channel;
        }

        public override string ToString()
        {
            return string.Format("MessageContextAsync,appId {0},messageId {1}", AppId, code);
        }
    }
}
