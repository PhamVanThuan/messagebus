using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YmatouMessageBusClientNet4;

namespace PublishTest
{
   public class MQHttpPublish
    {
       public static void SendMessage(bool sleep)
       {
           Console.WriteLine("发送消息 {0}",30000);
           var r = new Random();
           for (var i = 0; i < 30000; i++)
           {
               MessageBusAgent.Publish("trading" //appid
                   , "trading_postpay" //code
                   , "mitest_A_" + DateTime.Now.ToString("yyyyMMddHHmm") + "_" + i //messageid
                   , new
                   {
                       //body
                       a = "add",
                       c = new { c = 2, f = new { c = new List<int> { 1, 2, 3, 4 } } }
                   }
                   );
               if (sleep)
               Thread.Sleep(r.Next(10, 500));
           }          
       }
    }
}
