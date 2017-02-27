using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YmatouMQNet4.Extensions
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// 循环操作
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values">数据源</param>
        /// <param name="action">要执行的操作</param>
        /// <param name="parallel">是否并行</param>
        /// <param name="errorHandle">错误处理</param>
        public static void EachAction<T>(this IEnumerable<T> values, Action<T> action, bool parallel = false, Action<Exception> errorHandle = null)
        {
            if (!parallel)
            {
                if (values != null && values.Any())
                {
                    foreach (var item in values)
                        if (item != null)
                            action(item);
                }
            }
            else
            {
                Parallel.ForEach(values, action);
            }
        }
        public static void EachAction<T>(this IEnumerable<T> values, Func<T, Task> action, Action<Exception> errorHandle = null)
        {

            if (values != null && values.Any())
            {
                foreach (var item in values)
                    if (item != null)
                        action(item);
            }
        }
        public static IEnumerable<To> CopyTo<From, To>(this IEnumerable<From> values, Func<From, To> copyAction)
        {
            if (values != null && values.Any())
            {
                var list = new List<To>();
                foreach (var item in values)
                {
                    list.Add(copyAction(item));
                }
                return list;
            }
            return Enumerable.Empty<To>();
        }
    }
}
