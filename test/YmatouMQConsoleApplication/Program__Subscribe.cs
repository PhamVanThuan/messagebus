using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YmatouMQNet4;
using YmatouMQNet4.Core;

namespace YmatouMQConsoleApplication
{
    class Program__Subscribe
    {
        static void Main(string[] args)
        {
            //
            MessageBus.StartBusApplication();
            Console.WriteLine("MQ BUs 启动完成");
            //MessageBus.Subscribe<string>("test", "A", new TestMessageHandler());
            Console.Read();
            MessageBus.StopBusApplication();
            Console.WriteLine("停止完成");
            Console.Read();
        }
    }
}
