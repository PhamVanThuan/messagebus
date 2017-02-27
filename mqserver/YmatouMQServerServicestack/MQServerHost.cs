using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ServiceStack;
using YmatouMQNet4.Dto;

namespace YmatouMQServerServicestack
{
    public class MQServerHost : AppHostHttpListenerBase
    {
        public MQServerHost() : base("mq server", typeof(MQServerHost).Assembly) { }

        public override void Configure(Funq.Container container)
        {
            ThreadPool.SetMinThreads(250, 250);
            ServicePointManager.DefaultConnectionLimit = Int16.MaxValue;

            Routes.Add<MessageDto>("/bus/Message/", "POST");
            SetConfig(new HostConfig
            {
                DebugMode = false,
            });
        }
    }
}
