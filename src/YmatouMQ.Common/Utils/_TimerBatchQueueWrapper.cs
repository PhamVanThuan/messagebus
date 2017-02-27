using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using YmatouMQ.Common.Extensions._Task;

namespace YmatouMQ.Common.Utils
{
    public class _TimerBatchQueueWrapper<T>
    {
        private readonly BlockingCollection<T> queue;
        private readonly SemaphoreSlim slim;
        private readonly int timer_CycleMilliseconds;
        private readonly int batch_Size;
        private readonly int max;
        private readonly int addTimeOutMillisecondes;
        private bool isrun;
        private Timer timer;
        private Func<IEnumerable<T>, CancellationToken, Task> action;
        private Action<Exception> errorHandle;

        public _TimerBatchQueueWrapper(int timer_CycleMilliseconds, int batch_Size, int max,
            Func<IEnumerable<T>, CancellationToken, Task> action
            , int concurrent = 1, Action<Exception> errorHandle = null, int sendTimeOutMilliseconds = 3000)
        {
            this.timer_CycleMilliseconds = timer_CycleMilliseconds;
            this.max = max;
            this.batch_Size = batch_Size;
            this.action = action;
            this.queue = new BlockingCollection<T>(this.max);
            this.slim = new SemaphoreSlim(concurrent, concurrent);
            this.errorHandle = errorHandle;
            this.addTimeOutMillisecondes = sendTimeOutMilliseconds;
        }

        public async Task<bool> SendAsync(T item, int addTimeOutMillisecondes)
        {
            Func<bool> _action = () => queue.TryAdd(item, addTimeOutMillisecondes);
            return await _action.ExecuteSynchronously().ConfigureAwait(false);
        }

        public async Task<bool> SendAsync(T item)
        {
            Func<bool> _action = () => queue.TryAdd(item, addTimeOutMillisecondes);
            return await _action.ExecuteSynchronously().ConfigureAwait(false);
        }

        public int Count
        {
            get { return queue.Count; }
        }

        public void Start()
        {
            isrun = true;
            timer = new Timer(async o =>
            {
                if (!isrun) return;
                await TryExecuted().ConfigureAwait(false);
                //
                timer.Change(timer_CycleMilliseconds, Timeout.Infinite);
            }, null, Timeout.Infinite, Timeout.Infinite);
            timer.Change(0, Timeout.Infinite);
        }

        public void Stop()
        {
            queue.CompleteAdding();
            isrun = false;
            var task = TryExecuted();
        }

        private async Task TryExecuted()
        {
            if (queue.Count > 0)
            {
                await slim.WaitAsync();
                try
                {
                    var count = 0;
                    var list = new ConcurrentBag<T>();
                    while (queue.Count > 0
                           && Interlocked.Increment(ref count) <= batch_Size)
                    {
                        T item;
                        if (queue.TryTake(out item))
                        {
                            list.Add(item);
                        }
                    }
                    if (list.Count > 0)
                    {
                        var cts = new CancellationTokenSource(5000);
                        await action(list, cts.Token);
                    }

                }
                catch (Exception ex)
                {
                    if (errorHandle != null) errorHandle(ex);
                }
                finally
                {
                    slim.Release();
                }
            }
        }
    }
}
