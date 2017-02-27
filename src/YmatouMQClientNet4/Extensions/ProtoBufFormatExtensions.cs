using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
//using ProtoBuf.Meta;
using YmatouMessageBusClientNet4.Persistent;

namespace YmatouMessageBusClientNet4.Extensions
{
    class ProtoBufFormat
    {
        //[ThreadStatic]
        //private static RuntimeTypeModel model;
        //public static RuntimeTypeModel Model
        //{
        //    get { return model ?? (model = TypeModel.Create()); }
        //}
        //public static RuntimeTypeModel CreateModel() 
        //{
        //    //if (model == null) 
        //    //{
        //    //    model = TypeModel.Create();
        //    //}
        //    //model.Add(typeof(MessagePersistent[]), false);
        //    //model.Compile();

        //    //return model;
        //    throw new NotImplementedException();
        //}
        public static void Serialize(object dto, Stream outputStream)
        {
            //Model.Serialize(outputStream, dto);
            throw new NotImplementedException();
        }

        public static object Deserialize(Type type, Stream fromStream)
        {
            //var obj = Model.Deserialize(fromStream, null, type);
            //return obj;
            throw new NotImplementedException();
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

        public static T FromProtoBuf<T>(this Stream stream) 
        {
            return (T)ProtoBufFormat.Deserialize(typeof(T), stream); ;
        }
    }
}
