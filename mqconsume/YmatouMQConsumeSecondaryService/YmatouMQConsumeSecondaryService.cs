using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
//using YmatouMQ.SubscribeAppDomain;
using YmatouMQ.SubscribeAppDomainSingle;

namespace YmatouMQConsumeSecondaryService
{
    public partial class YmatouMQConsumeSecondaryService : ServiceBase
    {
        public YmatouMQConsumeSecondaryService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _MessageBusSubscribeSetup.Start();
        }

        protected override void OnStop()
        {
            _MessageBusSubscribeSetup.Stop();
        }
    }
}
