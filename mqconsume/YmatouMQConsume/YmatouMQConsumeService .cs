using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
//using YmatouMQ.SubscribeAppDomain;
using YmatouMQ.SubscribeAppDomainSingle;
using YmatouMQNet4;

namespace YmatouMQConsume
{
    public partial class MqConsumeService : ServiceBase
    {
        public MqConsumeService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _MessageBusSubscribeSetup.Start();
            //MessageBusSubscribeSetup.Start();
        }

        protected override void OnStop()
        {
            _MessageBusSubscribeSetup.Stop();
            //MessageBusSubscribeSetup.Stop();
        }
    }
}
