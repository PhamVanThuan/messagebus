using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YmatouMQMessageMongodb.Domain.Module;
using YmatouMQNet4;
using YmatouMQ.Connection;
using YmatouMQNet4.Core;
using YmatouMQNet4.Utils;
using YmatouMQ.Common.Utils;
using YmatouMQMessageMongodb.AppService;

namespace YmatouMQConnectionConsoleApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            BatchWriteLogTest();
            //BatchInsertMongodbTest();
            // Batch_test2();
            Console.WriteLine("完成");
            Console.Read();
        }
        public static void BatchWriteLogTest()
        {
            //log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "log4net.config")));
            //MqBatchLog log = new MqBatchLog("YmatouMQConnectionConsoleApplication.Program");
            //MqBatchLog.StartJob();

            //for (var i = 0; i < 1000; i++)
            //{
            //    log.Debug("ssssssss" + i);
            //}
        }
        private static void BatchInsertMongodbTest()
        {
            Task.Run(() =>
            {
                for (var i = 0; i < 3005; i++)
                {
                    var result = MessageAppService_TimerBatch.Instance.PostMessageAsync(new MQMessage("test2", "t1", "0.0.0.0", Guid.NewGuid().ToString("N"), "hell mongodb batch insert " + i, null));
                    // await Task.Delay(500);
                }
            });           
        }

        private static void Batch_test2()
        {
            TimerBatchBlockWrapper<int> tbbw = new TimerBatchBlockWrapper<int>(3000, 10, data =>
            {
                Console.WriteLine("batch count" + data.Count() + "_" + DateTime.Now);
                foreach (var d in data)
                    Console.WriteLine(d + "__" + DateTime.Now + "__" + System.Threading.Thread.CurrentThread.ManagedThreadId);


            }
            , max: 10
            , errorHandle: err => Console.WriteLine(err.ToString())
            , sendTimeOutCallback: () => Console.WriteLine("发送数据超时"));
            tbbw.ReceiveAsync();



            Task.Run(async () =>
            {
                while (true)
                {
                    var cts = new CancellationTokenSource(3000);
                    var token = cts.Token;
                    token.Register(() => Console.WriteLine("超时了"));
                    for (var i = 0; i < 10; i++)
                    {
                        var s = tbbw.SendAsync(i, token);

                    }
                    await Task.Delay(500);

                }
            });

        }

    }
}
