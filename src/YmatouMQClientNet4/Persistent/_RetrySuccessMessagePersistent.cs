//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading;

//namespace YmatouMessageBusClientNet4.Persistent
//{
//    public class _RetrySuccessMessagePersistent : MessagePersistentBase<_RetrySuccessMessage>
//    {
//        public override string[] GetAllMessageBusFile()
//        {
//            if (!Directory.Exists(this.dir)) return new string[0];
//            return Directory.GetFiles(this.dir, "*_1.message");
//        }
//        public void SetFilePath(string path, string fileName)
//        {
//            base.SetFilePath(path, fileName, MessageType.RetrySuccess);
//        }
//    }
//}
