using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using YmatouMQNet4.Core;

namespace YmatouMQServerServicestack
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = Dns.GetHostAddresses(Dns.GetHostName()).Where(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).First().ToString();
            var uri = string.Format("http://mqserver2.ymatou.com:2345/", host);
            new MQServerHost()
               .Init()
               .Start(uri);
            MessageBus.StartBusApplication();
            Console.WriteLine("start ok.." + uri);
            Console.Read();
        }
    }
}
