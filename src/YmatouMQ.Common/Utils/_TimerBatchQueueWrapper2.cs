using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YmatouMQ.Common.Extensions;
using YmatouMQ.Common.Extensions._Task;

namespace YmatouMQ.Common.Utils
{
    public class _TimerBatchRetryEnQueueWrapper2<T>
    {
        public class Strategy
        {
            public int timer_CycleMilliseconds { get; set; }
            public int batch_Size { get; set; }
            public int max { get; set; }
            public int addTimeOutMillisecondes { get; set; }
            public Func<IEnumerable<T>, CancellationToken, Task<IEnumerable<T>>> action { get; set; }
            public Action<Exception> errorHandle { get; set; }
            public int concurrent { get; set; }
        }

        private readonly Strategy strategy;
        private readonly BlockingCollection<T> queue;
        private readonly SemaphoreSlim slim;
        private readonly object obj=new object();
        private bool isrun;
        private Timer timer;

        public _TimerBatchRetryEnQueueWrapper2(Strategy strategy)
        {
            this.strategy = strategy;
            this.slim = new SemaphoreSlim(strategy.concurrent, strategy.concurrent);
            this.queue = new BlockingCollection<T>(strategy.max);
        }

        public async Task<bool> SendAsync(T item, int addTimeOutMillisecondes)
        {
            Func<bool> _action = () => queue.TryAdd(item, addTimeOutMillisecondes);
            return await _action.ExecuteSynchronously().ConfigureAwait(false);
        }

        public async Task<bool> SendAsync(T item)
        {
            Func<bool> _action = () => queue.TryAdd(item, this.strategy.addTimeOutMillisecondes);
            return await _action.ExecuteSynchronously().ConfigureAwait(false);
        }

        public int Count
        {
            get { return queue.Count; }
        }

        public void Start()
        {
            if (!isrun)
            {
                lock (obj)
                {
                    if (!isrun)
                    {
                        isrun = true;
                        timer = new Timer(async o =>
                        {
                            if (!isrun) return;
                            await TryExecuted().ConfigureAwait(false);
                            //
                            timer.Change(this.strategy.timer_CycleMilliseconds, Timeout.Infinite);
                        }, null, Timeout.Infinite, Timeout.Infinite);
                        timer.Change(0, Timeout.Infinite);
                    }
                }
            }
        }

        public void Stop()
        {
            queue.CompleteAdding();
            isrun = false;
            var task = TryExecuted();
        }

        private async Task TryExecuted()
        {
            if (queue.Count <= 0)
                return;
            await slim.WaitAsync();
            var list = new ConcurrentBag<T>();
            try
            {
                var count = 0;
                while (queue.Count > 0
                       && Interlocked.Increment(ref count) <= this.strategy.batch_Size)
                {
                    T item;
                    if (queue.TryTake(out item))
                    {
                        list.Add(item);
                    }
                }
                if (list.Count <= 0)
                    return;
                var cts = new CancellationTokenSource(5000);
                var handleResult = await this.strategy.action(list, cts.Token).ConfigureAwait(false);
                RetryEnqueue(handleResult);
            }
            catch (Exception ex)
            {
                RetryEnqueue(list);
                if (this.strategy.errorHandle != null)
                    this.strategy.errorHandle(ex);
            }
            finally
            {
                slim.Release();
            }
        }

        private void RetryEnqueue(IEnumerable<T> handleResult)
        {
            if (handleResult != null && handleResult.Any())
            {
                handleResult.EachAction(item => queue.TryAdd(item, 3000));
            }
        }
    }
}
