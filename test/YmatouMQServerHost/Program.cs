using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Topshelf;
using YmatouMessageBusClientNet4;

namespace YmatouMQServerHost
{
    class Program
    {
        private static bool isRun;
        private  bool _loop = true;
        private object obj = new object();
        static void Main(string[] args)
        {
//            Program test1 = new Program();
//            // Set _loop to false on another thread
//            new Thread(() => 
//            {
//                Monitor.Enter(test1.obj);
//                test1._loop = true;               
//                Monitor.Pulse(test1.obj);
//                Monitor.Exit(test1.obj);
//            }).Start();
//            new Thread(() =>
//            {
//                Monitor.Enter(test1.obj);
//                test1._loop = false;
//                Monitor.Pulse(test1.obj);
//                Monitor.Exit(test1.obj);
//            }).Start();
//            // Poll the _loop field until it is set to false
//            while (test1._loop)
//            {
//                Monitor.Enter(test1.obj);
//                Monitor.Wait(test1.obj);
//                var l = test1._loop;
//                Console.WriteLine(l);
//                Monitor.Exit(test1.obj);
//            };
//            Console.WriteLine(test1._loop);
//            Console.Read();
//            // The previous loop may never terminate
//            //ThreadSleep();
//            //SemaphoreSlimTest();
        }
        private static void ThreadSleep()
        {
            isRun = true;
            var syncThread = new Thread(_o =>
              {
                  while (isRun)
                  {
                      Thread.Sleep(30000);
                      //using (var monitor = new MethodMonitor(log, 1000, "完成一次配置维护"))
                      {
                          //同步配置&更新配置                
                          //TrySyncDefaultCfg();
                          //TrySyncAppCfg();
                          Console.WriteLine("sync cfg end,status ok...isRun " + isRun + " time " + DateTime.Now);
                      }
                  }
              }) { IsBackground = true };
            syncThread.Start();
            Console.Read();
        }
        private static void SemaphoreSlimTest()
        {
            var semaphore = new SemaphoreSlim(3, 3);
            var now = DateTime.Now;
            var totalMinutes = 0.0;
            while ((totalMinutes = DateTime.Now.Subtract(now).TotalMinutes) < 1)
            {
                for (var i = 0; i < 5; i++)
                {
                    Task.Factory.StartNew(async () =>
                    {
                        Console.WriteLine("线程ID {0} 等待，信号量 {1},时间 {2}",
                                 Thread.CurrentThread.ManagedThreadId
                                 , semaphore.CurrentCount
                                 , DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        //wait 减少计数器
                        //当计数为零时，后续请求阻止，直到其他线程释放信号量
                        await semaphore.WaitAsync().ConfigureAwait(false);
                        //semaphore.Wait();
                        try
                        {

                            Console.WriteLine("线程ID {0} 进入 ，信号量 {1},时间 {2},{3}",
                                 Thread.CurrentThread.ManagedThreadId
                                 , semaphore.CurrentCount
                                 , DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                                 , totalMinutes);
                            await Task.Delay(2000);
                        }
                        finally
                        {
                            //Release 增加计数器
                            semaphore.Release();
                        }
                    }, CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
                }
                Console.WriteLine("--------------------------------");
                Thread.Sleep(1000);
            }
            Console.WriteLine("end ..");
            Console.Read();

            //web api selfHost
            //new MQServerAppHost().Start();

            // MessageBusAgent.TryInitBusAgentService();
            //for (var i = 0; i < 100; i++)
            //{
            //    MessageBusAgent.Publish("test2", "liguo", i.ToString(), "AAAA");
            //    System.Threading.Thread.Sleep(200);
            //    //var r = MessageBusAgent.PublishAsync("test2"
            //    //                        , "liguo"
            //    //                        , "i_" + Guid.NewGuid().ToString("N")
            //    //                        , "a");
            //}
            //Console.WriteLine("发送完成");
            //// MessageBusAgent.TryStopBusAgentService();
            //Console.Read();
        }
    }
}
