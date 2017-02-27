//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using YmatouMessageBusClientNet4.Extensions;

//namespace YmatouMessageBusClientNet4.Persistent
//{
//    public class GtQueueLimitMessagePersistent : MessagePersistentBase<_Message>
//    {
//        public override string[] GetAllMessageBusFile()
//        {
//            if (!Directory.Exists(this.dir)) return new string[0];
//            return Directory
//                   .GetFiles(this.dir, "*.gtqueuelimit.message")
//                   .Where(e => Path.GetFileName(e) != "_retry_1.message"
//                       && Path.GetFileName(e) != "_timeout.message"
//                       && !Path.GetFileName(e).Contains("gtqueuelimit_del"))
//                   .ToArray();
//        }

//        public void SetFilePath(string path, string fileName)
//        {
//            base.SetFilePath(path, fileName, MessageType.NotRetry);
//        }
//        public int GetMaxIndex()
//        {
//            if (!Directory.Exists(this.dir)) return 0;
//            var allPath = GetAllMessageBusFile();
//            if (allPath == null || allPath.Length <= 0) return 0;
//            return allPath
//                   .Select(e => Path.GetFileNameWithoutExtension(e).Split(new char[] { '.' })[0])
//                   .ConvertAll(e => e.TryToInt32(0))
//                   .Max();
//        }
//        public string GetMinIndexFilePath()
//        {
//            var minIndex = GetMinIndex();
//            var minIndexFileName = "{0}.gtqueuelimit.message".F(minIndex);
//            var allPath = GetAllMessageBusFile();
//            return allPath.FirstOrDefault(e => Path.GetFileName(e) == minIndexFileName);
//        }
//        public string GetMaxIndexFilePath()
//        {
//            var maxIndex = GetMaxIndex();
//            var maxIndexFileName = "{0}.gtqueuelimit.message".F(maxIndex);
//            var allPath = GetAllMessageBusFile();
//            return allPath.FirstOrDefault(e => Path.GetFileName(e) == maxIndexFileName);
//        }
//        public int GetMinIndex()
//        {
//            if (!Directory.Exists(this.dir)) return 0;
//            var allPath = GetAllMessageBusFile();
//            if (allPath == null || allPath.Length <= 0) return 0;
//            return allPath
//                   .Select(e => Path.GetFileNameWithoutExtension(e).Split(new char[] { '.' })[0])
//                   .ConvertAll(e => e.TryToInt32(0))
//                   .Min();
//        }
//        public void MarkFileUsed(string oldFullName)
//        {
//            if (!File.Exists(oldFullName)) return;
//            var newName = "{0}.{1}.message".F(Path.GetFileNameWithoutExtension(oldFullName), "del");
//            var newFullName = Path.Combine("{0}".F(Path.GetDirectoryName(oldFullName)), newName);
//            File.Move(oldFullName, newFullName);
//        }
//        public void Delete(string fileFullName)
//        {
//            if (!File.Exists(fileFullName)) return;
//            File.Delete(fileFullName);
//        }
//        public void DeleteAllMarkDelFile()
//        {
//            GetAllMessageBusFile()
//                .Where(e => Path.GetFileName(e).Contains(".del."))
//                .TryForeach(e => File.Delete(e));
//        }
//    }
//}
