using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YmatouMQNet4.Configuration;
using YmatouMQNet4.Utils;
using YmatouMQ.Common.Extensions;
using YmatouMQ.Common.Extensions.Serialization;
using YmatouMQ.Common.Utils;

namespace TaskQueueTest
{
    class Program
    {
        static void Main(string[] args)
        {
            _TimerBatchRetryEnQueueWrapper2<A> queue =
                new _TimerBatchRetryEnQueueWrapper2<A>(new _TimerBatchRetryEnQueueWrapper2<A>.Strategy()
                {
                    action = (a, token) => Handle(a, token),
                    addTimeOutMillisecondes = 3000,
                    batch_Size = 10,
                    concurrent = 1,
                    errorHandle = ex => Console.WriteLine(ex.ToString()),
                    max = 1000,
                    timer_CycleMilliseconds = 1000
                });

            queue.Start();
            int index = 0;
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    queue.SendAsync(new A() {id = Interlocked.Increment(ref index)})
                        .ContinueWith(t => { }, TaskContinuationOptions.OnlyOnRanToCompletion);
                    Thread.Sleep(500);
                }
            });
            Console.Read();
        }

        private static Task<IEnumerable<A>> Handle(IEnumerable<A> arr,CancellationToken token)
        {

            Stopwatch stopwatch = Stopwatch.StartNew();
            IList<A> list = new List<A>();
#pragma warning disable 1998
            var task = arr.ForEachAsync(Environment.ProcessorCount,async a =>
#pragma warning restore 1998
            {
                if (a.id%2 == 0)
                {
                    list.Add(a);
                    Console.WriteLine("重新入队成功 \t" + a.id);
                }
                else
                {
                    Console.WriteLine(a.id);
                }
            }).ContinueWith(t => list.AsEnumerable(), token, TaskContinuationOptions.OnlyOnRanToCompletion,TaskScheduler.Current);
            stopwatch.Stop();
            Console.WriteLine("执行完成耗时：{0:N0}", stopwatch.ElapsedMilliseconds);
            return task;
        }
    }

    class  A
    {
        public int id { get; set; }
    }
}
