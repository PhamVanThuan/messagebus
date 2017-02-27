using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Collections.Concurrent;

namespace YmatouMQ.Common.Utils
{
    public class TimerBatchBlockWrapper<T>
    {
        private readonly BufferBlock<T> buffer;
        private readonly BatchBlock<T> batch;
        private readonly ActionBlock<IEnumerable<T>> action;
        private readonly int batchSize;
        private readonly int milliseconds;
        private readonly Action<Exception> errorHandle;
        private readonly int sendTimeOutMilliseconds;
        private readonly int max;
        private Timer timer;
        private bool isrun;

        public TimerBatchBlockWrapper(int milliseconds, int batchSize, Action<IEnumerable<T>> _action, int max = 500000
            , Action<Exception> errorHandle = null, int sendTimeOutMilliseconds = 3000, Action sendTimeOutCallback = null)
            : this(milliseconds, batchSize, max, errorHandle, sendTimeOutMilliseconds, sendTimeOutCallback)
        {
            this.action = new ActionBlock<IEnumerable<T>>(_action, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = DataflowBlockOptions.Unbounded });
            //this.batch.LinkTo(this.action);
        }
        public TimerBatchBlockWrapper(int milliseconds, int batchSize, Func<IEnumerable<T>, Task> _action, int max = 500000
            , Action<Exception> errorHandle = null, int sendTimeOutMilliseconds = 3000, Action sendTimeOutCallback = null)
            : this(milliseconds, batchSize, max, errorHandle, sendTimeOutMilliseconds, sendTimeOutCallback)
        {
            this.action = new ActionBlock<IEnumerable<T>>(_action, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = DataflowBlockOptions.Unbounded });
            //this.batch.LinkTo(this.action);
        }
        private TimerBatchBlockWrapper(int milliseconds, int batchSize, int max = 500000, Action<Exception> errorHandle = null
            , int sendTimeOutMilliseconds = 3000, Action sendTimeOutCallback = null)
        {
            this.milliseconds = milliseconds;
            this.batchSize = batchSize;
            this.errorHandle = errorHandle;
            this.max = max;
            this.sendTimeOutMilliseconds = sendTimeOutMilliseconds;
            this.buffer = new BufferBlock<T>(new DataflowBlockOptions { MaxMessagesPerTask = Environment.ProcessorCount * 5, BoundedCapacity = max });
            this.batch = new BatchBlock<T>(batchSize, new GroupingDataflowBlockOptions
            {
                Greedy = true,
                MaxMessagesPerTask = DataflowBlockOptions.Unbounded,
                BoundedCapacity = max
            });
            this.buffer.LinkTo(this.batch);
        }

        public async Task SendAsync(T t)
        {
            if (buffer.Count < max)
                await buffer.SendAsync(t).ConfigureAwait(false);
            else
            {
                var cts = new CancellationTokenSource(sendTimeOutMilliseconds);
                var token = cts.Token;
                await buffer.SendAsync(t, token).ConfigureAwait(false);
            }
        }
        public void Send(T t)
        {
            buffer.Post(t);
        }
        public void ReceiveAsync()
        {
            isrun = true;
            timer = new Timer(async o =>
            {
                if (!isrun) return;
                await TryExecuted().ConfigureAwait(false);
                //
                timer.Change(milliseconds, Timeout.Infinite);
            }, null, Timeout.Infinite, Timeout.Infinite);
            timer.Change(0, Timeout.Infinite);
        }
        public async Task SendAsync(T t, CancellationToken token)
        {
            await buffer.SendAsync(t, token);
        }
        public int Count { get { return batch.OutputCount; } }
        private async Task TryExecuted()
        {
            try
            {
                if (batch.OutputCount == 0)
                    batch.TriggerBatch();

                var cts = new CancellationTokenSource(sendTimeOutMilliseconds);
                var list = await batch.ReceiveAsync(cts.Token).ConfigureAwait(false);
                if (list != null && list.Any())
                {
                    var _cts = new CancellationTokenSource(sendTimeOutMilliseconds);
                    await action.SendAsync(list, _cts.Token).ConfigureAwait(false);
                }
            }
            catch
            {
                //
            }
        }
        public void Complete(TimeSpan timeOut)
        {
            var e = TryExecuted();
            buffer.Complete();
            batch.Complete();
            action.Complete();
            isrun = false;
            try
            {
                Task.WhenAll(buffer.Completion, batch.Completion, action.Completion).ConfigureAwait(false);
                //timer.Dispose();
            }
            catch (AggregateException ex)
            {
                if (errorHandle != null)
                {
                    foreach (var _e in ex.InnerExceptions)
                    {
                        errorHandle(_e);
                    }
                }
            }
            catch (Exception ex)
            {
                if (errorHandle != null)
                    errorHandle(ex);
            }
        }
    }
}
