using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using YmatouMQServerConsoleApp;

namespace YmatouMQPublishSecondaryService
{
    public partial class YmatouMQPublishSecondaryService : ServiceBase
    {
        public YmatouMQPublishSecondaryService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            MQHost.Start();
        }

        protected override void OnStop()
        {
            MQHost.Stop();
        }
    }
}
