using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YmatouMQ.Common;
using YmatouMQ.Log;
using YmatouMQNet4;
using YmatouMQNet4.Core;

namespace YmatouMQPubllishConsoleApplication
{
    class Program
    {
        private static ILog log = LogFactory.GetLogger(LogFactory._LogType, "YmatouMQPubllishConsoleApplication");
        private static bool isKeyPress = false;
        static void Main(string[] args)
        {
            MessageBus.StartBusApplication();
            Console.CancelKeyPress += Console_CancelKeyPress;

            var i = 0;
            while (!isKeyPress)
            {
                var msg = "info" + (++i);
                MessageBus.Publish<string>(msg + "__" + i, "test", "test","c");
                log.Info("第{0}个消息{1}发送完成", i, msg);
                //System.Threading.Thread.Sleep(1000);
            }
            Console.Read();
        }

        static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            isKeyPress = true;
            MessageBus.StopBusApplication();
            Console.Read();
        }
    }
}
