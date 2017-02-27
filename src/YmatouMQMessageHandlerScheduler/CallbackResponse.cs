using System;

namespace YmatouMQ.MessageScheduler
{
    public class CallbackResponse
    {
        public string CallbackKey { get; private set; }
        public string Result { get; private set; }
        public string Message { get; private set; }
        public string MessageId { get;private set; }
        public CallbackResponse() { }
        public CallbackResponse(string cKey, string result, string errorMessage,string messageId)
            : this()
        {
            this.CallbackKey = cKey;
            this.Result = result;
            this.Message = errorMessage;
            this.MessageId = messageId;
        }
    }
}
