using Microsoft.Owin.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Owin;
using YmatouMQServerConsoleApp.owin;
using Nowin;
using System.Net;
using YmatouMQNet4.Core;

namespace YmatouMQServerConsoleApp
{
    public class MQServerBootStart_Owin2
    {
        public static void Run()
        {

            var appBuilder = new AppBuilder();
            //appBuilder.MaxBandwidthPerRequest(0);
            //appBuilder.MaxBandwidthGlobal(0);
            //appBuilder.MaxConcurrentRequests(10000);

            appBuilder.Use<RequestMonitorMiddleware>();
            appBuilder.Use<MessageRequestHandler>();
            appBuilder.Run((owinContext) =>
            {
                owinContext.Response.ContentType = "text/plain";
                return owinContext.Response.WriteAsync("welcome ymatou mq server..");
            });

            var server = ServerBuilder.New()
               .SetEndPoint(new IPEndPoint(IPAddress.Loopback, 2345))
               .SetOwinApp(appBuilder.Build());

            using (server.Build())
            {
                server.Start();
                MessageBus.StartBusApplication();
                Console.WriteLine("mq bus application start seccuss...");
                Console.ReadLine();
            }
        }
    }
}
