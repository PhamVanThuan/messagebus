//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;

//namespace YmatouMessageBusClientNet4.Persistent
//{
//    public class TimeOutMessgaePersistent : MessagePersistentBase<_Message>
//    {
//        public override string[] GetAllMessageBusFile()
//        {
//            if (!Directory.Exists(this.dir)) return new string[0];
//            return Directory
//                    .GetFiles(this.dir, "*.message")
//                    .Where(e => Path.GetFileName(e) == "_timeout.message")
//                    .ToArray();
//        }
//    }
//}
