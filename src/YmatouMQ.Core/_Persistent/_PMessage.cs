//using System;
//using ProtoBuf;
//using ProtoBuf.Serializers;
//using YmatouMQNet4.Core;
//using YmatouMQ.Common.Extensions.Serialization;
//using YmatouMQ.Common.MessageHandleContract;

//namespace YmatouMQNet4._Persistent
//{
//    [ProtoContract]
//    internal class _PMessage<TMessage>
//    {
//        [ProtoIgnore]
//        public TMessage message { get; private set; }
//        [ProtoMember(1, Name = "_m_", IsRequired = false)]
//        public byte[] body { get; private set; }
//        [ProtoMember(2, Name = "_b_", IsRequired = false)]
//        public string code { get; private set; }
//        [ProtoMember(3, Name = "_a_", IsRequired = false)]
//        public string appid { get; private set; }
//        [ProtoMember(4, Name = "_s_", IsRequired = false)]
//        public Status status { get; private set; }

//        protected _PMessage()
//        {
//        //}
//        public _PMessage(TMessage msg, string appId, string code, Status status)
//        {
//            this.appid = appid;
//            this.code = code;
//            this.status = status;
//            this.message = msg;
//        }
//        [ProtoBeforeSerialization]
//        public void OnSerializing()
//        {
//            body = message.JSONSerializationToByte();
//        }
//        [ProtoAfterDeserialization]
//        public void OnDeserialized()
//        {
//            message = body.JSONDeserializeFromByteArray<TMessage>();
//        }
//    }
//}
