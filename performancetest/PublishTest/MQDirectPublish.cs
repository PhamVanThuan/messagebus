using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YmatouMQ.Common;
using YmatouMQ.Log;
using YmatouMQNet4.Core;

namespace PublishTest
{
   public  class MQDirectPublish
   {
       private static ILog log = LogFactory.GetLogger(LogFactory._LogType, "YmatouMQPubllishConsoleApplication");
       private static bool isKeyPress = false;
       public static void Publish()
       {
           try
           {
               Console.WriteLine("选择发送模式，1:同步，2：异步,3：buffer");
               var pubType = Console.ReadLine().Trim();
               Console.WriteLine("使用模式，" + pubType + ", 输入发布消息个数");
               var input = Console.ReadLine();
               var count = string.IsNullOrEmpty(input) ? 500000 : Convert.ToInt32(input);
               Console.WriteLine("1:单步测试(默认);2：并行测试");
               var type = Console.ReadLine();

               MessageBus.StartBusApplication();
               Console.CancelKeyPress += Console_CancelKeyPress;
               var watch = Stopwatch.StartNew();
               if (string.IsNullOrEmpty(type) || type == "1")
               {
                   var i = count;
                   while (i > 0)
                   {
                       var msg = "hell word " + (i);
                       //同步发送
                       if (pubType == "2")
                           MessageBus.PublishAsync<string>(msg, "test2", "gaoxu", Guid.NewGuid().ToString("N"));
                       else if (pubType == "3")
                       {
                           var async = MessageBus.PublishBufferAsync<string>(msg, "test2", "gaoxu", Guid.NewGuid().ToString("N"));
                       }
                       else
                           MessageBus.Publish(msg, "test2", "gaoxu", Guid.NewGuid().ToString("N"));
                       //Console.WriteLine("第{0}个消息{1}发送完成", i, msg);
                       i--;
                   }
               }
               else if (type == "2")
               {
                   Parallel.For(0, count, i =>
                   {
                       var msg = "hell word " + (i);
                       var async = MessageBus.PublishBufferAsync<string>(msg, "test2", "gaoxu", Guid.NewGuid().ToString("N"));
                   });
               }
               watch.Stop();
               var total = watch.ElapsedMilliseconds;
               var ops = count * 1000 / watch.ElapsedMilliseconds;
               Console.WriteLine("总共发送 {0} 个消息 ,总耗时 {1} 毫秒,平均每秒发送 {2} 个消息", count, total, ops);
               //MessageBus.PublishAsync<string>("hell word", "test", "A", "gg");
               //Console.WriteLine("额外发送一个消息");
               Console.Read();
           }
           catch (Exception ex)
           {
               Console.WriteLine(ex.ToString());
               Console.Read();
           }
       }
       static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
       {
           isKeyPress = true;
           MessageBus.StopBusApplication();
           Console.Read();
       }
    }
}
