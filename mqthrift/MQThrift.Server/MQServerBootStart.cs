using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MQMessageProtocols;
using MQThrift.Server.ServerHandler;
using Thrift.Server;
using Thrift.Transport;

namespace MQThrift.Server
{
    public class MQServerBootStart
    {
        public static void Start()
        {
            var handler = new MQMessageBusServerHandler();
            var processor = new _MQMessageProtocols.Processor(handler);

            TServerTransport transport = new TServerSocket(2345);
            TServer server = new TThreadPoolServer(processor, transport);
            YmatouMQNet4.Core.MessageBus.StartBusApplication();
            Console.WriteLine("mq start ok..");
            server.Serve();
            Console.WriteLine("服务启动成功。。。");
            Console.ReadLine();
        }
    }
}
