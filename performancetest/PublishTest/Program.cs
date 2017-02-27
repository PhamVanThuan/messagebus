using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YmatouMQ.Common;
using YmatouMQ.Log;
using YmatouMQNet4;
using YmatouMQNet4.Core;

namespace PublishTest
{
    class Program
    {
      
     
        static void Main(string[] args)
        {
             MQHttpPublish.SendMessage(false);
            Console.Read();
        }

       
    }
}
