using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YmatouMQ.CompensateMessageLog;
using YmatouMQ.MessageCompensateService;

namespace YmatouMQ.MessageCompensate.Server.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            MessageCompensateTaskService.Start();
            CompensateMessageStatusLog.Start();
            //WaitStartDone(service);
            Console.WriteLine("已启动消息补偿服务");
            Console.Read();
        }

        private static void WaitStartDone(Task service)
        {
            try
            {
                Task.WaitAll(service);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
