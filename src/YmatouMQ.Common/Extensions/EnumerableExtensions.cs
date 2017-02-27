using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace YmatouMQ.Common.Extensions
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
        public static async Task EachActionAsync<T>(this IEnumerable<T> values, Func<T, Task> action, SemaphoreSlim slim, Action<Exception> errorHandle = null)
        {
            if (values != null && values.Any())
            {
                foreach (var item in values)
                {
                    if (item != null)
                    {
                        await slim.WaitAsync();
                        try
                        {
                            await action(item);
                        }
                        finally
                        {
                            slim.Release();
                        }
                    }
                }
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
        ///////////////////////////////////////////////////////////////////////////////////////////
        //http://blogs.msdn.com/b/pfxteam/archive/2012/03/05/10278165.aspx
        //http://blogs.msdn.com/b/pfxteam/archive/2012/03/04/10277325.aspx?PageIndex=2#comments
        public static Task ForEachAsync<TSource, TResult>(this IEnumerable<TSource> source
                                                        , Func<TSource, Task<TResult>> taskSelector
                                                        , Action<TSource, TResult> resultProcessor
                                                        , int concurrentLimit = 1)
        {
            var oneAtATime = new SemaphoreSlim(initialCount: concurrentLimit, maxCount: concurrentLimit);
            return Task.WhenAll(
                from item in source
                select ProcessAsync(item, taskSelector, resultProcessor, oneAtATime));
        }

        private static async Task ProcessAsync<TSource, TResult>(
            TSource item,
            Func<TSource, Task<TResult>> taskSelector, Action<TSource, TResult> resultProcessor,
            SemaphoreSlim oneAtATime)
        {
            TResult result = await taskSelector(item);
            await oneAtATime.WaitAsync();
            try { resultProcessor(item, result); }
            finally { oneAtATime.Release(); }
        }
        public static async Task ForEachAsync2<TSource, TResult>(this IEnumerable<TSource> source
                                                                , Func<TSource, Task<TResult>> taskSelector
                                                                , Action<TResult> resultProcessor)
        {
            var taskSelectorBlock = new TransformBlock<TSource, TResult>(taskSelector, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = DataflowBlockOptions.Unbounded/*Environment.ProcessorCount*/ });
            var resultProcessorBlock = new ActionBlock<TResult>(resultProcessor);

            taskSelectorBlock.LinkTo(resultProcessorBlock, new DataflowLinkOptions { PropagateCompletion = true });

            foreach (var item in source)
                await taskSelectorBlock.SendAsync(item).ConfigureAwait(false);

            //await source.ForEachAsync(5, async souce => await taskSelectorBlock.SendAsync(souce));

            taskSelectorBlock.Complete();
            await resultProcessorBlock.Completion;
        }
        public static Task ForEachAsync<T>(this IEnumerable<T> source, int dop, Func<T, Task> body)
        {
            //var partitons = new OrderableListPartitioner<T>(source.ToList());
            return Task.WhenAll(
                from partition in Partitioner.Create(source, EnumerablePartitionerOptions.NoBuffering).GetPartitions(dop)
                select Task.Run(async delegate
                {
                    using (partition)
                        while (partition.MoveNext())
                            await body(partition.Current);
                }));
        }
        /////////////////////////////////////////////////////////////////////////////////////////////
        public static bool IsEmptyEnumerable<T>(this IEnumerable<T> val)
        {
            if (val == null || !val.Any()) return true;
            return false;
        }
        public static TResult TryExecute<TResult>(this Func<TResult> func, TResult def = default(TResult), ILog log = null, string decscript = null)
        {
            try
            {
                return func();
            }
            catch (Exception ex)
            {
                if (log != null)
                    log.Error(decscript, ex);
                return def;
            }
        }
        public static void TryExecute(this Action func, ILog log = null, string decscript = null)
        {
            try
            {
                func();
            }
            catch (Exception ex)
            {
                if (log != null)
                    log.Error(decscript, ex);
            }
        }
        public static IEnumerable<T> DistinctBy<T>(this IEnumerable<T> source)
        {
            var hashSet = new HashSet<T>();
            foreach (var item in source)
            {
                if (hashSet.Add(item))
                    yield return item;
            }
        }
        public static IEnumerable<T> DistinctBy<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer)
        {
            var hashSet = new HashSet<T>(comparer);
            foreach (var item in source)
            {
                if (hashSet.Add(item))
                    yield return item;
            }
        }
        public static IEnumerable<T> DistinctBy<T, Key>(this IEnumerable<T> source, Func<T, Key> keySelect)
        {
            var hashSet = new HashSet<Key>();
            foreach (var item in source)
            {
                if (hashSet.Add(keySelect(item)))
                    yield return item;
            }
        }
        public static IEnumerable<T> DistinctBy<T, Key>(this IEnumerable<T> source, Func<T, Key> keySelect, IEqualityComparer<Key> comparer)
        {
            var hashSet = new HashSet<Key>(comparer);
            foreach (var item in source)
            {
                if (hashSet.Add(keySelect(item)))
                    yield return item;
            }
        }
    }
    //https://msdn.microsoft.com/zh-cn/library/dd997416(v=vs.110).aspx
    class OrderableListPartitioner<TSource> : OrderablePartitioner<TSource>
    {
        private readonly IList<TSource> m_input;

        public OrderableListPartitioner(IList<TSource> input)
            : base(true, false, true)
        {
            m_input = input;
        }

        // Must override to return true.
        public override bool SupportsDynamicPartitions
        {
            get
            {
                return true;
            }
        }

        public override IList<IEnumerator<KeyValuePair<long, TSource>>>
            GetOrderablePartitions(int partitionCount)
        {
            var dynamicPartitions = GetOrderableDynamicPartitions();
            var partitions =
                new IEnumerator<KeyValuePair<long, TSource>>[partitionCount];

            for (int i = 0; i < partitionCount; i++)
            {
                partitions[i] = dynamicPartitions.GetEnumerator();
            }
            return partitions;
        }

        public override IEnumerable<KeyValuePair<long, TSource>>
            GetOrderableDynamicPartitions()
        {
            return new ListDynamicPartitions(m_input);
        }

        private class ListDynamicPartitions
            : IEnumerable<KeyValuePair<long, TSource>>
        {
            private IList<TSource> m_input;
            private int m_pos = 0;

            internal ListDynamicPartitions(IList<TSource> input)
            {
                m_input = input;
            }

            public IEnumerator<KeyValuePair<long, TSource>> GetEnumerator()
            {
                while (true)
                {
                    // Each task gets the next item in the list. The index is 
                    // incremented in a thread-safe manner to avoid races.
                    int elemIndex = Interlocked.Increment(ref m_pos) - 1;

                    if (elemIndex >= m_input.Count)
                    {
                        yield break;
                    }

                    yield return new KeyValuePair<long, TSource>(
                        elemIndex, m_input[elemIndex]);
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return
                   ((IEnumerable<KeyValuePair<long, TSource>>)this)
                   .GetEnumerator();
            }
        }
    }
}
