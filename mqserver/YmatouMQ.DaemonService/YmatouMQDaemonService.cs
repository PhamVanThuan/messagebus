using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace YmatouMQ.DaemonService
{
    public partial class YmatouMQDaemonService : ServiceBase
    {
        public YmatouMQDaemonService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _YmatouMQDaemonService.Start();
        }

        protected override void OnStop()
        {
            _YmatouMQDaemonService.Stop();
        }
    }
}
