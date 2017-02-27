using System;

namespace YmatouMQNet4.Core
{
    /// <summary>
    /// 异常消息上限文
    /// </summary>
    internal class ExceptionMessageContext
    {
        public string appId { get; private set; }
        public string code { get; private set; }
        public string msgId { get; private set; }
        public object body { get; private set; }

        public ExceptionMessageContext(string appId, string code, string msgId, object body)
        {
            this.appId = appId;
            this.code = code;
            this.msgId = msgId;
            this.body = body;
        }
    }
}
