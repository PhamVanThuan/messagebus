using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using ServiceStack.Text;
using ServiceStack.Text.Json;

namespace YmatouMQAdmin.WebApp2.ContextType
{
    #region
    //public class DtoJsonFormart : JsonMediaTypeFormatter
    //{
    //    public override JsonWriter CreateJsonWriter(Type type, Stream writeStream, Encoding effectiveEncoding)
    //    {
    //        JsonConvert.DefaultSettings = () =>
    //        {
    //            return new JsonSerializerSettings
    //            {
    //                DefaultValueHandling = DefaultValueHandling.Ignore,
    //                NullValueHandling = NullValueHandling.Ignore,
    //            };
    //        };
    //        var writer = new JsonTextWriter(new StreamWriter(writeStream, effectiveEncoding));
    //        return writer;
    //    }
    //    public override JsonReader CreateJsonReader(Type type, Stream readStream, Encoding effectiveEncoding)
    //    {
    //        JsonConvert.DefaultSettings = () =>
    //        {
    //            return new JsonSerializerSettings
    //            {
    //                DefaultValueHandling = DefaultValueHandling.Ignore,
    //                NullValueHandling = NullValueHandling.Ignore,
    //            };
    //        };
    //        var reader = new JsonTextReader(new StreamReader(readStream, effectiveEncoding));
    //        return reader;
    //    }
    //}
    #endregion
    public class ServiceStackTextFormatter : MediaTypeFormatter
    {
        public ServiceStackTextFormatter()
        {
            JsConfig.DateHandler = DateHandler.ISO8601;
            JsConfig.IncludeNullValues = false;
            JsConfig.IncludeTypeInfo = false;
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/json"));

            SupportedEncodings.Add(new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true));
            SupportedEncodings.Add(new UnicodeEncoding(bigEndian: false, byteOrderMark: true, throwOnInvalidBytes: true));
        }

        public override bool CanReadType(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");
            return true;
        }

        public override bool CanWriteType(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");
            return true;
        }

        public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, System.Net.Http.HttpContent content, IFormatterLogger formatterLogger)
        {
            var task = Task<object>.Factory.StartNew(() => JsonSerializer.DeserializeFromStream(type, readStream));
            return task;
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, System.Net.Http.HttpContent content, TransportContext transportContext)
        {
            var task = Task.Factory.StartNew(() => JsonSerializer.SerializeToStream(value, type, writeStream));
            return task;
        }
    }    
}