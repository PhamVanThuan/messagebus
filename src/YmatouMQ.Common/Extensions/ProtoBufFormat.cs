using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ProtoBuf.Meta;

namespace YmatouMQ.Common.Extensions
{
    class ProtoBufFormat
    {
        [ThreadStatic]
        private static RuntimeTypeModel model;
        public static RuntimeTypeModel Model
        {
            get { return model ?? (model = TypeModel.Create()); }
        }       

        public static void Serialize(object dto, Stream outputStream)
        {
            Model.Serialize(outputStream, dto);
        }

        public static object Deserialize(Type type, Stream fromStream)
        {
            var obj = Model.Deserialize(fromStream, null, type);
            return obj;
        }
    }

    public static class ProtoBufExtensions 
    {
        public static byte[] ToProtoBuf<T>(this T obj)
        {
            using (var ms = new MemoryStream())
            {
                ProtoBufFormat.Serialize(obj, ms);
                var bytes = ms.ToArray();
                return bytes;
            }
        }

        public static T FromProtoBuf<T>(this byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
            {
                var obj = (T)ProtoBufFormat.Deserialize(typeof(T), ms);
                return obj;
            }
        }
    }
}
