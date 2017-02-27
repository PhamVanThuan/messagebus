using System;

namespace YmatouMQServerConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            MQHost.Start();
            //MQServerBootStart_Owin2.Run();
            Console.WriteLine("mq web api host启动成功");
            Console.Read();
            MQHost.Stop();
            Console.WriteLine("mq web api host top");
            Console.Read();
        }
    }
}
