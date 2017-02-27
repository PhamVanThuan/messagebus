using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MQMessageProtocols;
using Thrift.Protocol;
using Thrift.Transport;

namespace MQThrift.Clent
{
    class MQBusClient
    {
        private _MQMessageProtocols.Client client;
        private TSocket socket;
        public void Start()
        {
            try
            {
                socket = new TSocket("172.16.100.50", 2345);
                var protocol = new TBinaryProtocol(socket);
                client = new _MQMessageProtocols.Client(protocol);

                socket.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public void Stop()
        {
            socket.Dispose();
        }

        public _MQMessageProtocols.Client _client { get { return client; } }
    }
}
