using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;

namespace YmatouMQ.Common.Extensions
{
    public static class _MQGeneralExtensions
    {
        public static string Fomart(this string val, params object[] args)
        {
            return string.Format(val, args);
        }
        public static T GetAppSettings<T>(this string key, Func<string, T> convert, T defVal = default(T))
        {
            var cfgVal = ConfigurationManager.AppSettings[key];
            if (string.IsNullOrEmpty(cfgVal)) return defVal;
            return convert(cfgVal);
        }
        public static string GetAppSettings(this string key, string defVal = null)
        {
            var cfgVal = ConfigurationManager.AppSettings[key];
            if (string.IsNullOrEmpty(cfgVal)) return defVal;
            return cfgVal;
        }
        public static string GetString(this byte[] by)
        {
            return System.Text.Encoding.GetEncoding("utf-8").GetString(by);
        }
        /// <summary>
        /// 判断对象值是否是默认值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static bool IsDefaultValue<T>(this T t, T val)
        {
            if (t.Equals(val)) return true;
            else return false;
        }
        /// <summary>
        /// 检查对象是否是为空
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        public static bool IsNull<T>(this T t)
        {
            if (t == null) return true;
            else return false;
        }
        /// <summary>
        /// cas 操作
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="targent"></param>
        /// <param name="source"></param>
        /// <param name="cmp"></param>
        /// <returns></returns>
        public static T Cas<T>(this Func<T> targent, Func<T> source, Func<T> cmp) where T : class
        {
            var t = targent();
            return Interlocked.CompareExchange(ref t, source(), cmp());
        }
        /// <summary>
        /// 创建默认值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="v"></param>
        /// <returns></returns>
        public static T CreateDefautl<T>(this T v)
        {
            return v;
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
        /// 满足条件，则执行action
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="action"></param>
        /// <param name="condition"></param>
        public static void ConditionAction<T>(this T obj, Action action, Func<bool> condition)
        {
            if (condition())
            {
                action();
            }
        }
        public static T ConditionAction<T>(this T obj, Func<T> action, Func<bool> condition)
        {
            if (condition())
            {
               return action();
            }
            return obj;
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
        /// <summary>
        /// 验证并返回。如果val 为空则返回，默认的值，否则返回 val
        /// </summary> 
        /// <typeparam name="TResult"></typeparam>
        /// <param name="val">要验证的值</param>
        /// <param name="defReruenVal">默认返回值</param>
        /// <returns></returns>
        public static TResult VerifyAndReturn<TResult>(this TResult val, TResult defReruenVal = default(TResult))
        {
            if (val == null) return defReruenVal;
            else return val;
        }
        public static bool IsEmpty(this string value)
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value))
                return true;
            else return false;
        }
        public static int ToInt32(this string value, int defaultVal)
        {
            if (string.IsNullOrWhiteSpace(value)) return defaultVal;
            int val;
            if (int.TryParse(value, out val)) { return val; }
            return defaultVal;
        }
        public static int? ToInt32(this string value, int? defaultVal)
        {
            if (string.IsNullOrWhiteSpace(value)) return defaultVal;
            int val;
            if (int.TryParse(value, out val)) { return val; }
            return defaultVal;
        }
        public static uint? ToUInt32(this string value, uint? defaultVal)
        {
            if (string.IsNullOrWhiteSpace(value)) return defaultVal;
            uint val;
            if (uint.TryParse(value, out val)) { return val; }
            return defaultVal;
        }
        public static bool ToBoole(this string value, bool defaultVal)
        {
            if (string.IsNullOrEmpty(value)) return defaultVal;
            if (value == "1") return true;
            if (value == "0") return true;
            bool val;
            if (bool.TryParse(value, out val))
            {
                return val;
            }
            else return defaultVal;
        }
        public static ushort? ToUshort(this string value, ushort? defaultVal)
        {
            if (string.IsNullOrEmpty(value)) return defaultVal;
            ushort val;
            if (ushort.TryParse(value, out val))
            {
                return val;
            }
            return defaultVal;
        }
        public static TimeSpan? ToTimeSpan(this string value, TimeSpan? defaultVal)
        {
            if (string.IsNullOrEmpty(value)) return defaultVal;
            TimeSpan val;
            if (TimeSpan.TryParse(value, out val))
            {
                return val;
            }
            return defaultVal;
        }
        public static byte ToByte(this int val, byte defaultVal)
        {
            try
            {
                return Convert.ToByte(val);
            }
            catch
            {
                return defaultVal;
            }
        }
        public static int ToInt32(this uint? value, int defVal)
        {
            if (value == null || !value.HasValue) return defVal;
            return Convert.ToInt32(value.Value);
        }
        public static int ToInt32(this uint? value)
        {
            return Convert.ToInt32(value.Value);
        }
        public static string TrySubString(this string val, int subLength, bool isByteLength = false, int startIndex = 0, string suffix = "...")
        {
            if (string.IsNullOrEmpty(val)) return val;
            if (subLength + startIndex > val.Length) return val;
            var isSub = false;
            if (isByteLength)
            {
                isSub = Encoding.UTF8.GetByteCount(val) > subLength ? true : false;

            }
            else
            {
                isSub = val.Length > subLength ? true : false;
            }
            if (isSub) return val.Substring(startIndex, subLength) + suffix;
            else return val;
        }
    }
}
