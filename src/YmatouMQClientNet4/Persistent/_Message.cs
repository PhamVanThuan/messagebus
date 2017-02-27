//using System;
////using ProtoBuf;

//namespace YmatouMessageBusClientNet4.Persistent
//{
//   // [ProtoContract]
//    public class _Message
//    {
//     //   [ProtoMember(1, Name = "m", IsRequired = false)]
//        public string message { get; private set; }
//     //   [ProtoMember(2, Name = "c", IsRequired = false)]
//        public string code { get; private set; }
//     //   [ProtoMember(3, Name = "a", IsRequired = false)]
//        public string appid { get; private set; }
//     //   [ProtoMember(4, Name = "r", IsRequired = false)]
//        public int retry { get; private set; }
//      //  [ProtoMember(5, Name = "i", IsRequired = false)]
//        public string messageid { get; private set; }
//     //   [ProtoMember(6, Name = "t", IsRequired = false)]
//        public long expiredAtTime { get; private set; }

//        protected _Message()
//        {
//        }

//        public _Message(string appId, string code, string messageBody, string messageId, DateTime expiredAtTime, int retry = 0)
//        {
//            this.appid = appId;
//            this.code = code;
//            this.retry = retry;
//            this.message = messageBody;
//            this.expiredAtTime = expiredAtTime.Ticks;
//            this.messageid = messageId;
//        }
//    }

//  //  [ProtoContract(Name = "rsm")]
//    public class _RetrySuccessMessage
//    {
//    //    [ProtoMember(1, Name = "m", IsRequired = false)]
//        public string messageid { get; private set; }

//        protected _RetrySuccessMessage()
//        {
//        }

//        public _RetrySuccessMessage(string messageId)
//        {
//            this.messageid = messageId;
//        }
//    }

//    public enum MessageType
//    {
//        NotRetry = 0,
//        RetrySuccess = 1
//    }
//}
