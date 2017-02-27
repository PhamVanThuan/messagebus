using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YmatouMQ.ClientNet45
{
    static class _Extensions
    {       
        public static string F(this string val, params object[] args)
        {
            if (string.IsNullOrEmpty(val)) return val;
            return string.Format(val, args);
        }
        public static byte[] ToByte(this string Val)
        {
            if (string.IsNullOrEmpty(Val)) return new byte[] { 0 };
            return Encoding.GetEncoding("utf-8").GetBytes(Val);
        }
        public static string ToJson<T>(this T val, string defVal = null)
        {
            if (val == null) return defVal;
            if (typeof(T) == typeof(string)) return val.ToString();
            var format = new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                TypeNameHandling = TypeNameHandling.None,
            };

            return JsonConvert.SerializeObject(val, format);
        }
        public static T FromJsonTo<T>(this string json, T defVal = default (T))
        {
            if (string.IsNullOrEmpty(json)) return defVal;
            var format = new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                TypeNameHandling = TypeNameHandling.None
            };
            try
            {
                return (T)JsonConvert.DeserializeObject(json, typeof(T), format);
            }
            catch
            {
                return defVal;
            }
        }
        /// <summary>
        /// 空对象替换.如果val为空，则使用默认的值替换
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="val">val</param>
        /// <param name="action">回调操作</param>
        public static void NullObjectReplace<T>(this T val, Action<T> action, T defVal = null) where T : class
        {
            if (val != null)
                action(val);
            else
            {
                if (defVal != null)
                    action(defVal);
            }
        }
        /// <summary>
        /// 空对象替换.如果val为空，则使用默认的值替换
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="val"></param>
        /// <param name="action"></param>
        /// <param name="defVal"></param>
        /// <returns></returns>
        public static T NullObjectReplace<T>(this T val, Func<T, T> action, T defVal = null) where T : class
        {
            if (val != null)
                return val;
            else
            {
                if (defVal != null)
                    return action(defVal);
            }
            return default(T);
        }
        /// <summary>
        /// 空连接符操作
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="val">val</param>
        /// <param name="action">回调操作</param>
        public static void NotNullAction<T>(this Nullable<T> val, Action<T> action, Nullable<T> defVal = null) where T : struct
        {
            if (val != null && val.HasValue)
                action(val.Value);
            else
            {
                if (defVal != null && defVal.HasValue)
                    action(defVal.Value);
            }
        }
        public static TResult NullAction<T, TResult>(this Nullable<T> val, Func<T, TResult> action, Nullable<T> defVal = null) where T : struct
        {
            if (val != null && val.HasValue)
                return action(val.Value);
            else
            {
                if (defVal != null && defVal.HasValue)
                    return action(defVal.Value);
            }
            return default(TResult);
        }
        public static DateTime ToDateTime(this long ticks)
        {
            return new DateTime(ticks);
        }
        public static IEnumerable<TResult> ConvertAll<T, TResult>(this IEnumerable<T> val, Func<T, TResult> action)
        {
            var list = new List<TResult>();
            foreach (var itme in val)
            {
                if (itme != null)
                {
                    list.Add(action(itme));
                }
            }
            return list;
        }
        public static int TryToInt32(this string val, int defVal)
        {
            int result = 0;
            if (int.TryParse(val, out result))
            {
                return result;
            }
            else return result;
        }
        public static void TryForeach<T>(this IEnumerable<T> val, Action<T> action, Action<Exception> errorHandler = null)
        {
            foreach (var item in val)
            {
                try
                {
                    action(item);
                }
                catch (Exception ex)
                {
                    if (errorHandler != null) errorHandler(ex);
                }
            }
        }
        public static T ConvertTo<T>(this object v, T defVal = default(T))
        {
            if (v == null) return defVal;
            //目标类型
            var type = typeof(T);
            var valueType = v.GetType();

            if (type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                type = Nullable.GetUnderlyingType(type);
            }

            if (type == typeof(String))
            {
                return (T)((Object)v.ToString());
            }
            else if (type == typeof(TimeSpan))
            {
                TimeSpan ts;
                if (TimeSpan.TryParse(v.ToString(), out ts))
                {
                    return (T)((object)ts);
                }
                else
                {
                    return defVal;
                }
            }
            else if (type == typeof(DateTime))
            {
                if (valueType == typeof(Int64))
                {
                    try
                    {
                        return (T)((Object)Convert.ToDateTime(Convert.ToInt64(v)));
                    }
                    catch
                    {
                        return defVal;
                    }
                }
                if (valueType == typeof(string))
                {
                    try
                    {
                        DateTime tmpTime;
                        if (DateTime.TryParse(v.ToString(), out tmpTime))
                        {
                            return (T)((Object)tmpTime);
                        }
                    }
                    catch
                    {
                        return defVal;
                    }
                }
            }
            else if (type.IsEnum)
            {
                try
                {
                    var flagsAttribute = type.GetCustomAttributes(false).OfType<FlagsAttribute>().SingleOrDefault();
                    if (flagsAttribute != null)
                        return (T)Enum.Parse(type, v.ToString(), true);
                    else
                        return (T)Enum.Parse(type, v.ToString(), true);

                }
                catch
                {

                    return defVal;
                }
            }
            else if (type == typeof(bool))
            {
                bool b;
                if (bool.TryParse(v.ToString(), out b))
                {
                    return (T)((object)b);
                }
                else
                {
                    return defVal;
                }
            }
            else if (type == typeof(Type))
            {
                try
                {
                    return (T)((Object)Type.GetType(valueType.FullName));
                }
                catch
                {
                    return defVal;
                }
            }

            try
            {
                return (T)Convert.ChangeType(v, type);
            }
            catch
            {
                return defVal;
            }
        }
        public static Task WithHandler(this Task task, Action<AggregateException> errorAction, Action successAction)
        {
            //if (task.Status == TaskStatus.RanToCompletion) return task;
            return task.ContinueWith(r =>
            {
                if (r.Status == TaskStatus.Faulted || r.Status == TaskStatus.Canceled || r.Exception != null) errorAction(r.Exception);
                if (r.Status == TaskStatus.RanToCompletion) successAction();
            }/*, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously*/);
        }
    }
}
