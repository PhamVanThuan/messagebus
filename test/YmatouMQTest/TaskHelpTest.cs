using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using YmatouMQNet4.Core;
using YmatouMQ.Common.Extensions._Task;
using YmatouMQ.Common.Extensions;
using YmatouMQ.Common.Utils;
using System.Net.Http;
using System.Threading;

namespace YmatouMQTest
{
    [TestClass]
    public class TaskHelpTest
    {
        [TestMethod]
        public void ExecuteASynchronously()
        {
            Func<int> fn = () => 5;
            var result = fn.ExecuteASynchronously();
            Assert.AreEqual(5, result.Result);
            fn.ExecuteASynchronously().Then(n =>
            {
                Assert.AreEqual(5, n);
                return TaskHelpers.FromReturnVoid();
            }).GetResult(false, null);
        }
        [TestMethod]
        public void ActionTaskBatch()
        {
            TimerBatchBlockWrapper<int> tqb = new TimerBatchBlockWrapper<int>(3000, 3, s =>
            {
                foreach (var i in s)
                    Console.WriteLine(i);
            });
            for (var i = 0; i < 10; i++)
                tqb.Send(i);
            Assert.IsTrue(true);
        }

        [TestMethod]
        public async Task ForAsync()
        {
            var client = new HttpClient();
            var results = new Dictionary<string, string>();
            var urlList = new List<string>();
            urlList.Add("http://blogs.msdn.com/b/pfxteam/archive/2012/03/04/10277325.aspx");
            urlList.Add("http://stackoverflow.com/questions/19189275/asynchronously-and-parallelly-downloading-files");
            urlList.Add("https://msdn.microsoft.com/zh-cn/library/hh194782(v=vs.110).aspx");
            urlList.Add("http://blog.sina.com.cn/");
            urlList.Add("https://msdn.microsoft.com/zh-cn/library/Hh228609(v=VS.110).aspx");
            await urlList.ForEachAsync(url =>
                {
                    Console.WriteLine("thread id " + Thread.CurrentThread.ManagedThreadId);
                    return client.GetStringAsync(url);
                },
                (url, contents) =>
                {
                    Console.WriteLine("thread id2 " + Thread.CurrentThread.ManagedThreadId);
                    results.Add(url, contents);
                }
                );
        }
        [TestMethod]
        public async Task ForAsync2()
        {
            var client = new HttpClient();
            client.MaxResponseContentBufferSize = 1024 * 1024 * 10;
            var results = new Dictionary<string, string>();
            var urlList = new List<string>();
            urlList.Add("http://blogs.msdn.com/b/pfxteam/archive/2012/03/04/10277325.aspx");
            urlList.Add("http://stackoverflow.com/questions/19189275/asynchronously-and-parallelly-downloading-files");
            urlList.Add("https://msdn.microsoft.com/zh-cn/library/hh194782(v=vs.110).aspx");
            urlList.Add("http://blog.sina.com.cn/");
            urlList.Add("https://msdn.microsoft.com/zh-cn/library/Hh228609(v=VS.110).aspx");
            await urlList.ForEachAsync2(async url =>
            {
                Console.WriteLine("thread id " + Thread.CurrentThread.ManagedThreadId);
                var result = await client.GetStringAsync(url);
                return Tuple.Create(result, url);
            },
                r =>
                {
                    Console.WriteLine("thread id2 " + Thread.CurrentThread.ManagedThreadId + " url " + r.Item2);
                    results.Add(r.Item2, r.Item1);
                }
           );
        }
        [TestMethod]
        public async Task ForAsync3()
        {
            var handler = new HttpClientHandler();

            var client = new HttpClient(handler);
            client.MaxResponseContentBufferSize = 1024 * 1024 * 10;
            var results = new Dictionary<string, string>();
            var urlList = new List<string>();
            urlList.Add("http://blogs.msdn.com/b/pfxteam/archive/2012/03/04/10277325.aspx");
            urlList.Add("http://stackoverflow.com/questions/19189275/asynchronously-and-parallelly-downloading-files");
            urlList.Add("https://msdn.microsoft.com/zh-cn/library/hh194782(v=vs.110).aspx");
            urlList.Add("http://blog.sina.com.cn/");
            urlList.Add("https://msdn.microsoft.com/zh-cn/library/Hh228609(v=VS.110).aspx");
            var _url = new TestUrl();
            for (var i = 0; i < 3; i++)
            {
                await urlList.ForEachAsync2(async url =>
                {
                    //Console.WriteLine("thread id " + Thread.CurrentThread.ManagedThreadId);
                    _url.Url = url;
                    Console.WriteLine("thread id " + Thread.CurrentThread.ManagedThreadId);
                    var result = await Request(client, _url).ConfigureAwait(false);
                    //Console.WriteLine(url);
                    return Tuple.Create(result, url);
                },
                r =>
                {
                    Console.WriteLine("thread id2 " + Thread.CurrentThread.ManagedThreadId + " url " + r.Item2);
                    results[r.Item2] = string.Empty;
                }
               );
                Console.WriteLine("________________");
            }
        }
        [TestMethod]
        public void semaphore()
        {
            var semaphore = new SemaphoreSlim(3, 3);
            var now = DateTime.Now;
            while (DateTime.Now.Subtract(now).TotalMinutes <= 1)
            {
                for (var i = 0; i < 5; i++)
                {
                    ThreadPool.QueueUserWorkItem(async o =>
                    {
                        Console.WriteLine("thread id {0} begins and waits for the semaphore.{1}",
                                 Thread.CurrentThread.ManagedThreadId
                                 , semaphore.CurrentCount);
                        await semaphore.WaitAsync();
                        try
                        {
                            Console.WriteLine("thread id {0} enter semaphore.",
                                 Thread.CurrentThread.ManagedThreadId);
                            await Task.Delay(500);
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    }, null);
                }
            }
            Console.WriteLine("end ..");
        }
        private static async Task<string> Request(HttpClient client, TestUrl _url)
        {
            if (_url.Url == "http://blogs.msdn.com/b/pfxteam/archive/2012/03/04/10277325.aspx")
                await Task.Delay(500);
            return await client.GetStringAsync(_url.Url).ConfigureAwait(false);
        }

        class TestUrl
        {
            public string Url { get; set; }
        }
    }
}
