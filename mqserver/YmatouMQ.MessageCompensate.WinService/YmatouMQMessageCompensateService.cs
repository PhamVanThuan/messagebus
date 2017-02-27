using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using YmatouMQ.CompensateMessageLog;
using YmatouMQ.MessageCompensateService;

namespace YmatouMQ.MessageCompensate.WinService
{
    public partial class YmatouMQMessageCompensateService : ServiceBase
    {
        public YmatouMQMessageCompensateService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            //启动补单任务
            MessageCompensateTaskService.Start();
            //启动定时扫描消息日志状态任务
            CompensateMessageStatusLog.Start();
        }

        protected override void OnStop()
        {
            MessageCompensateTaskService.Stop();
            CompensateMessageStatusLog.Stop();
        }
    }
}
