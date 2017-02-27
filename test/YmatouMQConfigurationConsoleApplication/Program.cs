using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YmatouMQNet4.Configuration;
using YmatouMQ.Common.Utils;
using YmatouMQ.Common.Extensions.Serialization;
using YmatouMQ.ConfigurationSync;

namespace YmatouMQConfigurationConsoleApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            //
            Console.WriteLine("启动配置测试,1:测试 MQAPP配置，2：测试MQ APP Domain 配置");
            var testType = Console.ReadLine();
            if (testType == "1")
            {
                Console.WriteLine("测试 MQAPP配置");
                test_mq_app_cfg();
            }
            else if (testType == "2")
            {
                Console.WriteLine("测试MQ APP Domain 配置");
                test_mq_app_domain_cfg();
            }
            Console.Read();
        }
        private static void test_mq_app_domain_cfg()
        {
            var cts = new CancellationTokenSource();
            var token = cts.Token;

            Console.CancelKeyPress += (o, e) =>
            {
                if (e.Cancel)
                {
                    cts.Cancel();
                }
                Console.Read();
            };
            var cfg = AppdomainConfigurationManager.Builder;
            cfg.Start();
            try
            {

                for (var i = 0; i < 3; i++)
                {
                    Task.Factory.StartNew(() =>
                   {
                       while (true)
                       {
                           if (token.IsCancellationRequested)
                               token.ThrowIfCancellationRequested();
                           Console.ForegroundColor = ConsoleColor.Yellow;
                           foreach (var item in cfg.GetAllAppdomain())
                           {
                               var time = string.Format("{0}:{1}:{2}", DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Millisecond);
                               Console.WriteLine("时间->" + time + "：应用->" + item.Key + "：版本->" + item.Value.Version + "：线程->" + Thread.CurrentThread.ManagedThreadId);

                           }
                           Console.WriteLine("------------------------------------");
                           Task.Delay(2000).Wait();
                       }
                   });
                }
            }
            catch (OperationCanceledException ex)
            {
                Console.WriteLine("操作取消");
            }
            Console.Read();
        }
        private static void test_mq_app_cfg()
        {
            var cfg = MQMainConfigurationManager.Builder;

            var cts = new CancellationTokenSource();
            var token = cts.Token;

            Console.CancelKeyPress += (o, e) =>
            {
                if (e.Cancel)
                {
                    cts.Cancel();
                }
                Console.Read();
            };

            try
            {
                Task.Factory.StartNew(() =>
                {
                    while (true)
                    {
                        if (token.IsCancellationRequested)
                            token.ThrowIfCancellationRequested();
                        else
                        {
                            foreach (var item in cfg.GetConfiguration())
                            {
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                var time = string.Format("{0}:{1}:{2}", DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Millisecond);
                                Console.WriteLine("时间->" + time + "：应用->" + item.Key + "：版本->" + item.Value.Version);

                            }
                            Console.WriteLine("------------------------------------");
                        }
                        Task.Delay(2000).Wait();
                    }
                }, token);
            }
            catch (OperationCanceledException ex)
            {
                Console.WriteLine("操作取消");
            }
            cfg.Start();
        }
    }
}
