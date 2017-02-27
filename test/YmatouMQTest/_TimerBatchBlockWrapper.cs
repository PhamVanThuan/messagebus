using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Reactive.Concurrency;
using System.Threading;
using System.Reactive.Linq;
using System.Reactive;

namespace YmatouMQTest
{
    public class _TimerBatchBlockWrapper<T>
    {
        private readonly CancellationTokenSource ct;
        private readonly TimeSpan timeOut;
        private readonly BatchBlock<T> bBlock;
        private readonly ActionBlock<IEnumerable<T>> aBlock;
        private readonly BufferBlock<T> buffer;
        private readonly TransformBlock<T, T> timerBlock;
        private Timer timer;
        private readonly IObservable<TimeInterval<long>> obser;
        public _TimerBatchBlockWrapper(TimeSpan ts, int batchSize, Action<IList<IEnumerable<T>>> _action)
        {
            obser = Observable.Interval(TimeSpan.FromSeconds(3)).TimeInterval();

            this.bBlock = new BatchBlock<T>(batchSize, new GroupingDataflowBlockOptions { BoundedCapacity = 1000000000 });
            this.bBlock.AsObservable<IEnumerable<T>>().Buffer(ts).Subscribe(_action);
        }

        public _TimerBatchBlockWrapper(TimeSpan ts, int batchSize, Func<IEnumerable<T>, Task> _action)
        {
            this.timeOut = ts;
            this.ct = new CancellationTokenSource();

            this.bBlock = new BatchBlock<T>(batchSize, new GroupingDataflowBlockOptions { BoundedCapacity = 1000000000, CancellationToken = ct.Token });
            this.aBlock = new ActionBlock<IEnumerable<T>>(_action);

            this.timerBlock = new TransformBlock<T, T>(val => { timer.Change(this.timeOut.Seconds, Timeout.Infinite); return val; });
            this.bBlock.LinkTo(aBlock);
            this.timerBlock.LinkTo(bBlock);
        }
        //public _TimerBatchBlockWrapper(TimeSpan ts, int batchSize, Action<IEnumerable<T>> _action)
        //{
        //    this.timeOut = ts;
        //    this.ct = new CancellationTokenSource();
        //    this.aBlock = new ActionBlock<IEnumerable<T>>(_action/*, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism=batchSize}*/);
        //    this.bBlock = new BatchBlock<T>(batchSize, new GroupingDataflowBlockOptions { BoundedCapacity = 1000000000, CancellationToken = ct.Token });
        //    this.timerBlock = new TransformBlock<T, T>(val => { timer.Change(this.timeOut.Seconds, Timeout.Infinite); return val; });
        //    this.bBlock.LinkTo(aBlock);
        //    this.timerBlock.LinkTo(bBlock);
        //}
        public void StartTriggerBatch()
        {
            this.timer = new Timer(_ =>
            {
                bBlock.TriggerBatch();
                //timer.Change(this.timeOut.Seconds, Timeout.Infinite);
            }, null, Timeout.Infinite, Timeout.Infinite);

            timer.Change(this.timeOut.Seconds, Timeout.Infinite);

        }

        public void StopTriggerBatch()
        {
            bBlock.Complete();
            aBlock.Complete();
            ct.Cancel();
            if (timer != null)
                timer.Dispose();
        }

        public async Task EnqueueAsync(T item)
        {
            await bBlock.SendAsync(item);
        }
        public void Enqueue(T item)
        {
            bBlock.Post(item);
        }
    }
}
