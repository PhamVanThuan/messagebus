using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ServiceStack.Text;
using YmatouMQNet4.Extensions;
using YmatouMQNet4.Extensions._Task;
using Newtonsoft.Json;
using JsonSerializer = ServiceStack.Text.JsonSerializer;
namespace YmatouMQNet4.Extensions.Serialization
{
    public static class MessageSerialization
    {
        public static string ReadAsString(this Stream stream, string encodingName = "utf-8", string defReturn = null)
        {
            if (stream == null || !stream.CanRead) return defReturn;
            using (var _stream = new StreamReader(stream, Encoding.GetEncoding(encodingName)))
            {
                return _stream.ReadToEnd();
            }
        }
        public static string JSONSerializationToString<T>(this T value)
        {
            JsConfig.IncludeNullValues = false;
            JsConfig.IncludeTypeInfo = false;
            JsConfig.ExcludeTypeInfo = true;
            return JsonSerializer.SerializeToString<T>(value);
        }
        public static Task<string> JSONSerializationToStringAsync<T>(this T value)
        {
            Func<string> fn = () => value.JSONSerializationToString<T>();
            return fn.ExecuteSynchronously();
        }
        public static byte[] JSONSerializationToByte<T>(this T value)
        {
            using (var stream = new MemoryStream())
            {
                JsConfig.IncludeNullValues = false;
                JsConfig.IncludeTypeInfo = false;
                JsConfig.ExcludeTypeInfo = true;
                JsonSerializer.SerializeToStream<T>(value, stream);
                return stream.ToArray();
            }
        }
        public static T JSONDeserializeFromString<T>(this string value)
        {
            if (string.IsNullOrEmpty(value)) return default(T);
            JsConfig.IncludeTypeInfo = false;
            JsConfig.ExcludeTypeInfo = true;
            JsConfig.IncludeNullValues = false;
            return JsonSerializer.DeserializeFromString<T>(value);
        }
        public static T JSONDeserializeFromStream<T>(this Stream value)
        {
            return JsonSerializer.DeserializeFromStream<T>(value);
        }
        public static Task<T> JSONDeserializeFromStreamAsync<T>(this Stream value)
        {
            Func<T> fn = () => value.JSONDeserializeFromStream<T>();
            return fn.ExecuteSynchronously();
        }
        public static T _JSONDeserializeFromStream<T>(this Stream value)
        {
            using (var stream = new StreamReader(value, Encoding.GetEncoding("utf-8")))
            {
                var jsonString = stream.ReadToEnd();
                Console.WriteLine(jsonString);
                _JsonSettings();
                #region
                //var jsonReader = new JsonTextReader(stream);
                //var ser = new Newtonsoft.Json.JsonSerializer();
                //return ser.Deserialize<T>(jsonReader);
                #endregion
                return JsonConvert.DeserializeObject<T>(jsonString);
            }
        }
        public static T _JSONDeserializeFromString<T>(this string value)
        {
            if (value.IsEmpty()) return default(T);
            _JsonSettings();
            return JsonConvert.DeserializeObject<T>(value);
        }
        public static byte[] _JSONSerializationToByte<T>(this T value)
        {
            _JsonSettings();

            return Encoding.GetEncoding("utf-8").GetBytes(JsonConvert.SerializeObject(value));
        }
        public static string _JSONSerializationToString<T>(this T value)
        {
            if (typeof(T) == typeof(string)) return value.ToString();
            _JsonSettings();

            return JsonConvert.SerializeObject(value);
        }
        private static void _JsonSettings()
        {
            JsonConvert.DefaultSettings = () =>
            {
                return new JsonSerializerSettings
                {
                    DefaultValueHandling = DefaultValueHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.None,
                    TypeNameHandling = TypeNameHandling.None
                };
            };
        }
        public static Task<T> JSONDeserializeFromStringAsync<T>(this string value)
        {
            Func<T> fn = () => value.JSONDeserializeFromString<T>();
            return fn.ExecuteSynchronously();
        }
        public static T JSONDeserializeFromByteArray<T>(this byte[] value)
        {
            if (value == null || value.Length == 0) return default(T);
            using (var stream = new MemoryStream(value))
            {
                JsConfig.IncludeTypeInfo = false;
                JsConfig.ExcludeTypeInfo = true;
                JsConfig.IncludeNullValues = false;
                return JsonSerializer.DeserializeFromStream<T>(stream);
            }
        }
        public static Task<byte[]> JSONSerializationToByteAsync<T>(this T value)
        {
            Func<byte[]> fu = () => value.JSONSerializationToByte();
            return fu.ExecuteASynchronously();
        }
        public static Task<T> JSONDeserializeFromByteArrayAsync<T>(this byte[] value)
        {
            if (value == null || value.Length == 0) return TaskHelpers.FromResult(default(T).CreateDefautl());
            Func<T> fn = () => value.JSONDeserializeFromByteArray<T>();
            return fn.ExecuteASynchronously();
        }
    }
}
