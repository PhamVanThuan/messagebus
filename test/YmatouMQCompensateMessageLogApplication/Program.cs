using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YmatouMQ.CompensateMessageLog;

namespace YmatouMQCompensateMessageLogApplication
{
    class Program
    {
        static void Main(string[] args)
        {
//            var stime1 = DateTime.Now.AddHours(-1);
//            var endTime = DateTime.Now;
//            Console.WriteLine(stime1 + " -> " + endTime);
//            var totalMinutes = Convert.ToInt32(endTime.Subtract(stime1).TotalMinutes);
//            var pageCount = (totalMinutes/30) + (totalMinutes%30 > 0 ? 1 : 0);
//            Console.WriteLine(pageCount);
//
//            for (var i = 0; i <= pageCount; i++)
//            {
//               
//                var etime = stime1.AddMinutes(i*30);
//                var stime = etime.AddMinutes(-(30));
//                Console.WriteLine(stime + "->" + etime);
//            }
          
            try
            {
                Console.WriteLine("执行补单任务");
                CompensateMessageTask.Start();
                Console.WriteLine("执行结束");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            Console.Read();
        }
    }
}
