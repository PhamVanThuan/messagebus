namespace YmatouMessageBusClientNet4.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    public static class DictionaryExpand
    {
        private static ReaderWriterLockSlim rs = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        public static Dictionary<K, V> TryAddOrSet<K, V>(this Dictionary<K, V> dic, K k, V v, Func<V, bool> fn = null, bool throwOut = false)
        {
            rs.EnterWriteLock();
            try
            {
                if (fn != null)
                {
                    if (fn(v))
                        dic[k] = v;
                }
                else
                {
                    dic[k] = v;
                }
                return dic;
            }
            catch (Exception)
            {
                if (throwOut) throw;
                return dic;
            }
            finally
            {
                rs.ExitWriteLock();
            }
        }

        public static V TryGetVal<K, V>(this Dictionary<K, V> dic, K k, V defV = default(V), bool notFindThrowOut = false)
        {
            if (dic == null) return defV;
            rs.EnterReadLock();
            try
            {
                V v;
                if (dic.TryGetValue(k, out v))
                {
                    return v;
                }
                else
                {
                    if (notFindThrowOut)
                    {
                        throw new KeyNotFoundException(string.Format("未找到 {0}", k));
                    }
                    else
                        return defV;
                }
            }
            finally
            {
                rs.ExitReadLock();
            }
        }
      
        public static Dictionary<K, V> TryRemove<K, V>(this Dictionary<K, V> dic, K k)
        {
            rs.EnterWriteLock();
            try
            {
                dic.Remove(k);
                return dic;
            }
            finally
            {
                rs.ExitWriteLock();
            }
        }

        public static Dictionary<K, V> TryRemove<K, V>(this Dictionary<K, V> dic, Func<K, V, bool> where)
        {
            rs.EnterWriteLock();
            try
            {
                var tmp = new Dictionary<K, V>();
                foreach (var item in dic)
                {
                    tmp.Add(item.Key, item.Value);
                }
                foreach (var item in dic)
                {
                    if (where(item.Key, item.Value))
                    {
                        tmp.Remove(item.Key);
                    }
                }
                dic = tmp;
                return dic;
            }
            finally
            {
                rs.ExitWriteLock();
            }
        }
    }
}