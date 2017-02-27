//using System;
//using System.Collections.Generic;
//using System.Collections.Concurrent;
//using System.Linq;
//using System.Text;
//using System.IO;
//using System.Threading;
//using YmatouMessageBusClientNet4.Extensions;

//namespace YmatouMessageBusClientNet4.Persistent
//{
//    public class MessagePersistent : MessagePersistentBase<_Message>
//    {
//        public override string[] GetAllMessageBusFile()
//        {
//            if (!Directory.Exists(this.dir)) return new string[0];
//            return Directory
//                    .GetFiles(this.dir, "*.message")
//                    .AsParallel()
//                    .Where(e =>
//                        Path.GetFileName(e) != "_retry_1.message"
//                        && Path.GetFileName(e) != "_timeout.message"
//                        && !Path.GetFileName(e).Contains("_gtqueuelimit"))
//                    .ToArray();
//        }
//        public void SetFilePath(string path, string fileName)
//        {
//            base.SetFilePath(path, fileName, MessageType.NotRetry);
//        }
//    }
//}
